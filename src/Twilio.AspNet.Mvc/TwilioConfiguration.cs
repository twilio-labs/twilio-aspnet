using System;
using System.Configuration;

namespace Twilio.AspNet.Mvc
{
    public class RequestValidationConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("authToken")]
        public string AuthToken
        {
            get => (string)this["authToken"];
            set => this["authToken"] = value;
        }

        [ConfigurationProperty("baseUrlOverride")]
        public string BaseUrlOverride
        {
            get => (string)this["baseUrlOverride"];
            set => this["baseUrlOverride"] = value;
        }

        [ConfigurationProperty("allowLocal")]
        public bool AllowLocal
        {
            get => (bool)this["allowLocal"];
            set => this["allowLocal"] = value;
        }
    }

    public class TwilioSectionGroup : ConfigurationSectionGroup
    {

        [ConfigurationProperty("requestValidation", IsRequired = false)]
        public RequestValidationConfigurationSection RequestValidation
        {
            get { return (RequestValidationConfigurationSection)Sections["requestValidation"]; }
        }
    }
}