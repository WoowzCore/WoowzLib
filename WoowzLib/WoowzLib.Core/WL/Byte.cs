using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace WL;

public struct Byte{
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static byte BoolToByte(bool Value) => (byte)(Value ? 1 : 0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool ByteToBool(byte Value) => Value != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Bool8ToByte(bool B1 = false, bool B2 = false, bool B3 = false, bool B4 = false, bool B5 = false, bool B6 = false, bool B7 = false, bool B8 = false){
        byte Mask = 0;

        if(B1){ Mask |= 0B_0000_0001; }
        if(B2){ Mask |= 0B_0000_0010; }
        if(B3){ Mask |= 0B_0000_0100; }
        if(B4){ Mask |= 0B_0000_1000; }
        if(B5){ Mask |= 0B_0001_0000; }
        if(B6){ Mask |= 0B_0010_0000; }
        if(B7){ Mask |= 0B_0100_0000; }
        if(B8){ Mask |= 0B_1000_0000; }

        return Mask;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (bool B1, bool B2, bool B3, bool B4, bool B5, bool B6, bool B7, bool B8) ByteToBool8(byte Value) => (
        (Value & 0B_0000_0001) != 0,
        (Value & 0B_0000_0010) != 0,
        (Value & 0B_0000_0100) != 0,
        (Value & 0B_0000_1000) != 0,
        (Value & 0B_0001_0000) != 0,
        (Value & 0B_0010_0000) != 0,
        (Value & 0B_0100_0000) != 0,
        (Value & 0B_1000_0000) != 0
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SizeOf<T>() => Unsafe.SizeOf<T>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SizeOfArray<T>(int Length) => Length * SizeOf<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensure(ref byte[] Data, int Position, int Required){
        if(Position + Required > Data.Length){
            Array.Resize(ref Data, System.Math.Max(Data.Length * 2, Position + Required));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SizeOfString(string Value, Encoding Encoding){
        if(string.IsNullOrEmpty(Value)){ return 4; /* todo, жду норм функции или пояснений, длина значения "0" */ }
        return 4 + Encoding.GetByteCount(Value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SizeOfString(string Value) => SizeOfStringUTF8(Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SizeOfStringUTF8(string Value) => SizeOfString(Value, Encoding.UTF8);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SizeOfStringASCII(string Value) => SizeOfString(Value, Encoding.ASCII);

    public static byte[] CompressClear(byte[] Data, CompressionLevel CompressionLevel = CompressionLevel.Optimal){
        using MemoryStream MS = new MemoryStream();
        using(DeflateStream DS = new DeflateStream(MS, CompressionLevel)){
            DS.Write(Data, 0, Data.Length);
        }
        return MS.ToArray();
    }
    public static void DecompressClear(byte[] Result, MemoryStream MS){
        using(DeflateStream DS = new DeflateStream(MS, CompressionMode.Decompress, leaveOpen: true)){
            DS.ReadExactly(Result, 0, Result.Length);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] DecompressClear(byte[] Data, ref int Position, int L, int CL){
        byte[] Result = new byte[L];
        using(MemoryStream MS = new MemoryStream(Data, Position, CL)){
            DecompressClear(Result, MS);
        }
        Position += CL;
        return Result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] DecompressClear(byte[] Data, int L){
        byte[] Result = new byte[L];
        using(MemoryStream MS = new MemoryStream(Data)){
            DecompressClear(Result, MS);
        }
        return Result;
    }

    public static byte[] Compress(byte[] Data, CompressionLevel CompressionLevel = CompressionLevel.Optimal){
        byte[] Compressed = CompressClear(Data, CompressionLevel);
        int L  = Data.Length;
        int CL = Compressed.Length;
        byte[] Result = new byte[8 + 8 + CL];

        int Position = 0;
        WInt(Result, ref Position, L);
        WInt(Result, ref Position, CL);
        WBytes(Result, ref Position, Compressed);
        return Result;
    }

    public static byte[] Decompress(byte[] Data){
        int Position = 0;
        int L  = RInt(Data, ref Position);
        int CL = RInt(Data, ref Position);
        return DecompressClear(Data, ref Position, L, CL);
    }
    
    // ----------------------------------------------------------------------
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WByte(byte[] Data, ref int Position, byte Value) => Data[Position++] = Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static byte RByte(byte[] Data, ref int Position) => Data[Position++];

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WBool(byte[] Data, ref int Position, bool Value) => WByte(Data, ref Position, BoolToByte(Value));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool RBool(byte[] Data, ref int Position) => ByteToBool(RByte(Data, ref Position));
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WBool8(byte[] Data, ref int Position, bool B1 = false, bool B2 = false, bool B3 = false, bool B4 = false, bool B5 = false, bool B6 = false, bool B7 = false, bool B8 = false) => WByte(Data, ref Position, Bool8ToByte(B1, B2, B3, B4, B5, B6, B7, B8));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static (bool B1, bool B2, bool B3, bool B4, bool B5, bool B6, bool B7, bool B8) RBool8(byte[] Data, ref int Position) => ByteToBool8(RByte(Data, ref Position));
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WShort(byte[] Data, ref int Position, short Value) => W<short>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static short RShort(byte[] Data, ref int Position) => R<short>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WShortArray(byte[] Data, ref int Position, short[] Value) => WArray<short>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static short[] RShortArray(byte[] Data, ref int Position) => RArray<short>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WUInt(byte[] Data, ref int Position, uint Value) => W<uint>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint RUInt(byte[] Data, ref int Position) => R<uint>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WUIntArray(byte[] Data, ref int Position, uint[] Value) => WArray<uint>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint[] RUIntArray(byte[] Data, ref int Position) => RArray<uint>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WInt(byte[] Data, ref int Position, int Value) => W<int>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int RInt(byte[] Data, ref int Position) => R<int>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WIntArray(byte[] Data, ref int Position, int[] Value) => WArray<int>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int[] RIntArray(byte[] Data, ref int Position) => RArray<int>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WFloat(byte[] Data, ref int Position, float Value) => W<float>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float RFloat(byte[] Data, ref int Position) => R<float>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WFloatArray(byte[] Data, ref int Position, float[] Value) => WArray<float>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float[] RFloatArray(byte[] Data, ref int Position) => RArray<float>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WDouble(byte[] Data, ref int Position, double Value) => W<double>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double RDouble(byte[] Data, ref int Position) => R<double>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WDoubleArray(byte[] Data, ref int Position, double[] Value) => WArray<double>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double[] RDoubleArray(byte[] Data, ref int Position) => RArray<double>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WULong(byte[] Data, ref int Position, ulong Value) => W<ulong>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ulong RULong(byte[] Data, ref int Position) => R<ulong>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WULongArray(byte[] Data, ref int Position, ulong[] Value) => WArray<ulong>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ulong[] RULongArray(byte[] Data, ref int Position) => RArray<ulong>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WLong(byte[] Data, ref int Position, long Value) => W<long>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static long RLong(byte[] Data, ref int Position) => R<long>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WLongArray(byte[] Data, ref int Position, long[] Value) => WArray<long>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static long[] RLongArray(byte[] Data, ref int Position) => RArray<long>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void W<T>(byte[] Data, ref int Position, T Value) where T : unmanaged{
        int Size = SizeOf<T>();
        MemoryMarshal.Write(Data.AsSpan(Position), in Value);
        Position += Size;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T R<T>(byte[] Data, ref int Position) where T : unmanaged{
        int Size = SizeOf<T>();
        T Value = MemoryMarshal.Read<T>(Data.AsSpan(Position));
        Position += Size;
        return Value;
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WAt<T>(byte[] Data, int Position, T Value) where T : unmanaged => MemoryMarshal.Write(Data.AsSpan(Position), in Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T RAt<T>(byte[] Data, int Position) where T : unmanaged => MemoryMarshal.Read<T>(Data.AsSpan(Position));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T Peek<T>(byte[] Data, int Position) where T : unmanaged => RAt<T>(Data, Position);
    
    
    public static byte[] WBytes(byte[] Data, ref int Position, byte[] Value){
        System.Buffer.BlockCopy(Value, 0, Data, Position, Value.Length);
        Position += Value.Length;
        return Value;
    }
    public static byte[] RBytes(byte[] Data, ref int Position, int Count){
        byte[] Result = new byte[Count];
        System.Buffer.BlockCopy(Data, Position, Result, 0, Count);
        Position += Count;
        return Result;
    }


    public static byte[] WBytesCompressed(byte[] Data, ref int Position, byte[] Value, CompressionLevel CompressionLevel = CompressionLevel.Optimal) => WBytes(Data, ref Position, Compress(Value, CompressionLevel));
    public static byte[] RBytesCompressed(byte[] Data, ref int Position){
        int L  = RInt(Data, ref Position);
        int CL = RInt(Data, ref Position);
        return DecompressClear(Data, ref Position, L, CL);
    }
    
    
    public static void WString(byte[] Data, ref int Position, string Value, Encoding Encoding){
        if(string.IsNullOrEmpty(Value)){ WInt(Data, ref Position, 0); return; }
        int ByteCount = Encoding.GetByteCount(Value);
        WInt(Data, ref Position, ByteCount);
        Encoding.GetBytes(Value, 0, Value.Length, Data, Position);
        Position += ByteCount;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WString(byte[] Data, ref int Position, string Value) => WStringUTF8(Data, ref Position, Value);
    public static string RString(byte[] Data, ref int Position, Encoding Encoding){
        int ByteCount = RInt(Data, ref Position);
        if(ByteCount <= 0){ return string.Empty; }
        string Result = Encoding.GetString(Data, Position, ByteCount);
        Position += ByteCount;
        return Result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string RString(byte[] Data, ref int Position) => RStringUTF8(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WStringUTF8(byte[] Data, ref int Position, string Value) => WString(Data, ref Position, Value, Encoding.UTF8);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static string RStringUTF8(byte[] Data, ref int Position) => RString(Data, ref Position, Encoding.UTF8);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WStringASCII(byte[] Data, ref int Position, string Value) => WString(Data, ref Position, Value, Encoding.ASCII);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static string RStringASCII(byte[] Data, ref int Position) => RString(Data, ref Position, Encoding.ASCII);
    
    /// todo, отличается от WUInt тем, что записывает не строго в 4 байта, а как получится, динамически
    public static void WUIntVar(byte[] Data, ref int Position, uint Value){
        while(Value >= 0x80){
            Data[Position++] = (byte)(Value | 0x80);
            Value >>= 7;
        }
        Data[Position++] = (byte)Value;
    }
    public static uint RUIntVar(byte[] Data, ref int Position){
        uint Result = 0;
        int Shift = 0;
        while(true){
            byte Byte = Data[Position++];
            Result |= (uint)(Byte & 0x7F) << Shift;
            if((Byte & 0x80) == 0){ break; }
            Shift += 7;
        }
        return Result;
    }


    /// todo, это zigzag
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WIntVar(byte[] Data, ref int Position, int Value) => WUIntVar(Data, ref Position, (uint)((Value << 1) ^ (Value >> 31)));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int RIntVar(byte[] Data, ref int Position){
        uint V = RUIntVar(Data, ref Position);
        return (int)((V >> 1) ^ (uint)(-(int)(V & 1)));
    }
    
    
    public static void WULongVar(byte[] Data, ref int Position, ulong Value){
        while(Value >= 0x80){
            Data[Position++] = (byte)(Value | 0x80);
            Value >>= 7;
        }
        Data[Position++] = (byte)Value;
    }
    public static ulong RULongVar(byte[] Data, ref int Position){
        ulong Result = 0;
        int Shift = 0;
        while(true){
            byte Byte = Data[Position++];
            Result |= (ulong)(Byte & 0x7F) << Shift;
            if((Byte & 0x80) == 0){ break; }
            Shift += 7;
        }
        return Result;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WLongVar(byte[] Data, ref int Position, long Value) => WULongVar(Data, ref Position, (ulong)((Value << 1) ^ (Value >> 31)));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static long RLongVar(byte[] Data, ref int Position){
        ulong V = RULongVar(Data, ref Position);
        return (long)((V >> 1) ^ (ulong)(-(long)(V & 1)));
    }


    public static void WArray<T>(byte[] Data, ref int Position, T[] Array) where T : unmanaged{
        if(Array == null!){ WInt(Data, ref Position, -1); return; }
        int L = Array.Length;
        WInt(Data, ref Position, L);
        if(L <= 0){ return; }

        int SizeInBytes = SizeOfArray<T>(L);
        ReadOnlySpan<byte> Bytes = MemoryMarshal.Cast<T, byte>(Array.AsSpan());
        Bytes.CopyTo(Data.AsSpan(Position));
        Position += SizeInBytes;
    }
    public static T[] RArray<T>(byte[] Data, ref int Position) where T : unmanaged{
        int L = RInt(Data, ref Position);
        if(L == -1){ return null!; }
        if(L == 0){ return []; }

        T[] Result = new T[L];
        int SizeInBytes = SizeOfArray<T>(L);
        Span<byte> Bytes = MemoryMarshal.Cast<T, byte>(Result.AsSpan());
        Data.AsSpan(Position, SizeInBytes).CopyTo(Bytes);
        Position += SizeInBytes;
        return Result;
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WGUID(byte[] Data, ref int Position, Guid Value) => W<Guid>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Guid RGUID(byte[] Data, ref int Position) => R<Guid>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WDateTime(byte[] Data, ref int Position, DateTime Value) => WLong(Data, ref Position, Value.Ticks);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static DateTime RDateTime(byte[] Data, ref int Position) => new DateTime(RLong(Data, ref Position));


    public static void WSpan<T>(byte[] Data, ref int Position, ReadOnlySpan<T> Span) where T : unmanaged{
        int L = Span.Length;
        WInt(Data, ref Position, L);
        if(L <= 0){ return; }

        int SizeInBytes = SizeOfArray<T>(L);
        ReadOnlySpan<byte> Bytes = MemoryMarshal.Cast<T, byte>(Span);
        Bytes.CopyTo(Data.AsSpan(Position));
        Position += SizeInBytes;
    }
}