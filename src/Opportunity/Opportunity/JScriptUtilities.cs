namespace Opportunity;

public static class JScriptUtilities {
    static readonly long DatetimeMinTimeTicks = new DateTime(1970, 1, 1).Ticks;

    ///<summary>Gets JS date.now()</summary>
    public static long Now {
        get {
            return (DateTime.UtcNow.Ticks - DatetimeMinTimeTicks) / TimeSpan.TicksPerMillisecond;
        }
    }

    public static long GetRoundedMinuteTime() {
        var now = Now;
        return now - (now % 60000);
    }


    public static long ToJavaScriptMilliseconds(this DateTime dt) {
        return (dt.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / TimeSpan.TicksPerMillisecond;
    }

    public static DateTime JSToDateTime(this long javaScriptMilliseconds) {
        return new DateTime(javaScriptMilliseconds * TimeSpan.TicksPerMillisecond + DatetimeMinTimeTicks, DateTimeKind.Utc);
    }

    public static long JavascriptDateNow() {
        return (DateTime.UtcNow.Ticks - DatetimeMinTimeTicks) / TimeSpan.TicksPerMillisecond;
    }

}

