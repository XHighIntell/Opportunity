using System.Net.WebSockets;
using System.Text.Json;

namespace Opportunity.Net.WebSockets;

public class UndergroundMessage {

    public UndergroundMessage(UnderWebSocket ws, int id, string action, bool responseRequired) {
        this.WebSocket = ws;
        this.Id = id;
        this.Action = action;
        this.ResponseRequired = responseRequired;
    }

    #region Properties
    public UnderWebSocket WebSocket { get; private set; }
    public int Id { get; set; }
    public string Action { get; set; }
    public string? Value { get; set; }
    public bool ResponseRequired { get; private set; }
    public bool IsSent { get; private set; } = false;
    #endregion


    public void SendResponse(string? content) {
        if (ResponseRequired == false) throw new Exception("This message is not allowed for sending responses.");
        if (IsSent == true) throw new Exception("We have already sent a response for this message.");

        var message = new UndergroundMessageRaw() { Id = Id, IsResponse = true, Content = content };
        var json = JsonSerializer.Serialize(message);

        _ = WebSocket.SendRawAsync(json);

        IsSent = true;
    }
}