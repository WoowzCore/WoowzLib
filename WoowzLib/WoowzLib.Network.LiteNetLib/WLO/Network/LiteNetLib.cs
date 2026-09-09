using LiteNetLib;
using WLI.Network;

namespace WLO.Network;

// todo, off logs
public class LiteNetLib : WLI.Network.Transport{
    private NetManager? __S;
    private NetManager? __C;

    private readonly EventBasedNetListener __SL;
    private readonly EventBasedNetListener __CL;
    
    private NetPeer? __ServerPeer;
    private const string ConnectionKey = "WoowzLib.Network";
    
    public LiteNetLib(){
        NetDebug.Logger = new InternalLogger();

        __SL = new EventBasedNetListener();
        __S  = new NetManager(__SL);

        __SL.ConnectionRequestEvent += Request => {
            WL.Logger.Debug($"[LNL-Server] Запрос на подключение от {Request.RemoteEndPoint}");
            if(__S!.ConnectedPeersCount < 10){ // лимит игроков
                Request.AcceptIfKey(ConnectionKey);
            }else{
                WL.Logger.Warn($"[LNL-Server] Отклонено: превышен лимит игроков");
                Request.Reject();   
            }
        };
        
        __SL.PeerConnectedEvent += Peer => {
            WL.Logger.Info($"[LNL-Server] Новый клиент подключен: {Peer.Id}");
            OnConnected?.Invoke(Peer.Id);
        };
        __SL.PeerDisconnectedEvent += (Peer, Info) => {
            WL.Logger.Info($"[LNL-Server] Клиент {Peer.Id} отключился. Причина: {Info.Reason}");
            OnDisconnected?.Invoke(Peer.Id);
        };
        __SL.NetworkReceiveEvent += (Peer, Reader, Channel, Method) => OnReceive?.Invoke(Peer.Id, Reader.GetRemainingBytes());
        
        // ----------------------------------------------------------------------

        __CL = new EventBasedNetListener();
        __C  = new NetManager(__CL);
        
        __CL.PeerConnectedEvent += Peer => {
            __ServerPeer = Peer;
            WL.Logger.Info($"[LNL-Client] Мы подключились к серверу: {Peer.Address}:{Peer.Port}");
            OnConnected?.Invoke(Peer.Id);
        };
        __CL.PeerDisconnectedEvent += (Peer, Info) => {
            if(Peer == __ServerPeer){ __ServerPeer = null; }
            WL.Logger.Warn($"[LNL] Отключено. Причина: {Info.Reason}");
            OnDisconnected?.Invoke(Peer.Id);
        };
        __CL.NetworkReceiveEvent += (Peer, Reader, Channel, Method) => OnReceive?.Invoke(Peer.Id, Reader.GetRemainingBytes());
    }
    
    public void StartServer(int Port){
        if(!__S!.IsRunning){
            WL.Logger.Info($"[LNL] Запуск сервера на порту {Port}...");
            if(__S.Start(Port)){
                WL.Logger.Info("[LNL] Сервер успешно запущен.");
            }else{
                WL.Logger.Error("[LNL] Не удалось запустить сервер!");
            }
        }
    }
    public void StartClient(string Address, int Port){
        if(!__C!.IsRunning){
            WL.Logger.Info("[LNL] Запуск клиента...");
            __C.Start();
        }
        WL.Logger.Info($"[LNL] Попытка подключения к {Address}:{Port}...");
        __C.Connect(Address, Port, ConnectionKey);
    }

    public void Update(){
        __S?.PollEvents();
        __C?.PollEvents();
    }

    public void Stop(){
        WL.Logger.Info("[LNL] Остановка всех сетевых служб...");
        __S?.Stop();
        __C?.Stop();
        __ServerPeer = null;
    }
    
    public void Send(byte[] Data, int TargetID = Transport.ServerID, bool Matter = false){
        DeliveryMethod Method = Matter ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced;

        if(TargetID == Transport.ServerID){
            __ServerPeer?.Send(Data, Method);
        }else if(TargetID == Transport.AllClientsID){
            __S?.SendToAll(Data, Method);
        }else{
            NetPeer? Peer = __S?.GetPeerById(TargetID) as NetPeer;
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
            switch (Level) {
                case NetLogLevel.Error:   WL.Logger.Error($"[LNL-Core] {Message}"); break;
                case NetLogLevel.Warning: WL.Logger.Warn ($"[LNL-Core] {Message}"); break;
                default:                  WL.Logger.Debug($"[LNL-Core] {Message}"); break; 
            }
        }
    }
}