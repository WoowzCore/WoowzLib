namespace WLO.Network;

public struct Packet{
    public byte   Type; // todo, мб лучше ushort?
    public byte[] Data;

    public byte[] Pack(){
        byte[] Result = new byte[Data.Length + 1];
        int Position = 0;
        WL.Byte.WByte(Result, ref Position, Type);
        WL.Byte.WBytes(Result, ref Position, Data);
        return Result;
    }

    public Packet(byte Type, byte[] Data){
        this.Type = Type;
        this.Data = Data;
    }

    public Packet(byte Type, Buf Data){
        this.Type = Type;
        this.Data = Data.ToArray();
    }
}