using Ardalis.Result;
using Signix.API.Models.Requests;
using Signix.Entities.Entities;

namespace Signix.API.Infrastructure;

public interface ISigningRoomService
{
    Task<Result<SigningRoom>> GetByIdAsync(int id);
    Task<PagedResult<List<SigningRoom>>> GetAllAsync(SigningRoomQuery query);
    Task<Result<SigningRoom>> CreateAsync(SigningRoomCreateRequest request);
    Task<Result<int>> RegisterAsync(RegisterSigningRoomRequest request);
    Task<Result<SigningRoom>> UpdateAsync(SigningRoomUpdateRequest request);
    Task<Result> DeleteAsync(int id);
}