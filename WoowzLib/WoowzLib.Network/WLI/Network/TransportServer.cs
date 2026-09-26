using WLO.Network;

namespace WLI.Network;

public interface TransportServer{
    void Start(int Port);
    void Stop();

    void SendClient(ConnectionID Client, ReadOnlySpan<byte> Data, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable);
    void SendAllClients(ReadOnlySpan<byte> Data, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable);
    
    void Update();

    public delegate void DataReceivedHandler(ConnectionID Client, ReadOnlySpan<byte> Data);
    DataReceivedHandler OnDataReceived{ get; set; }
    Action<ConnectionID> OnClientConnected{ get; set; }
    Action<ConnectionID> OnClientDisconnected{ get; set; }
}