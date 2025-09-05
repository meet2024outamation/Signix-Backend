using Ardalis.Result;
using Signix.API.Models.Requests;
using Signix.API.Models.Responses;
using Signix.Entities.Entities;

namespace Signix.API.Infrastructure;

public interface ISigningRoomService
{
    Task<Result<SigningRoomGetByIdResponse>> GetByIdAsync(int id);
    Task<PagedResult<List<SigningRoomListResponse>>> GetAllAsync(SigningRoomQuery query);
    Task<Result<SigningRoom>> CreateAsync(SigningRoomCreateRequest request);
    Task<Result<int>> RegisterAsync(RegisterSigningRoomRequest request);
    Task<Result<SigningRoom>> UpdateAsync(SigningRoomUpdateRequest request);
    Task<Result> DeleteAsync(int id);
}