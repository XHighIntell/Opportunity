using Binance.Net;
using Blite.Spot;
using Opportunity.Net.WebSockets;
using Opportunity.WebJob.Trader.Text.Json.Serialization;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Text.Json;

namespace Opportunity.WebJob.Trader;



public static class Application {

    static Application() {
        Name = "X HIGH INTELL";

#if RELEASE
        IsLive = true;
#else
        IsLive = false;
#endif
        
    }

    // Private Fields
    static BliteClient? spotClient { get; set; }


    #region Properties
    public static bool IsLive { get; }
    public static string Name { get; }
    public static BliteClient SpotClient {
        get {
            if (spotClient == null) {
                if (IsLive == false) spotClient = new BliteClient(BinanceEnvironment.Testnet);
                else spotClient = new BliteClient(BinanceEnvironment.Live);
            }
            return spotClient;
        }
    }
    public static List<Account> Accounts { get; } = new();
    public static UnderWebSocket? WebSocketUnderground { get; set; }
    #endregion

    #region Methods
    public static async void StartWebSocket() {
        while (true) {
            using var ws = new ClientWebSocket();
            try {
                if (IsLive == true) await ws.ConnectAsync(new Uri("wss://traders.azurewebsites.net/api/ws?key=1994"), default);
                else await ws.ConnectAsync(new Uri("wss://localhost:7273/api/ws?key=1994"), default);

                Console.WriteLine("Connected to web application.");

                var underWebsocket = WebSocketUnderground = new UnderWebSocket(ws);
                underWebsocket.Message += (message) => {
                    var parts = message.Action.Split("?");
                    var path = parts[0].ToLower();
                    var query = System.Web.HttpUtility.ParseQueryString(parts.Length > 1 ? parts[1] : "");

                    try {
                        if (message.Action == "getName") message.SendResponse(Application.Name);
                        else if (message.Action == "getAll") {
                            message.SendResponse(JsonSerializer.Serialize(new {
                                name = Application.Name,
                                accounts = Application.Accounts.Select(a => a.ToJsonObject())
                            }));
                        }
                        // else message.SendResponse("NotImplemented: " + path + query.ToString());
                    }
                    catch (Exception ex) {
                        if (message.ResponseRequired == true) {
                            message.SendResponse(JsonSerializer.Serialize(new {
                                error = new {
                                    message = ex.Message,
                                    stack = ex.StackTrace,
                                }
                            }));
                        }
                    }





                };

                await underWebsocket.Task;
                Console.WriteLine("Lost connection to web application.");
            }
            catch (WebSocketException ex) {
                Console.WriteLine(ex.GetBaseException().Message);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.GetBaseException().Message);
            }

            await Task.Delay(60000);


        }

    }
    public static async void StartKeepAlive() {
        while (true) {
            await Task.Delay(60000 * 60);
            Console.WriteLine("[KeepAlive]" + DateTime.UtcNow.ToLongDateString());
        }
    }
    #endregion
}