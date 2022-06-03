namespace Twilio.AspNet.Core
{
    public class TwilioOptions
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

    public enum CredentialType
    {
        Unspecified,
        AuthToken,
        ApiKey
    }
}