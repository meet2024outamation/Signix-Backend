namespace SharedKernal.AuthorizeHandler;

public class PermissionVM
{
    public HashSet<string> Moduels { get; set; }
    public HashSet<string> Permissions { get; set; }
    public PermissionVM()
    {
        Moduels = new HashSet<string>();
        Permissions = new HashSet<string>();
    }

}

public static class Permissions
{
    public static class Loan
    {
        public const string CreateDocgenOrder = $"{nameof(Loan)}.{nameof(CreateDocgenOrder)}";
        public const string GetLoanDoc = $"{nameof(Loan)}.{nameof(GetLoanDoc)}";
        public const string GetStatus = $"{nameof(Loan)}.{nameof(GetStatus)}";
        public const string GenerateDocument = $"{nameof(Loan)}.{nameof(GenerateDocument)}";
    }
    public static class Decision
    {
        public const string SMDULossMitigation = $"{nameof(Decision)}.{nameof(SMDULossMitigation)}";
        public const string CreateDecisionOrder = $"{nameof(Decision)}.{nameof(CreateDecisionOrder)}";

    }
    public static class FedEx
    {
        public const string Tracking = $"{nameof(FedEx)}.{nameof(Tracking)}";
    }
    
    public static class Timios
    {
        public const string PlaceOrder = $"{nameof(Timios)}.{nameof(PlaceOrder)}";
    }

    public static class Simplifile
    {
        public const string CreatePackage = $"{nameof(Simplifile)}.{nameof(CreatePackage)}";
        public const string GetPackageStatus = $"{nameof(Simplifile)}.{nameof(GetPackageStatus)}";

    }

    public static class LibertyTitleOrder
    {
        public const string PlaceOrder = $"{nameof(LibertyTitleOrder)}.{nameof(PlaceOrder)}";
    }

    public static class NotaryProfile
    {
        public const string Management = $"{nameof(NotaryProfile)}.{nameof(Management)}";
    }
}
