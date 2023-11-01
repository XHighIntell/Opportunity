using System.Text.Json;
using System.Net.WebSockets;

using Blite.Spot;
using Blite.Spot.Strategy;

using Opportunity.Net.WebSockets;
using Opportunity.WebJob.Trader.Strategy;
using Opportunity.WebJob.Trader.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace Opportunity.WebJob.Trader;

public class Program {
    static void Main(string[] args) {

        AddAccounts(Application.IsLive);
        Application.StartWebSocket();
        Application.StartKeepAlive();

        while (true) {
            Console.ReadLine();
            Console.Clear();
            if (Application.Accounts[0].Strategies[0].Signal is MACDStochRSISignal signal) {
                Console.WriteLine(signal.UpdateCount);
            }
        }
    }

    static void AddAccounts(bool live = false) {

        if (live == false) {
            var account = new Account(Application.SpotClient, Secret.Spot.Testnet) { Name = "Personal" };
            var signal = new MACDStochRSISignal(Application.SpotClient, "BTCUSDT", Binance.Net.Enums.KlineInterval.OneMinute);
            //var signal = new RandomizeSignal(); 
            var strategy = new StrategyDouble(account, new StrategyDoubleParameters("BTCUSDT"), signal);
            strategy.ChainUpdated += (chain, reason) => {
                Console.WriteLine("*** ChainUpdated: " + reason);
                Console.WriteLine(JsonSerializer.Serialize(reason));
            };
            strategy.Profit += (chain, profit) => { Console.WriteLine("********* Profit: " + profit); };
            strategy.Run();
            account.Strategies.Add(strategy);
            Application.Accounts.Add(account);
        }
        else {
            var account = new Account(Application.SpotClient, Secret.Spot.Live) { Name = "X High Intell" };
            var signal = new MACDStochRSISignal(Application.SpotClient, "BTCFDUSD", Binance.Net.Enums.KlineInterval.OneMinute);
            var strategy = new StrategyDouble(account, new StrategyDoubleParameters("BTCFDUSD"), signal);
            strategy.ChainUpdated += (chain, reason) => {
                Console.WriteLine("*** ChainUpdated: " + reason);
                Console.WriteLine(JsonSerializer.Serialize(reason));
            };
            strategy.Profit += (chain, profit) => { Console.WriteLine("********* Profit: " + profit); };

            strategy.Run();
            account.Strategies.Add(strategy);
                
            Application.Accounts.Add(account);
        }

            
    }

}


