using Binance.Net.Objects.Models.Spot;
using Blite;
using Blite.Spot;
using Blite.Spot.Strategy;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Opportunity.WebJob.Trader.Text.Json.Serialization {
    public static class JsonObjectConverter {
        public static JsonObject ToJsonObject(this Account account) {
            var jaccount = new JsonObject {
                ["name"] = account.Name,
                ["strategies"] = new JsonArray(account.Strategies.Select(s => s.ToJsonObject()).ToArray())
            };

            return jaccount;
        }
        public static JsonObject ToJsonObject(this StrategyDouble strategy) {
            var jstrategy = new JsonObject {
                ["name"] = strategy.Parameters.SYMBOL + "/" + nameof(StrategyDouble),
                ["parameters"] = strategy.Parameters.ToJsonObject(),
                ["chains"] = new JsonArray(strategy.ChainSteps.Select(o => o.ToJsonObject()).ToArray()),
                ["statistics"] = strategy.Statistics.ToJsonObject(),
            };
            //(strategy.Signal as MACDStochRSISignal).SubscriptionItem
            return jstrategy;
        }
        public static JsonObject ToJsonObject(this StrategyStatistics statistics) {
            return new JsonObject {
                ["startedAt"] = statistics.StartedAt.ToJsDateTime(),
                ["totalProfit"] = statistics.TotalProfit,
                ["totalLoss"] = statistics.TotalLoss,
                ["netProfit"] = statistics.NetProfit,
                ["winningSignal"] = statistics.WinningSignal,
                ["lossingSignal"] = statistics.LossingSignal,
            };
        }
        public static JsonObject ToJsonObject(this StrategyDoubleParameters parameters) {
            return new JsonObject {
                ["SYMBOL"] = parameters.SYMBOL,
                ["QUANTITY"] = parameters.QUANTITY,
                ["MAX_TRY"] = parameters.MAX_TRY,
                ["ENTRY_DIFF_PERCENT"] = parameters.ENTRY_DIFF_PERCENT,
                ["WAIT_TIME_FOR_ORDER_TO_FILL_BEFORE_CANCEL"] = parameters.WAIT_TIME_FOR_ORDER_TO_FILL_BEFORE_CANCEL.Ticks / TimeSpan.TicksPerMillisecond,
                ["TIME_BETWEEN_SIGNAL"] = parameters.MIN_TIME_BETWEEN_SIGNAL.Ticks / TimeSpan.TicksPerMillisecond,
            };
        }
        public static JsonObject ToJsonObject(this ChainStep chain) {
            return new JsonObject {
                ["time"] = chain.CreationTime.ToJsDateTime(),
                ["signal"] = chain.Signal?.ToString()?.ToUpper(),
                ["asset"] = chain.Asset,
                ["try"] = chain.Try,
                ["price"] = chain.Price,
                ["order"] = chain.Order?.ToJsonObject(),
                ["completed"] = chain.IsCompleted,
                ["profit"] = chain.Profit,
            };
        }
        public static JsonObject ToJsonObject(this BinancePlacedOrder order) {
            return new JsonObject {
                ["symbol"] = order.Symbol,
                ["orderId"] = order.Id,
                ["side"] = order.Side.ToString().ToUpper(),
                ["type"] = order.Type.ToString(),
                ["status"] = order.Status.ToString().ToUpper(),

                ["origQty"] = order.Quantity,
                ["executedQty"] = order.QuantityFilled,
                ["price"] = order.Price,
                ["timeInForce"] = order.TimeInForce.ToString().ToUpper(),
                ["updateTime"] = order.UpdateTime.ToJsDateTime(),
            };

        }
    }





}
