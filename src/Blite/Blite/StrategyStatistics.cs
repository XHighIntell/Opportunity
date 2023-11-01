namespace Blite;

public class StrategyStatistics {
    ///<summary>Gets the start time of strategy.</summary>
    public DateTime? StartedAt { get; internal set; }

    ///<summary>Gets the total number of profit.</summary>
    public decimal TotalProfit { get; internal set; }

    ///<summary>Gets the total number of loss.</summary>
    public decimal TotalLoss { get; internal set; }

    ///<summary>Gets the the net profit.</summary>
    public decimal NetProfit { get { return TotalProfit - TotalLoss; } }

    ///<summary>Get the total win signal.</summary>
    public decimal WinningSignal { get; internal set; }

    ///<summary>Gets the total loss signal.</summary>
    public decimal LossingSignal { get; internal set; }
}

