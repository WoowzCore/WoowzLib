using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics; 
 
 /* 
	Класс Vector3F сгенерирован с помощью WLGenerator
	Сгенерирован: 2026.09.14 00:26:16
 */ 

namespace WLO.Math; 

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct Vector3F : IEquatable<Vector3F>, WLI.Packable{
	public float X;
	public float Y;
	public float Z; 
 

	public float W { get => X; set => X = value; }
	public float H { get => Y; set => Y = value; }
	public float D { get => Z; set => Z = value; } 
 

	public float R { get => X; set => X = value; }
	public float G { get => Y; set => Y = value; }
	public float B { get => Z; set => Z = value; } 
 

	public float Pitch { get => X; set => X = value; }
	public float Yaw { get => Y; set => Y = value; }
	public float Roll { get => Z; set => Z = value; } 
 

	public Vector2F XY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(X, Y); }
	public Vector2F YZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(Y, Z); }
	public Vector2F ZX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(Z, X); }
	public Vector3F YZX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(Y, Z, X); }
	public Vector3F ZXY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(Z, X, Y); }
	public Vector2F XX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(X, X); }
	public Vector3F XXX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(X, X, X); }
	public Vector2F YY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(Y, Y); }
	public Vector3F YYY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(Y, Y, Y); }
	public Vector2F ZZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(Z, Z); }
	public Vector3F ZZZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(Z, Z, Z); }
	public Vector2F RG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(R, G); }
	public Vector2F GB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(G, B); }
	public Vector2F BR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(B, R); }
	public Vector3F GBR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(G, B, R); }
	public Vector3F BRG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(B, R, G); }
	public Vector2F RR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(R, R); }
	public Vector3F RRR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(R, R, R); }
	public Vector2F GG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(G, G); }
	public Vector3F GGG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(G, G, G); }
	public Vector2F BB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(B, B); }
	public Vector3F BBB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(B, B, B); } 
 

	public Vector3F(float X, float Y, float Z){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
	}
	public Vector3F(float XYZ) : this(XYZ, XYZ, XYZ){}
	public Vector3F(Vector2F Vector, float Z) : this(Vector.X, Vector.Y, Z){} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2I To2I() => (Vector2I)this;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2F To2F() => (Vector2F)this;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2D To2D() => (Vector2D)this;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I To3I() => (Vector3I)this;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3D To3D() => (Vector3D)this;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I To4I() => (Vector4I)this;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F To4F() => (Vector4F)this;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D To4D() => (Vector4D)this; 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector128<float> ToSIMD() => Vector128.Create(X, Y, Z, 0); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector3F(System.Numerics.Vector3 A) => new Vector3F(A.X, A.Y, A.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator System.Numerics.Vector3(Vector3F A) => new System.Numerics.Vector3(A.X, A.Y, A.Z); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator Vector2I(Vector3F A) => new Vector2I((int)A.X, (int)A.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator Vector2F(Vector3F A) => new Vector2F(A.X, A.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator Vector2D(Vector3F A) => new Vector2D((double)A.X, (double)A.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator Vector3I(Vector3F A) => new Vector3I((int)A.X, (int)A.Y, (int)A.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector3D(Vector3F A) => new Vector3D((double)A.X, (double)A.Y, (double)A.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator Vector4I(Vector3F A) => new Vector4I((int)A.X, (int)A.Y, (int)A.Z, 0);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector4F(Vector3F A) => new Vector4F(A.X, A.Y, A.Z, 0);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector4D(Vector3F A) => new Vector4D((double)A.X, (double)A.Y, (double)A.Z, 0); 
 

	public static Vector3F Zero {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, 0, 0); }
	public static Vector3F One {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(1, 1, 1); }
	public static Vector3F MOne {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(-1, -1, -1); }
	public static Vector3F Half {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0.5f, 0.5f, 0.5f); }
	public static Vector3F MHalf {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(-0.5f, -0.5f, -0.5f); }
	public static Vector3F Right {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(1, 0, 0); }
	public static Vector3F Left {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(-1, 0, 0); }
	public static Vector3F AxisX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(1, 0, 0); }
	public static Vector3F AxisMX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(-1, 0, 0); }
	public static Vector3F Up {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, 1, 0); }
	public static Vector3F Down {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, -1, 0); }
	public static Vector3F AxisY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, 1, 0); }
	public static Vector3F AxisMY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, -1, 0); }
	public static Vector3F Front {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, 0, 1); }
	public static Vector3F Back {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, 0, -1); }
	public static Vector3F FrontGL {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, 0, -1); }
	public static Vector3F AxisZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, 0, 1); }
	public static Vector3F AxisMZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(0, 0, -1); }
	public static Vector3F MaxValue {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(WL.Math.MaxValueF, WL.Math.MaxValueF, WL.Math.MaxValueF); }
	public static Vector3F MinValue {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(WL.Math.MinValueF, WL.Math.MinValueF, WL.Math.MinValueF); }
	public static Vector3F NAN {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(WL.Math.NANF, WL.Math.NANF, WL.Math.NANF); } 

	// ----------------------------------------------------------------------

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator +(Vector3F A, Vector3F B) => new Vector3F(A.X + B.X, A.Y + B.Y, A.Z + B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator +(Vector3F A, float B) => new Vector3F(A.X + B, A.Y + B, A.Z + B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator -(Vector3F A, Vector3F B) => new Vector3F(A.X - B.X, A.Y - B.Y, A.Z - B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator -(Vector3F A, float B) => new Vector3F(A.X - B, A.Y - B, A.Z - B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator *(Vector3F A, Vector3F B) => new Vector3F(A.X * B.X, A.Y * B.Y, A.Z * B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator *(Vector3F A, float B) => new Vector3F(A.X * B, A.Y * B, A.Z * B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator /(Vector3F A, Vector3F B) => new Vector3F(A.X / B.X, A.Y / B.Y, A.Z / B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator /(Vector3F A, float B) => new Vector3F(A.X / B, A.Y / B, A.Z / B); 
 

	public float this[int Index]{
		get => Index switch{
			0 => X,
			1 => Y,
			2 => Z,
			var _ => throw new IndexOutOfRangeException()};
		set{
			switch (Index){
				case 0:
					X = value;
					break;
				case 1:
					Y = value;
					break;
				case 2:
					Z = value;
					break;
				default:
					throw new IndexOutOfRangeException();
			}
		}
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out float X, out float Y){
		X = this.X;
		Y = this.Y;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out float X, out float Y, out float Z){
		X = this.X;
		Y = this.Y;
		Z = this.Z;
	} 

	// ----------------------------------------------------------------------

	public float LengthSquared {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.LengthSquared3F(X, Y, Z); }
	public float Length {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.Length3F(X, Y, Z); } 
 

	public Vector3F Normalize{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get{
			float L = Length;
			return L > WL.Math.EpsilonF ? this / L : Vector3F.Zero;
		}
	} 
 

	public Vector3F Negative {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => -this; }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator -(Vector3F A) => new Vector3F(-A.X, -A.Y, -A.Z); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float DistanceSquared(Vector3F B) => WL.Math.DistanceSquared3F(X, Y, Z, B.X, B.Y, B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Distance(Vector3F B) => WL.Math.Distance3F(X, Y, Z, B.X, B.Y, B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquared(Vector3F A, Vector3F B) => A.DistanceSquared(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(Vector3F A, Vector3F B) => A.Distance(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Dot(Vector3F B) => WL.Math.Dot3F(X, Y, Z, B.X, B.Y, B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Dot(Vector3F A, Vector3F B) => A.Dot(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Cross(Vector3F B) => new Vector3F((Y * B.Z) - (Z * B.Y), (Z * B.X) - (X * B.Z), (X * B.Y) - (Y * B.X));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F Cross(Vector3F A, Vector3F B) => A.Cross(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Lerp(Vector3F B, float T) => new Vector3F(WL.Math.LerpF(X, B.X, T), WL.Math.LerpF(Y, B.Y, T), WL.Math.LerpF(Z, B.Z, T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F LerpSafe(Vector3F B, float T) => Lerp(B, WL.Math.Clamp01F(T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F Lerp(Vector3F A, Vector3F B, float T) => A.Lerp(B, T);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F LerpSafe(Vector3F A, Vector3F B, float T) => A.LerpSafe(B, T); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Min(Vector3F B) => new Vector3F(WL.Math.MinF(X, B.X), WL.Math.MinF(Y, B.Y), WL.Math.MinF(Z, B.Z));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Min(float B) => new Vector3F(WL.Math.MinF(X, B), WL.Math.MinF(Y, B), WL.Math.MinF(Z, B));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F Min(Vector3F A, Vector3F B) => A.Min(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F Min(Vector3F A, float B) => A.Min(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Max(Vector3F B) => new Vector3F(WL.Math.MaxF(X, B.X), WL.Math.MaxF(Y, B.Y), WL.Math.MaxF(Z, B.Z));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Max(float B) => new Vector3F(WL.Math.MaxF(X, B), WL.Math.MaxF(Y, B), WL.Math.MaxF(Z, B));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F Max(Vector3F A, Vector3F B) => A.Max(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F Max(Vector3F A, float B) => A.Max(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Clamp(Vector3F Min, Vector3F Max) => this.Min(Max).Max(Min);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Clamp(float Min, float Max) => this.Min(Max).Max(Min);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F Clamp(Vector3F A, Vector3F Min, Vector3F Max) => A.Clamp(Min, Max);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F Clamp(Vector3F A, float Min, float Max) => A.Clamp(Min, Max); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Floor() => new Vector3F(WL.Math.FloorF(X), WL.Math.FloorF(Y), WL.Math.FloorF(Z));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Round() => new Vector3F(WL.Math.RoundF(X), WL.Math.RoundF(Y), WL.Math.RoundF(Z));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Ceil() => new Vector3F(WL.Math.CeilF(X), WL.Math.CeilF(Y), WL.Math.CeilF(Z)); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F GetFront(float Yaw, float Pitch) => new Vector3F(WL.Math.SinF(Yaw) * WL.Math.CosF(Pitch), -WL.Math.SinF(Pitch), WL.Math.CosF(Yaw) * WL.Math.CosF(Pitch)).Normalize;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F GetFront(float Yaw) => new Vector3F(WL.Math.SinF(Yaw), 0, WL.Math.CosF(Yaw)).Normalize; 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F GetBack(float Yaw, float Pitch) => Vector3F.GetFront(Yaw, Pitch).Negative;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F GetBack(float Yaw) => Vector3F.GetFront(Yaw).Negative; 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F GetRight(Vector3F Front){
		if (WL.Math.AbsF(Front.Y) > 0.999f){
			return Vector3F.Cross(Vector3F.Front, Front).Normalize;
		}
		return Vector3F.Cross(Vector3F.Up, Front).Normalize;
	} 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F GetLeft(Vector3F Front) => Vector3F.GetRight(Front).Negative; 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F GetUp(Vector3F Front, Vector3F Right) => Vector3F.Cross(Right, Front).Normalize; 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F GetDown(Vector3F Front, Vector3F Right) => Vector3F.GetUp(Front, Right).Negative; 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public (float Yaw, float Pitch) GetAngle(){
		float L = Length;
		if (L < 0.0001f){
			return (0, 0);
		}
		Vector3F Direction = this / L;
		return (WL.Math.ATan2F(Direction.X, Direction.Z), WL.Math.ASinF(WL.Math.Clamp11F(-Direction.Y)));
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Abs() => new Vector3F(WL.Math.AbsF(X), WL.Math.AbsF(Y), WL.Math.AbsF(Z)); 

	// ----------------------------------------------------------------------

	public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
		["XYZ"] = $"{X}|{Y}|{Z}"}; 

	public void __Unpack(Dictionary<string, object?> Data){
		string XYZ = WL.Packer.Get<string>(Data, "XYZ", "0|0|0")!; 

		string[] Parts = XYZ.Split('|');
		if (Parts.Length >= 3){
			float.TryParse(Parts[0], out X);
			float.TryParse(Parts[1], out Y);
			float.TryParse(Parts[2], out Z);
		}
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector3F Other) => X == Other.X && Y == Other.Y && Z == Other.Z;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object? Object) => Object is Vector3F Other && Equals(Other); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector3F Left, Vector3F Right) => Left.Equals(Right);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector3F Left, Vector3F Right) => !(Left == Right); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToShortString() => $"{X}, {Y}, {Z}";
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString() => $"Vector3F({ToShortString()})"; 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => HashCode.Combine(X, Y, Z);
}