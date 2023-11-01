using Binance.Net.Enums;
using Binance.Net.Objects.Models.Spot;

namespace Blite.Spot.Strategy;

public class ChainStep {
    public DateTime CreationTime { get; set; }
    public OrderSide? Signal { get; set; }
    public DateTime? SignalTime { get; set; }
    public int? Try { get; set; }
    public decimal? Asset { get; set; }
    public decimal? Price { get; set; }

    public BinancePlacedOrder? Order { get; set; }
    public bool IsCompleted { get; set; }
    public decimal? Profit { get; set; }
}

public enum ChainUpdateReason {
    ///<summary>A new <see cref="ChainStep"/> is created.</summary>
    New,

    ///<summary>When <see cref="ChainStep.Signal"/>, <see cref="ChainStep.Try"/> or <see cref="ChainStep.Asset"/> are changed.</summary>
    Modified,

    ///<summary>Placed new order.</summary>
    OrderCreated,

    ///<summary>Canceled order.</summary>
    OrderCanceled,

    ///<summary>Order is filled.</summary>
    OrderFilled,
}