using SharedKernal.Models;

namespace SharedKernal.Services;

public interface IEmailServices
{
    Task SendEmailAsync(Email email);
    Task SendEmailWithTemplateAsync(EmailTemplate emailTemplate, dynamic obj);
}
