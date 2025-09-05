using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Signix.API.Models.Requests;

//public class UpdateSignerSignatureRequest
//{
//    public int SignerId { get; set; }

//    [Required]
//    [JsonPropertyName("base64Signature")]
//    public string Base64Signature { get; set; } = string.Empty;
//}

//public class UpdateSignerByIdEndpointRequest
//{
//    public int SignerId { get; set; }
//    public UpdateSignerSignatureBody Body { get; set; } = new();
//}
public class UpdateSignerByIdEndpointRequest
{
    [FromRoute]
    public int SignerId { get; set; }

    [FromBody]
    public UpdateSignerSignatureBody Body { get; set; } = new();
}
public class UpdateSignerSignatureBody
{
    [Required]
    public string Base64Signature { get; set; } = string.Empty;
}