namespace Opportunity.AspNetCore;

///<summary>Represents expected errors that occur during application execution.</summary>
///<remarks>Although it is named Exception, it is used for errors that are expected.</remarks>
public class HandledException: Exception {
    public HandledException(string message) : base(message) { HResult = -1; }
    public HandledException(string message, int code) : base(message) { HResult = code; }
}