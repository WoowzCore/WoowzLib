namespace WLI.Network;

public interface Transport{
    bool StartServer(int Port);
    void StartClient(string Address, int Port);
    void Stop();

    public const int ServerID     = -1;
    public const int AllClientsID = -2;
    void Send(byte[] Data, int TargetID = ServerID, bool Matter = true);

    void Update();

    Action<int, byte[]> OnReceive{ get; set; }
    Action<int        > OnConnected{ get; set; }
    Action<int        > OnDisconnected{ get; set; }

    ulong PacketsSent    { get; }
    ulong PacketsReceived{ get; }
    ulong BytesSent      { get; }
    ulong BytesReceived  { get; }

    void Disconnect(int Peer);
}