using UnityEngine;
using WebSocketSharp;

public class WebSocketClient : MonoBehaviour
{
    [SerializeField] private string _ipAddress;
    [SerializeField] private int _port;
    private string serverUri { get => "ws://" + _ipAddress + ":" + _port; }

    private WebSocket _webSocket;

    public delegate void MessageReceivedHandler(WebSocketEncoder.MessageData data);
    public event MessageReceivedHandler OnMessageReceived;

    void Start()
    {
        Connect();
    }

    public void Connect()
    {
        _webSocket = new WebSocket(serverUri);

        _webSocket.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket connected.");
        };

        _webSocket.OnMessage += (sender, e) =>
        {
            string message = e.Data;
            Debug.Log("WebSocket received: " + message);

            WebSocketEncoder.MessageData data = WebSocketEncoder.Decode(message);
            if (data != null)
            {
                OnMessageReceived?.Invoke(data);
            }
        };

        _webSocket.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket closed: " + e.Reason);
        };

        _webSocket.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket error: " + e.Message);
        };

        _webSocket.ConnectAsync();
    }

    public void Send(string message)
    {
        if (_webSocket != null && _webSocket.IsAlive)
        {
            _webSocket.Send(message);
            Debug.Log("Sent: " + message);
        }
    }

    public void Send(WebSocketEncoder.MessageData data)
    {
        if (_webSocket != null && _webSocket.IsAlive)
        {
            string message = WebSocketEncoder.Encode(data);
            _webSocket.Send(message);
            Debug.Log("Sent: " + message);
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        if (_webSocket != null)
        {
            _webSocket.CloseAsync();
            _webSocket = null;
        }
    }
}