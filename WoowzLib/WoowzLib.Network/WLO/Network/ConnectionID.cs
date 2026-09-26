namespace WLO.Network;

public readonly struct ConnectionID{
    public readonly ulong ID;
    public readonly bool  Special;
    
    public uint IDU => (uint)ID;
    public long IDL => (long)ID;
    public int  IDI => (int )ID;

    public ConnectionID(ulong ID, bool Special = false) { this.ID = ID; this.Special = Special; }
    public ConnectionID(uint  ID, bool Special = false) : this((ulong)ID, Special){}
    public ConnectionID(long  ID, bool Special = false) : this((ulong)ID, Special){}
    public ConnectionID(int   ID, bool Special = false) : this((ulong)ID, Special){}
}