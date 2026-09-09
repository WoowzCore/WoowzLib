using LiteNetLib;
using WLI.Network;

namespace WLO.Network;

// todo, off logs
public class LiteNetLib : WLI.Network.Transport{
    private NetManager? __Server;
    private NetManager? __Client;

    private readonly EventBasedNetListener __ServerListener;
    private readonly EventBasedNetListener __ClientListener;
    
    private NetPeer? __ServerPeer;

    private const string ConnectionKey = "WoowzLib.Network";
    
    public LiteNetLib(){
        NetDebug.Logger = new InternalLogger();

        __ServerListener = new EventBasedNetListener();
        __Server         = new NetManager(__ServerListener);

        __ServerListener.ConnectionRequestEvent += Request => {
            if(__Server!.ConnectedPeersCount < 10){ // лимит игроков
                Request.AcceptIfKey(ConnectionKey);
            }else{
                Request.Reject();   
            }
        };

        __ServerListener.NetworkReceiveEvent += (Peer, Reader, Channel, Method) => {
            OnReceive?.Invoke(Peer.Id, Reader.GetRemainingBytes());
        };

        __ServerListener.PeerConnectedEvent += Peer => {
            WL.Logger.Info($"[LNL-Server] Новый клиент подключен: {Peer.Id}");
            OnConnected?.Invoke(Peer.Id);
        };
        
        // ----------------------------------------------------------------------

        __ClientListener = new EventBasedNetListener();
        __Client         = new NetManager(__ClientListener);
        
        __ClientListener.NetworkReceiveEvent += (Peer, Reader, Channel, Method) => {
            OnReceive?.Invoke(Peer.Id, Reader.GetRemainingBytes());
        };

        __ClientListener.PeerConnectedEvent += Peer => {
            __ServerPeer = Peer;
            WL.Logger.Info($"[LNL-Client] Мы подключились к серверу: {Peer.Address}:{Peer.Port}");
            OnConnected?.Invoke(Peer.Id);
        };

        __ClientListener.PeerDisconnectedEvent += (Peer, Info) => {
            if(Peer == __ServerPeer){ __ServerPeer = null; }
            WL.Logger.Warn($"[LNL] Отключено. Причина: {Info.Reason}");
            OnDisconnected?.Invoke(Peer.Id);
        };
    }
    
    public void StartServer(int Port){
        if(!__Server!.IsRunning){
            __Server.Start(Port);
            WL.Logger.Info($"[LNL] Сервер запущен на порту {Port}");
        }
    }
    public void StartClient(string Address, int Port){
        if(!__Client!.IsRunning){
            __Client.Start();
        }
        WL.Logger.Info($"[LNL] Отправка запроса к {Address}:{Port}...");
        __Client.Connect(Address, Port, ConnectionKey);
    }

    public void Update(){
        __Server?.PollEvents();
        __Client?.PollEvents();
    }

    public void Stop(){
        __Server?.Stop();
        __Client?.Stop();
        __ServerPeer = null;
    }
    
    public void Send(byte[] Data, int TargetID = Transport.ServerID, bool NoMatter = true){
        DeliveryMethod Method = NoMatter ? DeliveryMethod.Sequenced : DeliveryMethod.ReliableOrdered;

        if(TargetID == Transport.ServerID){
            __ServerPeer?.Send(Data, Method);
        }else if(TargetID == Transport.AllClientsID){
            __Server?.SendToAll(Data, Method);
        }else{
            NetPeer? Peer = __Server?.GetPeerById(TargetID) as NetPeer;
            Peer?.Send(Data, Method);
        }
    }

    public Action<int, byte[]> OnReceive{ get; set; } = null!;
    public Action<int        > OnConnected{ get; set; } = null!;
    public Action<int        > OnDisconnected{ get; set; } = null!;
    
    // ----------------------------------------------------------------------
    
    private class InternalLogger : INetLogger{
        public void WriteNet(NetLogLevel Level, string String, params object[] Args){
            string Message = string.Format(String, Args);
            if(Level == NetLogLevel.Error){
                WL.Logger.Error($"[LNL-Core] {Message}");
            }else if(Level == NetLogLevel.Warning){
                WL.Logger.Warn($"[LNL-Core] {Message}");
            }else{
                WL.Logger.Debug($"[LNL-Core] {Message}");
            }
        }
    }
}