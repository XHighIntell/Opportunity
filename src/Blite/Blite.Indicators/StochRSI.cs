namespace Blite.Indicators;
///<summary>
///<para>
///The Stochastic RSI (StochRSI) is an indicator used in technical analysis that ranges between zero and one (or zero and 100 on some charting platforms) 
///and is created by applying the Stochastic oscillator formula to a set of relative strength index (RSI) values rather than to standard price data.
///Using RSI values within the Stochastic formula gives traders an idea of whether the current RSI value is overbought or oversold.
///</para>
/// https://www.investopedia.com/terms/s/stochrsi.asp
///</summary>
public class StochRSI {

    ///<summary>Initializes a new instance of the <see cref="StochRSI"/> class with default length (14, 14, 3, 3).</summary>
    public StochRSI() : this(14, 14, 3, 3) { }

    ///<summary>Initializes a new instance of the <see cref="StochRSI"/> class with specified length.</summary>
    public StochRSI(int periodRSI, int periodStoch, int periodSmoothK, int periodSmoothD) {
        this.rsi = new(periodRSI);
        this.stoch = new(periodStoch);
        this.sma_K = new(periodSmoothK);
        this.sma_D = new(periodSmoothD);
    }

    readonly RSI rsi;
    readonly Stoch stoch;
    readonly SMA sma_K;
    readonly SMA sma_D ;

    ///<summary>Gets the length of rsi indicator.</summary>
    public int RSIPeriod => rsi.Period;

    ///<summary>Gets the length of stoch indicator.</summary>
    public int StochPeriod => stoch.Period;

    ///<summary>Gets the length of k-sma indicator.</summary>
    public int KPeriod => sma_K.Period;

    ///<summary>Gets the length of d-sma indicator.</summary>
    public int DPeriod => sma_D.Period;


    ///<summary>Gets the current result of the indicator from the value and push the value to the stack.</summary>
    public StochRSIResult Next(double value) {
        var rsi = this.rsi.Next(value);
        var nextSTOCH = this.stoch.Next(rsi, rsi, rsi);
        var k = this.sma_K.Next(nextSTOCH);
        var d = this.sma_D.Next(k);

        return new StochRSIResult(k: k, d: d);
    }

    ///<summary>Gets results for a range of values and pushes them to the stack.</summary>
    public StochRSIResult[] Next(params double[] values) {
        var array = new StochRSIResult[values.Length];
        for (var i = 0; i < values.Length; i++) array[i] = Next(values[i]);
        return array;
    }

    public IEnumerable<StochRSIResult> INext(params double[] values) { for (var i = 0; i < values.Length; i++) yield return Next(values[i]); }

    ///<summary> Calculating the instantaneous value of the indicator allows you to obtain information about the indicator readings in real time, without affecting future readings.</summary>
    public StochRSIResult Moment(double value) {
        var rsi = this.rsi.Moment(value);
        var nextSTOCH = this.stoch.Moment(rsi, rsi, rsi);
        var k = this.sma_K.Moment(nextSTOCH);
        var d = this.sma_D.Moment(k);

        return new StochRSIResult(k: k, d: d);
    }


    public class StochRSIResult { 
        public StochRSIResult(double k, double d) {
            this.K = k;
            this.D = d;
        }

        ///<summary>The SMA(STOCH(RSI(), RSI(), RSI(), periodStoch), periodSmoothK).</summary>
        public double K { get; }

        ///<summary>The SMA(k, periodSmoothD).</summary>
        public double D { get; }
    }
}

