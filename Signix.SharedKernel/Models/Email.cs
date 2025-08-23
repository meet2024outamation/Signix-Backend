using SharedKernal.Adapters;
using System.Text;

namespace SharedKernal.Models;

public class Email
{
    /// <summary>
    /// Sender email address optional field
    /// </summary>
    public string From { get; set; } = null!;
    public List<string> To { get; set; } = null!;
    public List<string> Cc { get; set; }
    public List<string> Bcc { get; set; }
    public bool IsBodyHtml { get; set; }
    public StringBuilder? Subject { get; set; }
    public StringBuilder? Body { get; set; }
    public Email()
    {
        To = new List<string>();
        Cc = new List<string>();
        Bcc = new List<string>();
        Attachments = new List<Attachment>();
    }
    public bool? Priority { get; set; }
    public List<Attachment> Attachments { get; set; }

}
public class EmailTemplate
{
    public string? From { get; set; }
    public List<string> To { get; set; }
    public List<string> Cc { get; set; }
    public List<string> Bcc { get; set; }
    public ILiquidTemplate Subject { get; set; } = null!;
    public ILiquidTemplate Body { get; set; } = null!;
    public bool IsBodyHtml { get; set; }
    public bool? Priority { get; set; }
    public List<Attachment> Attachments { get; set; }
    public EmailTemplate()
    {
        To = new List<string>();
        Cc = new List<string>();
        Bcc = new List<string>();
        Attachments = new List<Attachment>();
    }

}
public class Attachment
{
    public string ContentType { get; set; } = null!;
    public byte[] Data { get; set; } = null!;
    public string FileName { get; set; } = null!;
}
