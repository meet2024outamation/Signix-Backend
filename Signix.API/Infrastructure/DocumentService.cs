using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Signix.API.Infrastructure.Messaging;
using Signix.API.Models;
using Signix.API.Models.Messages;
using Signix.API.Models.Requests;
using Signix.API.Models.Responses;
using Signix.Entities.Context;

namespace Signix.API.Infrastructure;

public class DocumentService : IDocumentService
{
    private readonly SignixDbContext _context;
    private readonly ILogger<DocumentService> _logger;
    private readonly IRabbitMQService _rabbitMqService;

    public DocumentService(
        SignixDbContext context,
        ILogger<DocumentService> logger,
        IRabbitMQService rabbitMqService)
    {
        _context = context;
        _logger = logger;
        _rabbitMqService = rabbitMqService;
    }

    public async Task<PagedResult<List<ListDocumentResponse>>> GetAllAsync(DocumentQuery query)
    {
        try
        {
            var queryable = _context.Documents
                .Include(d => d.DocumentStatus)
                .Include(d => d.SigningRoom)
                .AsQueryable();

            // Apply filters
            if (query.SigningRoomId.HasValue)
                queryable = queryable.Where(d => d.SigningRoomId == query.SigningRoomId.Value);

            if (query.DocumentStatusId.HasValue)
                queryable = queryable.Where(d => d.DocumentStatusId == query.DocumentStatusId.Value);

            if (!string.IsNullOrEmpty(query.Name))
                queryable = queryable.Where(d => d.Name.Contains(query.Name));

            if (!string.IsNullOrEmpty(query.FileType))
                queryable = queryable.Where(d => d.FileType.Contains(query.FileType));

            var totalCount = await queryable.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            var documents = await queryable
                .OrderBy(d => d.Name)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(d => new ListDocumentResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    FileSize = d.FileSize,
                    FileType = d.FileType,
                    Status = d.DocumentStatus != null ? d.DocumentStatus.Name : string.Empty,
                    SigningRoom = d.SigningRoom != null ? d.SigningRoom.Name : string.Empty,
                    OriginalPath = d.SigningRoom != null ? Path.Combine(d.SigningRoom.OriginalPath, d.Name) : string.Empty,
                    SignedPath = d.SigningRoom != null ? Path.Combine(d.SigningRoom.SignedPath, d.Name)
                                    : string.Empty
                })
                .ToListAsync();

            var pagedInfo = new PagedInfo(
                pageNumber: query.Page,
                pageSize: query.PageSize,
                totalRecords: totalCount,
                totalPages: totalPages);

            var successResult = Result<List<ListDocumentResponse>>.Success(documents);
            return new PagedResult<List<ListDocumentResponse>>(pagedInfo, successResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents");
            var errorResult = Result<List<ListDocumentResponse>>.Error("An error occurred while retrieving documents");
            return new PagedResult<List<ListDocumentResponse>>(new PagedInfo(1, 10, 0, 0), errorResult);
        }
    }


    public async Task<Result<int>> SignDocumentsAsync(SignDocumentRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Starting document signing process. SigningRoomId: {SigningRoomId}, DocumentIds: [{DocumentIds}]",
                request.SignningRoomId, string.Join(", ", request.DocumentIds));

            var signingRoom = await _context.SigningRooms
                .Include(sr => sr.Documents)
                    .ThenInclude(d => d.DocumentStatus)
                .Include(sr => sr.Client)
                .Include(sr => sr.Signers)
                    .ThenInclude(s => s.Designation)
                .FirstOrDefaultAsync(sr => sr.Id == request.SignningRoomId);

            if (signingRoom == null)
            {
                _logger.LogWarning("Signing room not found. SigningRoomId: {SigningRoomId}", request.SignningRoomId);
                return Result<int>.NotFound("No signing room found to sign.");
            }
            if (signingRoom.CompletedAt != null)
            {
                _logger.LogWarning("Signing room already completed. SigningRoomId: {SigningRoomId}", request.SignningRoomId);
                return Result<int>.Conflict("Signing room already completed.");
            }

            var documents = signingRoom.Documents
                .Where(d => request.DocumentIds.Contains(d.Id))
                .ToList();

            if (!documents.Any())
            {
                _logger.LogWarning("No documents found for signing. SigningRoomId: {SigningRoomId}", request.SignningRoomId);
                return Result<int>.NotFound("No documents found to sign.");
            }

            // Extract sign data from SignTags and signers
            var signTags = signingRoom.SignTags ?? new Dictionary<string, object>();
            Dictionary<string, string> signData = new Dictionary<string, string>();
            foreach (var item in signTags)
            {
                var signer = signingRoom.Signers.FirstOrDefault(s => s.Name == item.Value.ToString());
                signData.Add(item.Key, signer?.SignatureData ?? string.Empty);
            }
            var documentInfoList = documents.Select(doc => new SignedDocumentInfo
            {
                Id = doc.Id,
                Name = doc.Name,
                Description = doc.Description,
                FileType = doc.FileType,
                FileSize = doc.FileSize,
                DocTags = doc.DocTags,
                ClientName = signingRoom.Client?.Name,
                DocumentStatus = doc.DocumentStatus?.Name
            }).ToList();

            var signersInfoList = signingRoom.Signers.Select(signer => new SignedSignerInfo
            {
                Name = signer.Name,
                Email = signer.Email,
                Designation = signer.Designation?.Name ?? string.Empty,
                SignedAt = signer.SignedAt ?? DateTime.UtcNow,
                Base64SignData = signer.SignatureData ?? string.Empty
            }).ToList();

            var rabbitMqMessage = new DocumentSignedMessage
            {
                SigningRoomId = request.SignningRoomId,
                SignedDocuments = documentInfoList,
                SignData = signData,
                Signers = signersInfoList,
                SignedPath = signingRoom.SignedPath,
                OriginalPath = signingRoom.OriginalPath,
                Timestamp = DateTime.UtcNow,
                EventType = "DocumentSigned",
                CorrelationId = Guid.NewGuid().ToString()
            };

            signingRoom.StartedAt = DateTime.UtcNow;
            _context.SigningRooms.Update(signingRoom);
            await _context.SaveChangesAsync();

            // Publish to RabbitMQ - but don't update document status yet
            try
            {
                await _rabbitMqService.PublishAsync(Meta.RabbitMQ.QueueName, rabbitMqMessage);
                _logger.LogInformation("Published document signed message to RabbitMQ. CorrelationId: {CorrelationId}",
                    rabbitMqMessage.CorrelationId);
                //var signedStatusId = await _context.DocumentStatuses
                //.Where(ds => ds.Name == Meta.DocumentStatus.Signed)
                //.Select(ds => ds.Id)
                //.FirstOrDefaultAsync();

                //foreach (var doc in documents)
                //{
                //    doc.DocumentStatusId = signedStatusId;
                //}

                //_context.SigningRooms.Update(signingRoom);
                //await _context.SaveChangesAsync();
            }
            catch (Exception rabbitEx)
            {
                _logger.LogError(rabbitEx,
                    "Failed to publish message to RabbitMQ. SigningRoomId: {SigningRoomId}. Continuing...",
                    request.SignningRoomId);
                // Retry/DLQ could be added here
            }

            return Result<int>.Success(documents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error signing documents in SigningRoomId: {SigningRoomId}", request.SignningRoomId);
            return Result<int>.Error("An error occurred while signing the documents.");
        }

    }
}