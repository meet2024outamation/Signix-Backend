using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Signix.API.Models.Requests;
using Signix.Entities.Context;
using Signix.Entities.Entities;

namespace Signix.API.Infrastructure;

public class SignerService : ISignerService
{
    private readonly SignixDbContext _context;
    private readonly ILogger<SignerService> _logger;

    public SignerService(SignixDbContext context, ILogger<SignerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Signer>> UpdateSignatureAsync(UpdateSignerByIdEndpointRequest request)
    {
        try
        {
            _logger.LogInformation("Updating signature for signer ID: {SignerId}", request.SignerId);

            var signer = await _context.Signers
                .Include(s => s.SigningRoom)
                .Include(s => s.Designation)
                .FirstOrDefaultAsync(s => s.Id == request.SignerId);

            if (signer == null)
            {
                _logger.LogWarning("Signer not found with ID: {SignerId}", request.SignerId);
                return Result<Signer>.NotFound($"No signer found with ID {request.SignerId}");
            }

            try
            {
                Convert.FromBase64String(request.Body.Base64Signature);
            }
            catch (FormatException)
            {
                return Result<Signer>.Invalid(new ValidationError
                {
                    Identifier = nameof(request.Body.Base64Signature),
                    ErrorMessage = "Invalid base64 format"
                });
            }

            signer.SignatureData = request.Body.Base64Signature;
            signer.SignedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated signature for signer ID: {SignerId}", request.SignerId);

            return Result<Signer>.Success(signer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating signature for signer ID: {SignerId}", request.SignerId);
            return Result<Signer>.Error("An error occurred while updating the signature");
        }
    }
}