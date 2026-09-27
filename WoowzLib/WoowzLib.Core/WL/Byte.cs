using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using WLO.Math;

namespace WL;

// todo, добавить half, quard для vector3f и всего такого, меньше точность но меньше размер

public struct Byte{
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static byte BoolToByte(bool Value) => (byte)(Value ? 1 : 0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool ByteToBool(byte Value) => Value != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Bool8ToByte(bool B1 = false, bool B2 = false, bool B3 = false, bool B4 = false, bool B5 = false, bool B6 = false, bool B7 = false, bool B8 = false){
        byte Mask = 0;

        if(B1){ Mask |= 1 << 0; }
        if(B2){ Mask |= 1 << 1; }
        if(B3){ Mask |= 1 << 2; }
        if(B4){ Mask |= 1 << 3; }
        if(B5){ Mask |= 1 << 4; }
        if(B6){ Mask |= 1 << 5; }
        if(B7){ Mask |= 1 << 6; }
        if(B8){ Mask |= 1 << 7; }

        return Mask;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (bool B1, bool B2, bool B3, bool B4, bool B5, bool B6, bool B7, bool B8) ByteToBool8(byte Value) => (
        (Value & (1 << 0)) != 0,
        (Value & (1 << 1)) != 0,
        (Value & (1 << 2)) != 0,
        (Value & (1 << 3)) != 0,
        (Value & (1 << 4)) != 0,
        (Value & (1 << 5)) != 0,
        (Value & (1 << 6)) != 0,
        (Value & (1 << 7)) != 0
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SizeOf     <T>(          ) where T : unmanaged => Unsafe.SizeOf<T>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SizeOfArray<T>(int Length) where T : unmanaged => Length * SizeOf<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensure(ref Span<byte> Data, int Position, int Required, bool SaveData = true){
        if(Position + Required > Data.Length){
            int NewSize = WL.Math.MaxI(Data.Length * 2, Position + Required);
            byte[] NewArray = new byte[NewSize];
            if(SaveData){ Data.CopyTo(NewArray); }
            Data = NewArray;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SizeOfString(string Value, Encoding Encoding){
        if(string.IsNullOrEmpty(Value)){ return 4; /* todo, жду норм функции или пояснений, длина значения "0" */ }
        return 4 + Encoding.GetByteCount(Value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SizeOfString     (string Value) => SizeOfStringUTF8(Value                );
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SizeOfStringUTF8 (string Value) => SizeOfString    (Value, Encoding.UTF8 );
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SizeOfStringASCII(string Value) => SizeOfString    (Value, Encoding.ASCII);

    public static byte[] CompressClear(ReadOnlySpan<byte> Data, CompressionLevel CompressionLevel = CompressionLevel.Optimal){
        using MemoryStream MS = new MemoryStream();
        using(DeflateStream DS = new DeflateStream(MS, CompressionLevel)){
            DS.Write(Data);
        }
        return MS.ToArray();
    }
    public static byte[] DecompressClear(ReadOnlySpan<byte> Data, int OriginalLength){
        byte[] Result = new byte[OriginalLength];
        using MemoryStream  MS = new MemoryStream(Data.ToArray());
        using DeflateStream DS = new DeflateStream(MS, CompressionMode.Decompress);
        DS.ReadExactly(Result);
        return Result;
    }
    public static byte[] DecompressClear(ReadOnlySpan<byte> Data, ref int Position, int OriginalLength, int CompressedLength){
        byte[] Result = DecompressClear(Data.Slice(Position, CompressedLength), OriginalLength);
        Position += CompressedLength;
        return Result;
    }

    public static byte[] Compress(ReadOnlySpan<byte> Data, CompressionLevel CompressionLevel = CompressionLevel.Optimal){
        byte[] Compressed = CompressClear(Data, CompressionLevel);
        byte[] Result = new byte[8 + Compressed.Length];

        int Position = 0;
        WInt(Result, ref Position, Data.Length);
        WInt(Result, ref Position, Compressed.Length);
        WBytes(Result, ref Position, Compressed);
        return Result;
    }

    public static byte[] Decompress(ReadOnlySpan<byte> Data){
        int Position = 0;
        int L  = RInt(Data, ref Position);
        int CL = RInt(Data, ref Position);
        return DecompressClear(Data, ref Position, L, CL);
    }
    
    // ----------------------------------------------------------------------
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WByte(        Span<byte> Data, ref int Position, byte Value) => Data[Position++] = Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static byte RByte(ReadOnlySpan<byte> Data, ref int Position            ) => Data[Position++];

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WBool(        Span<byte> Data, ref int Position, bool Value) => WByte(Data, ref Position, BoolToByte(Value));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool RBool(ReadOnlySpan<byte> Data, ref int Position            ) => ByteToBool(RByte(Data, ref Position));
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WBool8(Span<byte> Data, ref int Position, bool B1 = false, bool B2 = false, bool B3 = false, bool B4 = false, bool B5 = false, bool B6 = false, bool B7 = false, bool B8 = false) => WByte(Data, ref Position, Bool8ToByte(B1, B2, B3, B4, B5, B6, B7, B8));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static (bool B1, bool B2, bool B3, bool B4, bool B5, bool B6, bool B7, bool B8) RBool8(ReadOnlySpan<byte> Data, ref int Position) => ByteToBool8(RByte(Data, ref Position));
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void  WShort(        Span<byte> Data, ref int Position, short Value) => W<short>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static short RShort(ReadOnlySpan<byte> Data, ref int Position) => R<short>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void    WShortArray(        Span<byte> Data, ref int Position, ReadOnlySpan<short> Value) => WArray<short>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static short[] RShortArray(ReadOnlySpan<byte> Data, ref int Position) => RArray<short>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WUInt(        Span<byte> Data, ref int Position, uint Value) => W<uint>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint RUInt(ReadOnlySpan<byte> Data, ref int Position) => R<uint>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void   WUIntArray(        Span<byte> Data, ref int Position, ReadOnlySpan<uint> Value) => WArray<uint>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint[] RUIntArray(ReadOnlySpan<byte> Data, ref int Position) => RArray<uint>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WInt(        Span<byte> Data, ref int Position, int Value) => W<int>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int  RInt(ReadOnlySpan<byte> Data, ref int Position) => R<int>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void  WIntArray(        Span<byte> Data, ref int Position, ReadOnlySpan<int> Value) => WArray<int>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int[] RIntArray(ReadOnlySpan<byte> Data, ref int Position) => RArray<int>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void  WFloat(        Span<byte> Data, ref int Position, float Value) => W<float>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float RFloat(ReadOnlySpan<byte> Data, ref int Position) => R<float>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void    WFloatArray(        Span<byte> Data, ref int Position, ReadOnlySpan<float> Value) => WArray<float>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float[] RFloatArray(ReadOnlySpan<byte> Data, ref int Position) => RArray<float>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void   WDouble(        Span<byte> Data, ref int Position, double Value) => W<double>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double RDouble(ReadOnlySpan<byte> Data, ref int Position) => R<double>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WDoubleArray(        Span<byte> Data, ref int Position, ReadOnlySpan<double> Value) => WArray<double>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double[] RDoubleArray(ReadOnlySpan<byte> Data, ref int Position) => RArray<double>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void  WULong(        Span<byte> Data, ref int Position, ulong Value) => W<ulong>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ulong RULong(ReadOnlySpan<byte> Data, ref int Position) => R<ulong>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void    WULongArray(        Span<byte> Data, ref int Position, ReadOnlySpan<ulong> Value) => WArray<ulong>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ulong[] RULongArray(ReadOnlySpan<byte> Data, ref int Position) => RArray<ulong>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WLong(        Span<byte> Data, ref int Position, long Value) => W<long>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static long RLong(ReadOnlySpan<byte> Data, ref int Position) => R<long>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void   WLongArray(        Span<byte> Data, ref int Position, long[] Value) => WArray<long>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static long[] RLongArray(ReadOnlySpan<byte> Data, ref int Position) => RArray<long>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WVector2F(        Span<byte> Data, ref int Position, Vector2F Value) => W<Vector2F>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2F RVector2F(ReadOnlySpan<byte> Data, ref int Position) => R<Vector2F>(Data, ref Position);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WVector2D(        Span<byte> Data, ref int Position, Vector2D Value) => W<Vector2D>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2D RVector2D(ReadOnlySpan<byte> Data, ref int Position) => R<Vector2D>(Data, ref Position);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WVector2I(        Span<byte> Data, ref int Position, Vector2I Value) => W<Vector2I>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2I RVector2I(ReadOnlySpan<byte> Data, ref int Position) => R<Vector2I>(Data, ref Position);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WVector3F(        Span<byte> Data, ref int Position, Vector3F Value) => W<Vector3F>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3F RVector3F(ReadOnlySpan<byte> Data, ref int Position) => R<Vector3F>(Data, ref Position);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WVector3D(        Span<byte> Data, ref int Position, Vector3D Value) => W<Vector3D>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3D RVector3D(ReadOnlySpan<byte> Data, ref int Position) => R<Vector3D>(Data, ref Position);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WVector3I(        Span<byte> Data, ref int Position, Vector3I Value) => W<Vector3I>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3I RVector3I(ReadOnlySpan<byte> Data, ref int Position) => R<Vector3I>(Data, ref Position);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WVector4F(        Span<byte> Data, ref int Position, Vector4F Value) => W<Vector4F>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector4F RVector4F(ReadOnlySpan<byte> Data, ref int Position) => R<Vector4F>(Data, ref Position);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WVector4D(        Span<byte> Data, ref int Position, Vector4D Value) => W<Vector4D>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector4D RVector4D(ReadOnlySpan<byte> Data, ref int Position) => R<Vector4D>(Data, ref Position);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WVector4I(        Span<byte> Data, ref int Position, Vector4I Value) => W<Vector4I>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector4I RVector4I(ReadOnlySpan<byte> Data, ref int Position) => R<Vector4I>(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void W<T>(Span<byte> Data, ref int Position, T Value) where T : unmanaged{
        MemoryMarshal.Write(Data[Position..], Value);
        Position += SizeOf<T>();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T R<T>(ReadOnlySpan<byte> Data, ref int Position) where T : unmanaged{
        T Value = MemoryMarshal.Read<T>(Data[Position..]);
        Position += SizeOf<T>();
        return Value;
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WAt<T>(Span<byte> Data, int Position, T Value) where T : unmanaged => MemoryMarshal.Write(Data[Position..], in Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T RAt<T>(ReadOnlySpan<byte> Data, int Position) where T : unmanaged => MemoryMarshal.Read<T>(Data[Position..]);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T Peek<T>(ReadOnlySpan<byte> Data, int Position) where T : unmanaged => RAt<T>(Data, Position);
    
    
    public static ReadOnlySpan<byte> WBytes(Span<byte> Data, ref int Position, ReadOnlySpan<byte> Value){
        Value.CopyTo(Data[Position..]);
        Position += Value.Length;
        return Value;
    }
    public static ReadOnlySpan<byte> RBytes(ReadOnlySpan<byte> Data, ref int Position, int Count){
        ReadOnlySpan<byte> Result = Data.Slice(Position, Count);
        Position += Count;
        return Result;
    }


    public static ReadOnlySpan<byte> WBytesCompressed(Span<byte> Data, ref int Position, ReadOnlySpan<byte> Value, CompressionLevel CompressionLevel = CompressionLevel.Optimal) => WBytes(Data, ref Position, Compress(Value, CompressionLevel));
    public static byte[] RBytesCompressed(ReadOnlySpan<byte> Data, ref int Position){
        int L  = RInt(Data, ref Position);
        int CL = RInt(Data, ref Position);
        return DecompressClear(Data, ref Position, L, CL);
    }
    
    
    public static void WString(Span<byte> Data, ref int Position, string Value, Encoding Encoding){
        if(string.IsNullOrEmpty(Value)){ WInt(Data, ref Position, 0); return; }
        int ByteCount = Encoding.GetByteCount(Value);
        WInt(Data, ref Position, ByteCount);
        Encoding.GetBytes(Value, Data[Position..]);
        Position += ByteCount;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WString(Span<byte> Data, ref int Position, string Value) => WStringUTF8(Data, ref Position, Value);
    public static string RString(ReadOnlySpan<byte> Data, ref int Position, Encoding Encoding){
        int ByteCount = RInt(Data, ref Position);
        if(ByteCount <= 0){ return string.Empty; }
        string Result = Encoding.GetString(Data.Slice(Position, ByteCount));
        Position += ByteCount;
        return Result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string RString(ReadOnlySpan<byte> Data, ref int Position) => RStringUTF8(Data, ref Position);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void   WStringUTF8(        Span<byte> Data, ref int Position, string Value) => WString(Data, ref Position, Value, Encoding.UTF8);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static string RStringUTF8(ReadOnlySpan<byte> Data, ref int Position              ) => RString(Data, ref Position, Encoding.UTF8);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void   WStringASCII(        Span<byte> Data, ref int Position, string Value) => WString(Data, ref Position, Value, Encoding.ASCII);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static string RStringASCII(ReadOnlySpan<byte> Data, ref int Position              ) => RString(Data, ref Position, Encoding.ASCII);
    
    /// todo, отличается от WUInt тем, что записывает не строго в 4 байта, а как получится, динамически
    public static void WUIntVar(Span<byte> Data, ref int Position, uint Value){
        while(Value >= 0x80){
            Data[Position++] = (byte)(Value | 0x80);
            Value >>= 7;
        }
        Data[Position++] = (byte)Value;
    }
    public static uint RUIntVar(ReadOnlySpan<byte> Data, ref int Position){
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WIntVar(        Span<byte> Data, ref int Position, int Value) => WUIntVar(Data, ref Position, (uint)((Value << 1) ^ (Value >> 31)));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int  RIntVar(ReadOnlySpan<byte> Data, ref int Position){
        uint V = RUIntVar(Data, ref Position);
        return (int)((V >> 1) ^ (uint)(-(int)(V & 1)));
    }
    
    
    public static void WULongVar(Span<byte> Data, ref int Position, ulong Value){
        while(Value >= 0x80){
            Data[Position++] = (byte)(Value | 0x80);
            Value >>= 7;
        }
        Data[Position++] = (byte)Value;
    }
    public static ulong RULongVar(ReadOnlySpan<byte> Data, ref int Position){
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WLongVar(        Span<byte> Data, ref int Position, long Value) => WULongVar(Data, ref Position, (ulong)((Value << 1) ^ (Value >> 63)));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static long RLongVar(ReadOnlySpan<byte> Data, ref int Position){
        ulong V = RULongVar(Data, ref Position);
        return (long)((V >> 1) ^ (ulong)(-(long)(V & 1)));
    }


    public static void WArray<T>(Span<byte> Data, ref int Position, ReadOnlySpan<T> Array) where T : unmanaged{
        WInt(Data, ref Position, Array.Length);
        if(Array.Length <= 0){ return; }

        ReadOnlySpan<byte> Bytes = MemoryMarshal.Cast<T, byte>(Array);
        Bytes.CopyTo(Data[Position..]);
        Position += Bytes.Length;
    }
    public static T[] RArray<T>(ReadOnlySpan<byte> Data, ref int Position) where T : unmanaged{
        int L = RInt(Data, ref Position);
        if(L == 0){ return []; }

        T[] Result = new T[L];
        int SizeInBytes = SizeOfArray<T>(L);
        Span<byte> Bytes = MemoryMarshal.Cast<T, byte>(Result);
        Data.Slice(Position, SizeInBytes).CopyTo(Bytes);
        Position += SizeInBytes;
        return Result;
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void WGUID(        Span<byte> Data, ref int Position, Guid Value) => W<Guid>(Data, ref Position, Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Guid RGUID(ReadOnlySpan<byte> Data, ref int Position) => R<Guid>(Data, ref Position); 
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void     WDateTime(        Span<byte> Data, ref int Position, DateTime Value) => WLong(Data, ref Position, Value.Ticks);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static DateTime RDateTime(ReadOnlySpan<byte> Data, ref int Position) => new DateTime(RLong(Data, ref Position));
}