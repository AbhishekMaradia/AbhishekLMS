namespace LMS_SoulCode.Features.Common.Services
{
    public class BusinessValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
