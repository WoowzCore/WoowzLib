using WLO.Network;

namespace WLI.Network;

public interface TransportClient{
    void Connect(string Address, int Port);
    void Disconnect();

    void SendServer(ReadOnlySpan<byte> Data, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable);
    
    void Update();
    
    public delegate void DataReceivedHandler(ReadOnlySpan<byte> Data);
    DataReceivedHandler? OnDataReceived{ get; set; }
    Action? OnConnected{ get; set; }
    Action? OnDisconnected{ get; set; }
}