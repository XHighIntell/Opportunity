namespace Blite.Spot.Strategy;

public class StrategyDoubleParameters {

    public StrategyDoubleParameters(string symbol) {
        this.SYMBOL = symbol;
        this.QUANTITY = 0.0005m;
        this.MAX_TRY = 5;
        
        this.WAIT_TIME_FOR_ORDER_TO_FILL_BEFORE_CANCEL = TimeSpan.FromMinutes(15);
        this.SIGNAL_LIFESPAN = TimeSpan.FromMinutes(10); 
        this.MIN_TIME_BETWEEN_SIGNAL = TimeSpan.FromMinutes(10); // 10m

        this.ENTRY_DIFF_PERCENT = 0.015m; // change from 0.02m to 0.015
        this.MIN_PRICE_CHANGE_PERCENT = 0.3m;

        // LOSS SERIES feature
        this.LOSS_SERIES_ENTRY_DIFF_MULTIPLIER = 1.1m;
        this.LOSS_SERIES_MIN_PRICE_CHANGE_MULTIPLIER = 1.5m; // IMPORTANT SHOULD BE 1.5 or MORE
        
        this.PERSISTENT = false;
    }

    #region Parameters
    ///<summary>The name of trading pair in uppercase. Ex: "XMRBUSD" | "BNBBUSD"</summary>
    public string SYMBOL { get; set; }

    ///<summary>The quantity of the first order. target_quantity = QUANTITY * 2 ^ (nth_try - 1).</summary>
    public decimal QUANTITY { get; set; }
        
    ///<summary>Maximum number of tries in a series of losses. Each time try increase quantity = QUANTITY * 2 ^ (nth_try - 1).</summary>
    public int MAX_TRY { get; set; }

    ///<summary>Time to wait (in seconds) for order to fille before cancle.</summary>
    public TimeSpan WAIT_TIME_FOR_ORDER_TO_FILL_BEFORE_CANCEL { get; set; }

    ///<summary>How long is signal lifespan?</summary>
    public TimeSpan SIGNAL_LIFESPAN { get; set; }

    ///<summary>The minimum time between each asking for a signal.</summary>
    public TimeSpan MIN_TIME_BETWEEN_SIGNAL { get; set; }

    ///<summary>The price for order will be better x percent.</summary>
    public decimal ENTRY_DIFF_PERCENT { get; set; }

    public decimal MIN_PRICE_CHANGE_PERCENT { get; set; }
    

    ///<summary>
    ///Each time a signal loss the next try will increase ENTRY_DIFF_PERCENT by value%.
    ///<para>DIFF_PERCENT = ENTRY_DIFF_PERCENT * LOSS_SERIES_ENTRY_DIFF_MULTIPLIER ^ (try-1)</para>
    ///</summary>
    public decimal LOSS_SERIES_ENTRY_DIFF_MULTIPLIER { get; set; }

    public decimal LOSS_SERIES_MIN_PRICE_CHANGE_MULTIPLIER { get; set; }



    ///<summary>Ignore</summary>
    public bool PERSISTENT { get; set; }
    #endregion

    public void Validate() {
        if (QUANTITY <= 0m) throw new Exception("QUANTITY must be greater than 0.");
        if (MAX_TRY < 2) throw new Exception("MAX_TRY must be greater than 1. How to double if we only try 1 time?");
    }

    //public static StrategyDoubleParameters Create(string symbol, int max_try = 5, decimal entry_diff_percent = 0.02m, decimal min_price_change_percent = 0.05m,
    //
    //    TimeSpan WAIT_TIME_FOR_ORDER_TO_FILL_BEFORE_CANCEL = new TimeSpan(0, 5, 0)
    //   ) {
    //    return new StrategyDoubleParameters() {
    //
    //    };
    //}
}

