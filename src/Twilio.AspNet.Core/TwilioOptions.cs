namespace Twilio.AspNet.Core
{
    public class TwilioOptions
    {
        public string AuthToken { get; set; }
        public TwilioClientOptions Client { get; set; }
        public TwilioRequestValidationOptions RequestValidation { get; set; }
    }

    public class TwilioClientOptions
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string ApiKeySid { get; set; }
        public string ApiKeySecret { get; set; }
        public CredentialType CredentialType { get; set; }
        public string Region { get; set; }
        public string Edge { get; set; }
        public string LogLevel { get; set; }
    }

    public class TwilioRequestValidationOptions
    {
        public string AuthToken { get; set; }
        public bool? AllowLocal { get; set; }
        public string BaseUrlOverride { get; set; }
    }

    public enum CredentialType
    {
        Unspecified,
        AuthToken,
        ApiKey
    }
}