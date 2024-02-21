namespace Twilio.AspNet.Common
{
    /// <summary>
    /// This class can be used as the parameter on your SMS action. Incoming parameters will be bound here.
    /// </summary>
    /// <remarks>https://www.twilio.com/docs/messaging/guides/webhook-request</remarks>
    public class SmsRequest : TwilioRequest
    {
        /// <summary>
        /// A 34 character unique identifier for the message. May be used to later retrieve this message from the REST API.
        /// </summary>
        public string MessageSid { get; set; }
        
        /// <summary>
        /// Same value as MessageSid. Deprecated and included for backward compatibility.
        /// </summary>
        public string SmsSid { get; set; }

        /// <summary>
        /// The text body of the SMS message. Up to 160 characters long
        /// </summary>
        public string Body { get; set; }
        
        /// <summary>
        /// The status of the message
        /// </summary>
        public string MessageStatus { get; set; }

        /// <summary>
        /// The message OptOut type 
        /// </summary>
        public string OptOutType { get; set; }

        /// <summary>
        /// A unique identifier of the messaging service
        /// </summary>
        public string MessagingServiceSid { get; set; }
        
        /// <summary>
        /// The number of media items associated with your message
        /// </summary>
        public int NumMedia { get; set; }
        
        /// <summary>
        /// The number of media items associated with a "Click to WhatsApp" advertisement.
        /// </summary>
        public int ReferralNumMedia { get; set; }

        /// <summary>
        /// The number of media files associated with the Message resource
        /// </summary>
        public int NumSegments { get; set; }
    }
}
