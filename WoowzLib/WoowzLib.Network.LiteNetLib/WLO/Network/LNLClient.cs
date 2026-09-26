using LiteNetLib;
using WLI.Network;

namespace WLO.Network;

public class LNLClient : TransportClient{
    private NetManager            __Manager  = null!;
    private EventBasedNetListener __Listener = null!;

    private NetPeer __ServerPeer = null!;
    
    // ----------------------------------------------------------------------
    
    public void Connect(string Address, int Port){
        __Listener = new EventBasedNetListener();
        __Manager = new NetManager(__Listener);

        __Listener.PeerConnectedEvent += Peer => { __ServerPeer = Peer; OnConnected?.Invoke(); };
        __Listener.PeerDisconnectedEvent += (Peer, Info) => { __ServerPeer = null!; OnDisconnected?.Invoke(); };
        
        __Listener.NetworkReceiveEvent += (Peer, Reader, Channel, Method) => {
            OnDataReceived?.Invoke(Reader.GetRemainingBytesSpan());
        };
        
        __Manager.Start();
        __Manager.Connect(Address, Port, "");
    }
    
    public void Disconnect(){
        __Manager.Stop();
        __Manager = null!;
        __Listener = null!;
    }
    
    public void SendServer(ReadOnlySpan<byte> Data, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable){
        if(__ServerPeer == null! || __ServerPeer.ConnectionState != ConnectionState.Connected){ return; }
        __ServerPeer.Send(Data, WL.LNL.ConvertDeliveryMethod(DeliveryMethod));
    }
    
    public void Update(){
        __Manager.PollEvents();
    }

    public TransportClient.DataReceivedHandler? OnDataReceived{ get; set; }
    public Action? OnConnected{ get; set; }
    public Action? OnDisconnected{ get; set; }
}