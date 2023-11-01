namespace Blite.Indicators;

///<summary>
/// <para>
/// Moving average convergence/divergence (MACD, or MAC-D) is a trend-following momentum indicator that shows the relationship between two exponential moving averages (EMAs) of a security’s price. The MACD line is calculated by subtracting the 26-period EMA from the 12-period EMA.
/// </para>
///https://www.investopedia.com/terms/m/macd.asp
///</summary>
public class MACD {

    ///<summary>Initializes a new instance of the <see cref="MACD"/> class with default length (12, 26, 9).</summary>
    public MACD() : this(12, 26, 9) { }

    ///<summary>Initializes a new instance of the <see cref="MACD"/> class with specified length.</summary>
    public MACD(int fastPeriod, int slowPeriod, int signalPeriod) {
        this.FastPeriod = fastPeriod;
        this.SlowPeriod = slowPeriod;
        this.SignalPeriod = signalPeriod;

        fast = new(fastPeriod);
        slow = new(slowPeriod);
        signal = new(signalPeriod);
    }

    #region Fields
    readonly EMA fast;
    readonly EMA slow;
    readonly EMA signal;
    #endregion

    #region Properties
    ///<summary>Gets the length of this fast-ema indicator.</summary>
    public int FastPeriod { get; private set; }

    ///<summary>Gets the length of this slow-ema indicator.</summary>
    public int SlowPeriod { get; private set; }

    ///<summary>Gets the length of this signal-ema indicator.</summary>
    public int SignalPeriod { get; private set; }
    #endregion

    #region Methods
    ///<summary>Gets the current result of the indicator from the value and push the value to the stack.</summary>
    public MACDResult Next(double value) {
        var fastResult = fast.Next(value);
        var slowResult = slow.Next(value);

        var macd = fastResult - slowResult;
        var signal = this.signal.Next(macd);
        var histogram = macd - signal;
        
        return new MACDResult(macd, signal, histogram);
    }

    ///<summary>Gets results for a range of values and pushes them to the stack.</summary>
    public MACDResult[] Next(params double[] values) {
        var array = new MACDResult[values.Length];
        for (var i = 0; i < values.Length; i++) array[i] = Next(values[i]);
        return array;
    }
    public IEnumerable<MACDResult> INext(params double[] values) { for (var i = 0; i < values.Length; i++) yield return Next(values[i]); }

    ///<summary> Calculating the instantaneous value of the indicator allows you to obtain information about the indicator readings in real time, without affecting future readings.</summary>
    public MACDResult Moment(double value) {
        // MACD = EMA(close, 12) - EMA(close, 26)
        // SIGNAL = EMA(MACD, 9)
        // HISTOGRAM = MACD - SIGNAL

        var fastResult = fast.Moment(value);
        var slowResult = slow.Moment(value);

        var macd = fastResult - slowResult;
        var signal = this.signal.Moment(macd);
        var histogram = macd - signal;

        return new MACDResult(macd: macd, signal: signal, histogram: histogram);
    }
    #endregion



    public class MACDResult {
        public MACDResult(double macd, double signal, double histogram) {
            this.Macd = macd;
            this.Signal = signal;
            this.Histogram = histogram;
        }

        public double Macd { get; }
        public double Signal { get; }
        public double Histogram { get; }
    }
}

