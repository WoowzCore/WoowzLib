using LiteNetLib;
using WLI.Network;

namespace WLO.Network;

public class LNLServer : TransportServer{
    private NetManager            __Manager  = null!;
    private EventBasedNetListener __Listener = null!;
    
    // ----------------------------------------------------------------------
    
    public void Start(int Port){
        __Listener = new EventBasedNetListener();
        __Manager = new NetManager(__Listener);

        __Listener.PeerConnectedEvent += Peer => OnClientConnected?.Invoke(Peer.Id);
        __Listener.PeerDisconnectedEvent += (Peer, Info) => OnClientDisconnected?.Invoke(Peer.Id);
        
        __Listener.NetworkReceiveEvent += (Peer, Reader, Channel, Method) => {
            OnDataReceived?.Invoke(Peer.Id, Reader.GetRemainingBytesSpan());
        };

        __Listener.ConnectionRequestEvent += Request => {
            if(OnConnectionRequest != null){
                OnConnectionRequest.Invoke(new ConnectionRequest(Request));
            }else{
                Request.Accept();
            }
        };
        
        __Manager.Start(Port);
    }
    
    public void Stop(){
        __Manager.Stop();
        __Manager = null!;
        __Listener = null!;
    }
    
    public void SendClient(ConnectionID Client, ReadOnlySpan<byte> Data, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable){
        NetPeer? Peer = __Manager.GetPeerById(Client) as NetPeer;
        Peer?.Send(Data, WL.LNL.ConvertDeliveryMethod(DeliveryMethod));
    }
    
    public void SendAllClients(ReadOnlySpan<byte> Data, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable){
        __Manager.SendToAll(Data, WL.LNL.ConvertDeliveryMethod(DeliveryMethod));
    }
    
    public void Update(){
        __Manager.PollEvents();
    }
    
    public TransportServer.DataReceivedHandler? OnDataReceived{ get; set; }
    public Action<ConnectionID>? OnClientConnected{ get; set; }
    public Action<ConnectionID>? OnClientDisconnected{ get; set; }
    
    public class ConnectionRequest(LiteNetLib.ConnectionRequest Request) : TransportServer.ConnectionRequest{
        public string RemoteEndPoint => Request.RemoteEndPoint.ToString();
        public void Accept() => Request.Accept();
        public void Reject() => Request.Reject();
    }
    public Action<TransportServer.ConnectionRequest>? OnConnectionRequest{ get; set; }
}