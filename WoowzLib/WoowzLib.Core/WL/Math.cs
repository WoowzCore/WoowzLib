using System.Runtime.CompilerServices;
using WLO.Math;

namespace WL;

public struct Math{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Expand3D(ref Vector3F Min, ref Vector3F Max, Vector3F Position){
        if(Position.X < Min.X){ Min.X = Position.X; }
        if(Position.Y < Min.Y){ Min.Y = Position.Y; }
        if(Position.Z < Min.Z){ Min.Z = Position.Z; }
            
        if(Position.X > Max.X){ Max.X = Position.X; }
        if(Position.Y > Max.Y){ Max.Y = Position.Y; }
        if(Position.Z > Max.Z){ Max.Z = Position.Z; }
    }
    
    /// Делает массив чисел уникальными
    public static void GenerateSequential(Span<uint> Buffer, uint Start = 0){
        for(int i = 0; i < Buffer.Length; i++){ Buffer[i] = Start + (uint)i; }
    }
}