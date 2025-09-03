using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Signix.API.Models;
using Signix.API.Models.Requests;
using Signix.API.Models.Responses;
using Signix.Entities.Context;
using Signix.Entities.Entities;

namespace Signix.API.Infrastructure;

public class SigningRoomService : ISigningRoomService
{
    private readonly SignixDbContext _context;
    private readonly ILogger<SigningRoomService> _logger;

    public SigningRoomService(SignixDbContext context, ILogger<SigningRoomService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<SigningRoomGetByIdResponse>> GetByIdAsync(int id)
    {
        try
        {
            var signingRoom = await _context.SigningRooms
                .Include(sr => sr.Notary)
                .Include(sr => sr.Client)
                .Include(sr => sr.Documents)
                .Include(sr => sr.Signers)
                    .ThenInclude(s => s.Designation)
                .FirstOrDefaultAsync(sr => sr.Id == id);

            if (signingRoom == null)
            {
                return Result<SigningRoomGetByIdResponse>.NotFound($"No signing room found with ID {id}");
            }
            var response = new SigningRoomGetByIdResponse
            {
                Id = signingRoom.Id,
                Name = signingRoom.Name,
                Description = signingRoom.Description,
                OriginalPath = signingRoom.OriginalPath,
                SignedPath = signingRoom.SignedPath,
                NotaryId = signingRoom.NotaryId,
                StartedAt = signingRoom.StartedAt,
                CompletedAt = signingRoom.CompletedAt,
                CreatedAt = signingRoom.CreatedAt,
                ClientName = signingRoom.Client?.Name ?? "",
                ModifiedBy = signingRoom.ModifiedBy,
                Signers = signingRoom.Signers?.Select(s => new ListSignignRoomsSignerResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Role = s.Designation?.Name ?? string.Empty,
                    Email = s.Email
                }).ToList() ?? new List<ListSignignRoomsSignerResponse>(),
                Documents = signingRoom.Documents?.Select(d => new GetByIdSignignRoomDocumentResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    FileSize = d.FileSize,
                    FileType = d.FileType
                }).ToList() ?? new List<GetByIdSignignRoomDocumentResponse>()
            };
            return Result<SigningRoomGetByIdResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving signing room with ID {Id}", id);
            return Result<SigningRoomGetByIdResponse>.Error("An error occurred while retrieving the signing room");
        }
    }

    public async Task<PagedResult<List<SigningRoomListResponse>>> GetAllAsync(SigningRoomQuery query)
    {
        try
        {
            var queryable = _context.SigningRooms
                .Include(sr => sr.Notary)
                .Include(sr => sr.Client)
                .Include(sr => sr.Documents)
                    .ThenInclude(d => d.DocumentStatus)
                .Include(sr => sr.Signers)
                    .ThenInclude(s => s.Designation)
                .AsQueryable();

            // Apply filters
            if (query.NotaryId.HasValue)
                queryable = queryable.Where(sr => sr.NotaryId == query.NotaryId.Value);

            if (!string.IsNullOrEmpty(query.Name))
                queryable = queryable.Where(sr => sr.Name.Contains(query.Name));

            if (query.CreatedAfter.HasValue)
                queryable = queryable.Where(sr => sr.CreatedAt >= query.CreatedAfter.Value);

            if (query.CreatedBefore.HasValue)
                queryable = queryable.Where(sr => sr.CreatedAt <= query.CreatedBefore.Value);

            var totalCount = await queryable.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            var signingRooms = await queryable
                .OrderByDescending(sr => sr.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var pagedInfo = new PagedInfo(
                pageNumber: query.Page,
                pageSize: query.PageSize,
                totalRecords: totalCount,
                totalPages: totalPages);
            var responseList = signingRooms.Select(sr => new SigningRoomListResponse
            {
                Id = sr.Id,
                Name = sr.Name,
                Description = sr.Description,
                OriginalPath = sr.OriginalPath,
                SignedPath = sr.SignedPath,
                NotaryName = sr.Notary.FirstName + ' ' + sr.Notary.LastName,
                ClientName = sr.Client.Name,
                CreatedAt = sr.CreatedAt,
                StartedAt = sr.StartedAt,
                CompletedAt = sr.CompletedAt,
                SignersCount = sr.Signers?.Count ?? 0,
                DocumentsCount = sr.Documents?.Count ?? 0,
            }).ToList();            // Create a successful result and then convert to PagedResult
            var successResult = Result<List<SigningRoomListResponse>>.Success(responseList);
            return new PagedResult<List<SigningRoomListResponse>>(pagedInfo, successResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving signing rooms");
            return (PagedResult<List<SigningRoomListResponse>>)PagedResult<List<SigningRoomListResponse>>.Error("An error occurred while retrieving signing rooms");
        }
    }

    public async Task<Result<int>> RegisterAsync(RegisterSigningRoomRequest request)
    {
        //return Result<int>.Error("Not implemented");
        try
        {
            var notaryId = await _context.Users.Where(u => u.Email == request.NotaryEmail).SingleOrDefaultAsync();
            if (notaryId == null)
            {
                return Result<int>.Error("Notary does not exists");
            }
            var client = await _context.Clients.Where(c => c.Name.ToLower() == request.Client.ToLower()).SingleOrDefaultAsync();
            if (client == null)
            {
                return Result<int>.Error("Client does not exists");
            }
            var pendingStatus = await _context.DocumentStatuses.Where(ds => ds.Name == Meta.DocumentStatus.Pending).SingleOrDefaultAsync();
            SigningRoom signingRoom = new SigningRoom
            {
                CreatedBy = notaryId.Id,
                Description = request.Description,
                MetaData = request.MetaData,
                SignTags = request.SignTags,
                Name = request.Name,
                NotaryId = notaryId.Id,
                OriginalPath = request.OriginalPath,
                ClientId = client.Id,
            };
            request.Documents.ForEach(doc =>
            {
                signingRoom.Documents.Add(new Document
                {
                    Name = doc.Name,
                    DocTags = doc.DocTags,
                    FileSize = doc.FileSize,
                    FileType = doc.FileType,
                    DocumentStatusId = pendingStatus.Id,
                    SigningRoomId = signingRoom.Id,
                    //ClientId = 1, // TODO: Replace with actual client ID
                    Description = doc.Description
                });
            });
            foreach (var signer in request.Signers)
            {
                var designation = await _context.Designations
                    .Where(d => d.Name.ToLower() == signer.Designation.ToLower() && d.IsActive)
                    .FirstOrDefaultAsync();

                signingRoom.Signers.Add(new Signer
                {
                    Name = signer.Name,
                    Email = signer.Email,
                    DesignationId = designation.Id,
                    SigningRoomId = signingRoom.Id,
                });
            }

            await _context.SigningRooms.AddAsync(signingRoom);
            await _context.SaveChangesAsync();
            return Result<int>.Created(signingRoom.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering signing room");
            return Result<int>.Error("An error occurred while registering the signing room");
        }
    }
    public async Task<Result<SigningRoom>> CreateAsync(SigningRoomCreateRequest request)
    {
        try
        {
            // Validate notary exists
            var notaryExists = await _context.Users.AnyAsync(u => u.Id == request.NotaryId);
            if (!notaryExists)
            {
                return Result<SigningRoom>.Invalid(new ValidationError
                {
                    Identifier = nameof(request.NotaryId),
                    ErrorMessage = $"No user found with ID {request.NotaryId}"
                });
            }

            var signingRoom = new SigningRoom
            {
                Name = request.Name,
                Description = request.Description,
                OriginalPath = request.OriginalPath,
                NotaryId = request.NotaryId,
                CreatedBy = request.CreatedBy,
                SignTags = request.SignTags,
                MetaData = request.MetaData,
                CreatedAt = DateTime.UtcNow
            };

            _context.SigningRooms.Add(signingRoom);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(signingRoom)
                .Reference(sr => sr.Notary)
                .LoadAsync();

            return Result<SigningRoom>.Created(signingRoom);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating signing room");
            return Result<SigningRoom>.Error("An error occurred while creating the signing room");
        }
    }

    public async Task<Result<SigningRoom>> UpdateAsync(SigningRoomUpdateRequest request)
    {
        try
        {
            var signingRoom = await _context.SigningRooms
                .Include(sr => sr.Notary)
                .FirstOrDefaultAsync(sr => sr.Id == request.Id);

            if (signingRoom == null)
            {
                return Result<SigningRoom>.NotFound($"No signing room found with ID {request.Id}");
            }

            // Validate notary exists
            var notaryExists = await _context.Users.AnyAsync(u => u.Id == request.NotaryId);
            if (!notaryExists)
            {
                return Result<SigningRoom>.Invalid(new ValidationError
                {
                    Identifier = nameof(request.NotaryId),
                    ErrorMessage = $"No user found with ID {request.NotaryId}"
                });
            }

            // Update properties
            signingRoom.Name = request.Name;
            signingRoom.Description = request.Description;
            signingRoom.OriginalPath = request.OriginalPath;
            signingRoom.SignedPath = request.SignedPath;
            signingRoom.NotaryId = request.NotaryId;
            signingRoom.StartedAt = request.StartedAt;
            signingRoom.CompletedAt = request.CompletedAt;
            signingRoom.ModifiedBy = request.ModifiedBy;
            signingRoom.MetaData = request.MetaData;
            signingRoom.SignTags = request.SignTags;

            await _context.SaveChangesAsync();

            return Result<SigningRoom>.Success(signingRoom);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating signing room with ID {Id}", request.Id);
            return Result<SigningRoom>.Error("An error occurred while updating the signing room");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var signingRoom = await _context.SigningRooms.FindAsync(id);

            if (signingRoom == null)
            {
                return Result.NotFound($"No signing room found with ID {id}");
            }

            _context.SigningRooms.Remove(signingRoom);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting signing room with ID {Id}", id);
            return Result.Error("An error occurred while deleting the signing room");
        }
    }
}