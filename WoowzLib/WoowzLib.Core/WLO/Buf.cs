using System.Runtime.CompilerServices;

namespace WLO;

public class Buf{
    public byte[] Data;
    public int    Position;

    public Buf(){
        Data     = new byte[64];
        Position = 0;
    }
    
    public Buf(int InitialCapacity){
        Data     = new byte[InitialCapacity];
        Position = 0;
    }

    public Buf(byte[] Data, int Position = 0){
        this.Data     = Data;
        this.Position = Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Buf Ensure(int Required){
        if(Position + Required > Data.Length){
            Array.Resize(ref Data, System.Math.Max(Data.Length * 2, Position + Required));
        }
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToArray(){
        byte[] Result = new byte[Position];
        Array.Copy(Data, 0, Result, 0, Position);
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> AsSpan() => Data.AsSpan(0, Position);
    
    // ----------------------------------------------------------------------
    
    public void WByte  (byte   Value){ Ensure(1); WL.Byte.WByte  (Data, ref Position, Value); }
    public void WInt   (int    Value){ Ensure(4); WL.Byte.WInt   (Data, ref Position, Value); }
    public void WUInt  (uint   Value){ Ensure(4); WL.Byte.WUInt  (Data, ref Position, Value); }
    public void WLong  (long   Value){ Ensure(8); WL.Byte.WLong  (Data, ref Position, Value); }
    public void WULong (ulong  Value){ Ensure(8); WL.Byte.WULong (Data, ref Position, Value); }
    public void WFloat (float  Value){ Ensure(4); WL.Byte.WFloat (Data, ref Position, Value); }
    public void WDouble(double Value){ Ensure(8); WL.Byte.WDouble(Data, ref Position, Value); }

    public void WString(string Value){
        if(string.IsNullOrEmpty(Value)){ WInt(0); return; }
        int Count = WL.Byte.SizeOfStringUTF8(Value);
        Ensure(4 + Count);
        WL.Byte.WStringUTF8(Data, ref Position, Value);
    }
    
    public void W<T>(T Value) where T : unmanaged{
        Ensure(WL.Byte.SizeOf<T>());
        WL.Byte.W<T>(Data, ref Position, Value);
    }

    public void WBytes(byte[] Value){ Ensure(Value.Length); WL.Byte.WBytes(Data, ref Position, Value); }
}