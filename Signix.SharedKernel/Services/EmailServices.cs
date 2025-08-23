using SharedKernal.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Ardalis.GuardClauses;
using MimeKit.Text;
using System.Text;

namespace SharedKernal.Services;

public class EmailServices : IEmailServices
{
    private readonly ILogger<EmailServices> _logger;
    private SmtpSettings _smtpSettings { get; set; }
    public EmailServices(ILogger<EmailServices> logger, IOptions<SmtpSettings> smtpSettings)
    {
        _logger = logger;
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(Email email)
    {
        Guard.Against.Null(email, nameof(email));

        try
        {
            // create message
            var mimeMsg = new MimeMessage();

            var defaultFrom = MailboxAddress.Parse(_smtpSettings.Sender);

            if (email.From.IsNullOrWhiteSpace())
            {
                mimeMsg.From.Add(defaultFrom);
            }
            else
            {
                mimeMsg.From.Add(MailboxAddress.Parse(email.From));
            }


            this.AddEmails(mimeMsg.To, email.To);

            if (email.Cc != null && email.Cc.Count > 0)
                this.AddEmails(mimeMsg.Cc, email.Cc);
            if (email.Bcc != null && email.Bcc.Count > 0)
                this.AddEmails(mimeMsg.Bcc, email.Bcc);

            mimeMsg.Subject = email.Subject!.ToString();

            if (email.IsBodyHtml)
            {
                mimeMsg.Body = new TextPart(TextFormat.Html) { Text = email.Body!.ToString() };
            }
            else
            {
                mimeMsg.Body = new TextPart(TextFormat.Plain) { Text = email.Body!.ToString() };
            }
            // Attachments
            if (email.Attachments != null && email.Attachments.Count > 0)
            {
                foreach (var attachment in email.Attachments)
                {
                    var attachmentPart = new MimePart(attachment.ContentType)
                    {
                        Content = new MimeContent(new MemoryStream(attachment.Data), ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = attachment.FileName
                    };

                    mimeMsg.Body = new Multipart("mixed") { mimeMsg.Body, attachmentPart };
                }
            }
            // send email
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpSettings.SmtpServer, _smtpSettings.Port, _smtpSettings.SSL);

            if (!_smtpSettings.UserName.IsNullOrWhiteSpace())
                await smtp.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);

            await smtp.SendAsync(mimeMsg);
            await smtp.DisconnectAsync(true);
            //_logger.LogDebug($"Sending email to {to} from {from} with subject {subject}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Unexpected error while sending email.");
            throw;
        }
    }

    private void AddEmails(InternetAddressList list, List<string> emails)
    {
        if (emails?.Count > 0)
        {
            list.AddRange(emails.Select(e => MailboxAddress.Parse(e)));
        }
    }

    public async Task SendEmailWithTemplateAsync(EmailTemplate emailTemplate, dynamic obj)
    {
        var email = new Email
        {
            Body = new StringBuilder(await emailTemplate!.Body.RenderAsync(obj)),
            Subject = new StringBuilder(await emailTemplate!.Subject.RenderAsync(obj))
        };

        if (emailTemplate.To?.Count > 0) email.To.AddRange(emailTemplate.To);
        if (emailTemplate.Cc?.Count > 0) email.Cc.AddRange(emailTemplate.Cc);
        if (emailTemplate.Bcc?.Count > 0) email.Bcc.AddRange(emailTemplate.Bcc);
        if (emailTemplate.Attachments?.Count > 0) email.Attachments.AddRange(emailTemplate.Attachments);


        email.IsBodyHtml = emailTemplate.IsBodyHtml;
        await SendEmailAsync(email);
    }
}
