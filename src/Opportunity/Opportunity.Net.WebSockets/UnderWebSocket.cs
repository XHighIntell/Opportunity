using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Opportunity.Net.WebSockets;

/*
    { action: "setX", value: "2" } → 


    { id: 123, action: "getAccount", value: "XHighIntell" } → 
                ← { id: 123, isResponse: true, value: "asdasasdasdasdasd" }

*/

public class UnderWebSocket: IDisposable {

    public UnderWebSocket(WebSocket webSocket) {
        if (webSocket.State != WebSocketState.Open) throw new Exception("The websocket must be connected.");
        
        this.WebSocket = webSocket;

        BeginReceive();
    }

    #region Properties
    internal WebSocket WebSocket { get; private set; }
    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new();
    public Task<bool> Task { get { return TaskCompletionSource.Task; } }
    #endregion
    
    public event Action<UndergroundMessage>? Message;
    private readonly Dictionary<int, TaskCompletionSource<string?>> callbacks = new();

    #region Public Methods
    public async Task Send(string action, string? value = null) {
        var message = new UndergroundMessageRaw() { Action = action, Content = value };
        await SendRawAsync(JsonSerializer.Serialize(message));
    }
    public async Task<string?> SendWithResponse(string action, string? value = null) {
        var tcs = new TaskCompletionSource<string?>();

        var message = new UndergroundMessageRaw() { Id = Random.Shared.Next(), Action = action, Content = value };
        callbacks.Add(message.Id.Value, tcs);

        var json = JsonSerializer.Serialize(message);
        _ = SendRawAsync(json);
        
        return await tcs.Task;
    }
    public void Dispose() {
        this.WebSocket.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Private Methods
    internal async Task SendRawAsync(string raw) {
        var bytes = Encoding.UTF8.GetBytes(raw);
        await WebSocket.SendAsync(bytes, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
    }
    private async void BeginReceive() {
        var buffer = new byte[1024 * 4];
        var sb = new StringBuilder();

        while (true) {
            bool endOfMessage;
            sb.Clear();
            do {
                try {
                    var result = await WebSocket.ReceiveAsync(buffer, default);
                    if (result.CloseStatus == null) {
                        var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        sb.Append(text);
                        endOfMessage = result.EndOfMessage;
                    }
                    else {
                        OnClose(result.CloseStatus.Value, result.CloseStatusDescription!);
                        return;
                    }
                }
                catch (WebSocketException ex) {
                    OnClose(WebSocketCloseStatus.Empty, ex.GetBaseException().Message);
                    return;
                }


            } while (endOfMessage == false);

            OnMessage(sb.ToString());
        }

    }
    #endregion

    static UndergroundMessageRaw? DeserializeRawMessageFromJson(string json) {
        try {
            var raw = JsonSerializer.Deserialize<UndergroundMessageRaw>(json);
            return raw;
        }
        catch {
            Console.WriteLine("[WARNING] why we get this json?");
            Console.WriteLine(json);
        }
        return null;
    }



    void OnMessage(string json) {
        //var jobject = JsonNode.Parse(json) as JsonObject ?? throw new Exception("We only support json.");
        
        var raw = DeserializeRawMessageFromJson(json);
        if (raw == null) return; // ignore

        if (raw.Id == null) {
            // a message without waiting for response
            var message = new UndergroundMessage(this, 0, raw.Action!, false);
            Message?.Invoke(message);
        }
        else if (raw.Id != null && raw.IsResponse == false) {
            // a message that is waiting for response
            var message = new UndergroundMessage(this, raw.Id.Value, raw.Action!, true);
            Message?.Invoke(message);
            if (message.ResponseRequired == true && message.IsSent == false) message.SendResponse(null);
        } 
        else if (raw.Id != null && raw.IsResponse == true) {
            // this is response
            if (raw?.Id == null) {
                Console.WriteLine("Why a response without id?");
                return;
            }
            var tsc = callbacks[raw.Id.Value];
            if (tsc == null) return;
            callbacks.Remove(raw.Id.Value);
            tsc.SetResult(raw.Content);
        }
    }
    void OnClose(WebSocketCloseStatus closeStatus, string description) {
        TaskCompletionSource.SetResult(false);
        foreach (var kv in callbacks) {
            kv.Value.SetException(new Exception("Connection closed."));
        }
    }
}

internal class UndergroundMessageRaw {
    public int? Id { get; set; }
    public string? Action { get; set; }
    public string? Content { get; set; }
    public bool IsResponse { get; set; } = false;
}


