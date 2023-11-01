namespace Blite;

public static class JsDateTime {

    public static readonly long JavascriptMinTick = new DateTime(1970, 1, 1).Ticks;

    ///<summary>Gets JS date.now()</summary>
    public static long Now => (DateTime.UtcNow.Ticks - JavascriptMinTick) / TimeSpan.TicksPerMillisecond;
    public static long ToJsDateTime(this DateTime dt) => (dt.ToUniversalTime().Ticks - JavascriptMinTick) / TimeSpan.TicksPerMillisecond;
        
    public static long? ToJsDateTime(this DateTime? dt) {
        if (dt == null) return null;

        return dt.Value.ToJsDateTime();
    }

    public static DateTime JSToDateTime(this long jsDate) {
        return new DateTime(JavascriptMinTick + jsDate * TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
    }

}


