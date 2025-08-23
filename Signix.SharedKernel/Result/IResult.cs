/// <summary>
/// Forked from Ardalis.Result
/// </summary>

namespace SharedKernal.Result;
public interface IResult
{
  bool IsSuccess { get; }
  ResultStatus Status { get; }
  IEnumerable<string> Errors { get; }
  List<ValidationError> ValidationErrors { get; }
  Type? ValueType { get; }
  Object? GetValue();
}
