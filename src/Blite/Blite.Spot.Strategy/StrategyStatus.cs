namespace Blite.Spot.Strategy;

///<summary>Status of a strategy.</summary>
public enum StrategyStatus {
    ///<summary>Reserved for future use.</summary>
    None,

    ///<summary>Running.</summary>
    Running,

    ///<summary>Indicates that the <seealso cref="StrategyDouble"/> has been aborted due to unhandled error.</summary>
    Aborted,
}