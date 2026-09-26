namespace WLO.Network;

public readonly struct ConnectionID{
    public readonly ulong ID;
    
    public uint IDU => (uint)ID;
    public long IDL => (long)ID;
    public int  IDI => (int )ID;

    public ConnectionID(ulong ID) { this.ID = ID; }
    public ConnectionID(uint  ID) : this((ulong)ID){}
    public ConnectionID(long  ID) : this((ulong)ID){}
    public ConnectionID(int   ID) : this((ulong)ID){}

    public static implicit operator ConnectionID(ulong ID) => new ConnectionID(ID);
    public static implicit operator ulong(ConnectionID ID) => ID.ID;
    public static implicit operator ConnectionID(uint ID) => new ConnectionID(ID);
    public static implicit operator uint(ConnectionID ID) => ID.IDU;
    public static implicit operator ConnectionID(int ID) => new ConnectionID(ID);
    public static implicit operator int(ConnectionID ID) => ID.IDI;
    public static implicit operator ConnectionID(long ID) => new ConnectionID(ID);
    public static implicit operator long(ConnectionID ID) => ID.IDL;
}