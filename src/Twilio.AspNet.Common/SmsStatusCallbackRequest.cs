namespace Twilio.AspNet.Common;

public class SmsStatusCallbackRequest: SmsRequest
{
    /// <summary>
    /// The error code (if any) associated with your message. If your message 
    /// status is failed or undelivered, the ErrorCode can give you more information 
    /// about the failure. If the message was delivered successfully, no ErrorCode 
    /// will be present. Find the possible values here:
    /// https://www.twilio.com/docs/sms/api/message-resource#delivery-related-errors
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// The Installed Channel SID (found on the Channel detail page) that was 
    /// used to send this message. Only present if the message was sent using a 
    /// Channel.
    /// </summary>
    public string ChannelInstallSid { get; set; }

    /// <summary>
    /// The Error message returned by the underlying Channel if Message delivery 
    /// failed. Only present if the message was sent using a Channel and message 
    /// delivery failed.
    /// </summary>
    public string ChannelStatusMessage { get; set; }

    /// <summary>
    /// Channel specific prefix that allows you to identify which channel this 
    /// message was sent over.
    /// </summary>
    public string ChannelPrefix { get; set; }

    /// <summary>
    /// Contains post-delivery events. If the Channel supports Read receipts, this 
    /// parameter will be included with a value of READ after the user has read 
    /// the message. Currently supported only for WhatsApp.
    /// </summary>
    public string EventType { get; set; }
}