namespace Signix.API.Models
{
    public class Meta
    {
        public enum DesignationEnum
        {
            Lender = 1,
            Notary = 2,
            Mers = 3,
        }

        public static class RabbitMQ
        {
            public const string QueueName = "signix-docsign";
            public const string AckQueueName = "signix-docsign-ack";
        }

        public static class DocumentStatus
        {
            public const string Pending = "Pending";
            public const string Signed = "Signed";
            public const string Failed = "Failed";
        }
        public class RabbitMQSettings
        {
            public string Host { get; set; } = string.Empty;
            public string VirtualHost { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

    }
}
