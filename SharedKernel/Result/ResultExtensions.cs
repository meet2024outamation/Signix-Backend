using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace SharedKernel.Result.Extensions;
public static class ResultExtensions
{
  public static Result<T> ToSuccessResult<T>(this T value)
  {
    return Result<T>.Success(value);
  }

  public static Result<T> ToErrorResult<T>(this IEnumerable<string> errors)
  {
    return Result<T>.Error(errors.ToArray());
  }
  
  public static Result<T> ToInvalidResult<T>(this List<ValidationError> validationErrors)
  { 
    return Result<T>.Invalid(validationErrors);
  }
   
}
