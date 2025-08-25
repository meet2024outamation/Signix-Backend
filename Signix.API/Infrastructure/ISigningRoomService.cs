using Ardalis.Result;
using Signix.API.Models.Requests;
using Signix.Entities.Entities;

namespace Signix.API.Infrastructure;

public interface ISigningRoomService
{
    Task<Result<SigningRoom>> GetByIdAsync(int id);
    Task<PagedResult<List<SigningRoom>>> GetAllAsync(SigningRoomQuery query);
    Task<Result<SigningRoom>> CreateAsync(CreateSigningRoomRequest request);
    Task<Result<SigningRoom>> UpdateAsync(UpdateSigningRoomRequest request);
    Task<Result> DeleteAsync(int id);
}