using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics; 
 
 /* 
	Класс Vector4F сгенерирован с помощью WLGenerator
	Сгенерирован: 2026.09.12 03:04:37
 */ 

namespace WLO.Math; 

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct Vector4F : IEquatable<Vector4F>, WLI.Packable{
	public float X;
	public float Y;
	public float Z;
	public float W; 
 

	public float R { get => X; set => X = value; }
	public float G { get => Y; set => Y = value; }
	public float B { get => Z; set => Z = value; }
	public float A { get => W; set => W = value; } 
 

	public Vector2F XY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(X, Y); }
	public Vector2F YZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(Y, Z); }
	public Vector2F ZW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(Z, W); }
	public Vector2F WX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(W, X); }
	public Vector3F XYZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(X, Y, Z); }
	public Vector3F YZW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(Y, Z, W); }
	public Vector3F ZWX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(Z, W, X); }
	public Vector3F WXY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(W, X, Y); }
	public Vector4F YZWX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(Y, Z, W, X); }
	public Vector4F ZWXY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(Z, W, X, Y); }
	public Vector4F WXYZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(W, X, Y, Z); }
	public Vector2F XX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(X, X); }
	public Vector3F XXX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(X, X, X); }
	public Vector4F XXXX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(X, X, X, X); }
	public Vector2F YY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(Y, Y); }
	public Vector3F YYY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(Y, Y, Y); }
	public Vector4F YYYY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(Y, Y, Y, Y); }
	public Vector2F ZZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(Z, Z); }
	public Vector3F ZZZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(Z, Z, Z); }
	public Vector4F ZZZZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(Z, Z, Z, Z); }
	public Vector2F WW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(W, W); }
	public Vector3F WWW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(W, W, W); }
	public Vector4F WWWW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(W, W, W, W); }
	public Vector2F RG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(R, G); }
	public Vector2F GB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(G, B); }
	public Vector2F BA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(B, A); }
	public Vector2F AR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(A, R); }
	public Vector3F RGB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(R, G, B); }
	public Vector3F GBA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(G, B, A); }
	public Vector3F BAR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(B, A, R); }
	public Vector3F ARG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(A, R, G); }
	public Vector4F GBAR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(G, B, A, R); }
	public Vector4F BARG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(B, A, R, G); }
	public Vector4F ARGB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(A, R, G, B); }
	public Vector2F RR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(R, R); }
	public Vector3F RRR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(R, R, R); }
	public Vector4F RRRR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(R, R, R, R); }
	public Vector2F GG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(G, G); }
	public Vector3F GGG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(G, G, G); }
	public Vector4F GGGG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(G, G, G, G); }
	public Vector2F BB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(B, B); }
	public Vector3F BBB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(B, B, B); }
	public Vector4F BBBB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(B, B, B, B); }
	public Vector2F AA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2F(A, A); }
	public Vector3F AAA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3F(A, A, A); }
	public Vector4F AAAA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(A, A, A, A); } 
 

	public Vector4F(float X, float Y, float Z, float W){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
		this.W = W;
	}
	public Vector4F(float XYZW) : this(XYZW, XYZW, XYZW, XYZW){}
	public Vector4F(Vector2F VectorA, Vector2F VectorB) : this(VectorA.X, VectorA.Y, VectorB.X, VectorB.Y){}
	public Vector4F(Vector3F Vector, float W) : this(Vector.X, Vector.Y, Vector.Z, W){}
	public Vector4F(float X, float Y, float Z) : this(X, Y, Z, 1){} /* <- Типо цвет */ 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2F To2F() => new Vector2F(X, Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F To3F() => new Vector3F(X, Y, Z); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector128<float> ToSIMD() => Unsafe.As<Vector4F, Vector128<float>>(ref Unsafe.AsRef(in this)); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector4F(System.Numerics.Vector4 A) => new Vector4F(A.X, A.Y, A.Z, A.W);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator System.Numerics.Vector4(Vector4F A) => new System.Numerics.Vector4(A.X, A.Y, A.Z, A.W); 
 

	public static Vector4F Zero {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, 0, 0); }
	public static Vector4F One {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(1, 1, 1, 1); }
	public static Vector4F MOne {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(-1, -1, -1, -1); }
	public static Vector4F Half {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0.5f, 0.5f, 0.5f, 0.5f); }
	public static Vector4F MHalf {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(-0.5f, -0.5f, -0.5f, -0.5f); }
	public static Vector4F Right {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(1, 0, 0, 0); }
	public static Vector4F Left {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(-1, 0, 0, 0); }
	public static Vector4F AxisX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(1, 0, 0, 0); }
	public static Vector4F AxisMX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(-1, 0, 0, 0); }
	public static Vector4F Up {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 1, 0, 0); }
	public static Vector4F Down {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, -1, 0, 0); }
	public static Vector4F AxisY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 1, 0, 0); }
	public static Vector4F AxisMY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, -1, 0, 0); }
	public static Vector4F Front {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, 1, 0); }
	public static Vector4F Back {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, -1, 0); }
	public static Vector4F FrontGL {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, -1, 0); }
	public static Vector4F AxisZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, 1, 0); }
	public static Vector4F AxisMZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, -1, 0); }
	public static Vector4F Ana {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, 0, 1); }
	public static Vector4F Kata {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, 0, -1); }
	public static Vector4F AxisW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, 0, 1); }
	public static Vector4F AxisMW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(0, 0, 0, -1); }
	public static Vector4F MaxValue {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(WL.Math.MaxValueF, WL.Math.MaxValueF, WL.Math.MaxValueF, WL.Math.MaxValueF); }
	public static Vector4F MinValue {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(WL.Math.MinValueF, WL.Math.MinValueF, WL.Math.MinValueF, WL.Math.MinValueF); }
	public static Vector4F NAN {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4F(WL.Math.NANF, WL.Math.NANF, WL.Math.NANF, WL.Math.NANF); } 

	// ----------------------------------------------------------------------

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator +(Vector4F A, Vector4F B){
		Vector128<float> Result = Vector128.Add(A.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator +(Vector4F A, float B){
		Vector128<float> Result = Vector128.Add(A.ToSIMD(), Vector128.Create(B));
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator -(Vector4F A, Vector4F B){
		Vector128<float> Result = Vector128.Subtract(A.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator -(Vector4F A, float B){
		Vector128<float> Result = Vector128.Subtract(A.ToSIMD(), Vector128.Create(B));
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator *(Vector4F A, Vector4F B){
		Vector128<float> Result = Vector128.Multiply(A.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator *(Vector4F A, float B){
		Vector128<float> Result = Vector128.Multiply(A.ToSIMD(), Vector128.Create(B));
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator /(Vector4F A, Vector4F B){
		Vector128<float> Result = Vector128.Divide(A.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator /(Vector4F A, float B){
		Vector128<float> Result = Vector128.Divide(A.ToSIMD(), Vector128.Create(B));
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	} 
 

	public float this[int Index]{
		get => Index switch{
			0 => X,
			1 => Y,
			2 => Z,
			3 => W,
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
				case 3:
					W = value;
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out float X, out float Y, out float Z, out float W){
		X = this.X;
		Y = this.Y;
		Z = this.Z;
		W = this.W;
	} 

	// ----------------------------------------------------------------------

	public float LengthSquared {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.LengthSquared4F(X, Y, Z, W); }
	public float Length {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.Length4F(X, Y, Z, W); } 
 

	public Vector4F Normalize{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get{
			float L = Length;
			return L > WL.Math.EpsilonF ? this / L : Vector4F.Zero;
		}
	} 
 

	public Vector4F Negative{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get{
			Vector128<float> Result = Vector128.Negate(this.ToSIMD());
			return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator -(Vector4F A) => A.Negative; 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float DistanceSquared(Vector4F B){
		Vector128<float> Diff = Vector128.Subtract(this.ToSIMD(), B.ToSIMD());
		return Vector128.Dot(Diff, Diff);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Distance(Vector4F B) => WL.Math.Distance4F(X, Y, Z, W, B.X, B.Y, B.Z, B.W);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquared(Vector4F A, Vector4F B) => A.DistanceSquared(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(Vector4F A, Vector4F B) => A.Distance(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Dot(Vector4F B) => Vector128.Dot(this.ToSIMD(), B.ToSIMD());
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Dot(Vector4F A, Vector4F B) => A.Dot(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Lerp(Vector4F B, float T){
		Vector128<float> Result = Vector128.Add(this.ToSIMD(), Vector128.Multiply(Vector128.Subtract(B.ToSIMD(), this.ToSIMD()), Vector128.Create(T)));
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F LerpSafe(Vector4F B, float T) => Lerp(B, WL.Math.Clamp01F(T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F Lerp(Vector4F A, Vector4F B, float T) => A.Lerp(B, T);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F LerpSafe(Vector4F A, Vector4F B, float T) => A.LerpSafe(B, T); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Min(Vector4F B){
		Vector128<float> Result = Vector128.Min(this.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Min(float B){
		Vector128<float> Result = Vector128.Min(this.ToSIMD(), Vector128.Create(B));
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F Min(Vector4F A, Vector4F B) => A.Min(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F Min(Vector4F A, float B) => A.Min(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Max(Vector4F B){
		Vector128<float> Result = Vector128.Max(this.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Max(float B){
		Vector128<float> Result = Vector128.Max(this.ToSIMD(), Vector128.Create(B));
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F Max(Vector4F A, Vector4F B) => A.Max(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F Max(Vector4F A, float B) => A.Max(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Clamp(Vector4F Min, Vector4F Max) => this.Min(Max).Max(Min);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Clamp(float Min, float Max) => this.Min(Max).Max(Min);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F Clamp(Vector4F A, Vector4F Min, Vector4F Max) => A.Clamp(Min, Max);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F Clamp(Vector4F A, float Min, float Max) => A.Clamp(Min, Max); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Floor(){
		Vector128<float> Result = Vector128.Floor(this.ToSIMD());
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Round() => new Vector4F(WL.Math.RoundF(X), WL.Math.RoundF(Y), WL.Math.RoundF(Z), WL.Math.RoundF(W));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Ceil(){
		Vector128<float> Result = Vector128.Ceiling(this.ToSIMD());
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Abs(){
		Vector128<float> Result = Vector128.Abs(this.ToSIMD());
		return Unsafe.As<Vector128<float>, Vector4F>(ref Result);
	} 

	// ----------------------------------------------------------------------

	public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
		["XYZW"] = $"{X}|{Y}|{Z}|{W}"}; 

	public void __Unpack(Dictionary<string, object?> Data){
		string XYZW = WL.Packer.Get<string>(Data, "XYZW", "0|0|0|0")!; 

		string[] Parts = XYZW.Split('|');
		if (Parts.Length >= 4){
			float.TryParse(Parts[0], out X);
			float.TryParse(Parts[1], out Y);
			float.TryParse(Parts[2], out Z);
			float.TryParse(Parts[3], out W);
		}
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector4F Other) => X == Other.X && Y == Other.Y && Z == Other.Z && W == Other.W;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object? Object) => Object is Vector4F Other && Equals(Other); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector4F Left, Vector4F Right) => Left.Equals(Right);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector4F Left, Vector4F Right) => !(Left == Right); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToShortString() => $"{X}, {Y}, {Z}, {W}";
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString() => $"Vector4F({ToShortString()})"; 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
}