using Ardalis.Result;
using Signix.API.Models.Requests;
using Signix.Entities.Entities;

namespace Signix.API.Infrastructure;

public interface ISignerService
{
    Task<Result<Signer>> UpdateSignatureAsync(UpdateSignerByIdEndpointRequest request);
}