namespace Signix.IAM.API.Endpoints.Client
{
    public class GetAccessibleClientResponse
    {
        public string ClientId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsCurrentClient { get; set; }
        public string ClientType { get; set; }
        public int ClientTypeId { get; set; }

    }
}
