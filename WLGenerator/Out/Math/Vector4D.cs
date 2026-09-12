using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics; 
 
 /* 
	Класс Vector4D сгенерирован с помощью WLGenerator
	Сгенерирован: 2026.09.12 03:04:37
 */ 

namespace WLO.Math; 

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct Vector4D : IEquatable<Vector4D>, WLI.Packable{
	public double X;
	public double Y;
	public double Z;
	public double W; 
 

	public double R { get => X; set => X = value; }
	public double G { get => Y; set => Y = value; }
	public double B { get => Z; set => Z = value; }
	public double A { get => W; set => W = value; } 
 

	public Vector2D XY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(X, Y); }
	public Vector2D YZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(Y, Z); }
	public Vector2D ZW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(Z, W); }
	public Vector2D WX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(W, X); }
	public Vector3D XYZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(X, Y, Z); }
	public Vector3D YZW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(Y, Z, W); }
	public Vector3D ZWX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(Z, W, X); }
	public Vector3D WXY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(W, X, Y); }
	public Vector4D YZWX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(Y, Z, W, X); }
	public Vector4D ZWXY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(Z, W, X, Y); }
	public Vector4D WXYZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(W, X, Y, Z); }
	public Vector2D XX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(X, X); }
	public Vector3D XXX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(X, X, X); }
	public Vector4D XXXX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(X, X, X, X); }
	public Vector2D YY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(Y, Y); }
	public Vector3D YYY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(Y, Y, Y); }
	public Vector4D YYYY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(Y, Y, Y, Y); }
	public Vector2D ZZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(Z, Z); }
	public Vector3D ZZZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(Z, Z, Z); }
	public Vector4D ZZZZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(Z, Z, Z, Z); }
	public Vector2D WW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(W, W); }
	public Vector3D WWW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(W, W, W); }
	public Vector4D WWWW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(W, W, W, W); }
	public Vector2D RG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(R, G); }
	public Vector2D GB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(G, B); }
	public Vector2D BA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(B, A); }
	public Vector2D AR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(A, R); }
	public Vector3D RGB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(R, G, B); }
	public Vector3D GBA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(G, B, A); }
	public Vector3D BAR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(B, A, R); }
	public Vector3D ARG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(A, R, G); }
	public Vector4D GBAR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(G, B, A, R); }
	public Vector4D BARG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(B, A, R, G); }
	public Vector4D ARGB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(A, R, G, B); }
	public Vector2D RR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(R, R); }
	public Vector3D RRR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(R, R, R); }
	public Vector4D RRRR {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(R, R, R, R); }
	public Vector2D GG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(G, G); }
	public Vector3D GGG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(G, G, G); }
	public Vector4D GGGG {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(G, G, G, G); }
	public Vector2D BB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(B, B); }
	public Vector3D BBB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(B, B, B); }
	public Vector4D BBBB {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(B, B, B, B); }
	public Vector2D AA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector2D(A, A); }
	public Vector3D AAA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector3D(A, A, A); }
	public Vector4D AAAA {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(A, A, A, A); } 
 

	public Vector4D(double X, double Y, double Z, double W){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
		this.W = W;
	}
	public Vector4D(double XYZW) : this(XYZW, XYZW, XYZW, XYZW){}
	public Vector4D(Vector2D VectorA, Vector2D VectorB) : this(VectorA.X, VectorA.Y, VectorB.X, VectorB.Y){}
	public Vector4D(Vector3D Vector, double W) : this(Vector.X, Vector.Y, Vector.Z, W){}
	public Vector4D(double X, double Y, double Z) : this(X, Y, Z, 1){} /* <- Типо цвет */ 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2D To2D() => new Vector2D(X, Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3D To3D() => new Vector3D(X, Y, Z); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector256<double> ToSIMD() => Unsafe.As<Vector4D, Vector256<double>>(ref Unsafe.AsRef(in this)); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector4D(System.Numerics.Vector4 A) => new Vector4D(A.X, A.Y, A.Z, A.W);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator System.Numerics.Vector4(Vector4D A) => new System.Numerics.Vector4((float)A.X, (float)A.Y, (float)A.Z, (float)A.W); 
 

	public static Vector4D Zero {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, 0, 0); }
	public static Vector4D One {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(1, 1, 1, 1); }
	public static Vector4D MOne {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(-1, -1, -1, -1); }
	public static Vector4D Half {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0.5, 0.5, 0.5, 0.5); }
	public static Vector4D MHalf {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(-0.5, -0.5, -0.5, -0.5); }
	public static Vector4D Right {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(1, 0, 0, 0); }
	public static Vector4D Left {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(-1, 0, 0, 0); }
	public static Vector4D AxisX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(1, 0, 0, 0); }
	public static Vector4D AxisMX {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(-1, 0, 0, 0); }
	public static Vector4D Up {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 1, 0, 0); }
	public static Vector4D Down {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, -1, 0, 0); }
	public static Vector4D AxisY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 1, 0, 0); }
	public static Vector4D AxisMY {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, -1, 0, 0); }
	public static Vector4D Front {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, 1, 0); }
	public static Vector4D Back {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, -1, 0); }
	public static Vector4D FrontGL {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, -1, 0); }
	public static Vector4D AxisZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, 1, 0); }
	public static Vector4D AxisMZ {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, -1, 0); }
	public static Vector4D Ana {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, 0, 1); }
	public static Vector4D Kata {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, 0, -1); }
	public static Vector4D AxisW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, 0, 1); }
	public static Vector4D AxisMW {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(0, 0, 0, -1); }
	public static Vector4D MaxValue {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(WL.Math.MaxValueD, WL.Math.MaxValueD, WL.Math.MaxValueD, WL.Math.MaxValueD); }
	public static Vector4D MinValue {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(WL.Math.MinValueD, WL.Math.MinValueD, WL.Math.MinValueD, WL.Math.MinValueD); }
	public static Vector4D NAN {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new Vector4D(WL.Math.NAND, WL.Math.NAND, WL.Math.NAND, WL.Math.NAND); } 

	// ----------------------------------------------------------------------

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D operator +(Vector4D A, Vector4D B){
		Vector256<double> Result = Vector256.Add(A.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D operator +(Vector4D A, double B){
		Vector256<double> Result = Vector256.Add(A.ToSIMD(), Vector256.Create(B));
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D operator -(Vector4D A, Vector4D B){
		Vector256<double> Result = Vector256.Subtract(A.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D operator -(Vector4D A, double B){
		Vector256<double> Result = Vector256.Subtract(A.ToSIMD(), Vector256.Create(B));
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D operator *(Vector4D A, Vector4D B){
		Vector256<double> Result = Vector256.Multiply(A.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D operator *(Vector4D A, double B){
		Vector256<double> Result = Vector256.Multiply(A.ToSIMD(), Vector256.Create(B));
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D operator /(Vector4D A, Vector4D B){
		Vector256<double> Result = Vector256.Divide(A.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D operator /(Vector4D A, double B){
		Vector256<double> Result = Vector256.Divide(A.ToSIMD(), Vector256.Create(B));
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	} 
 

	public double this[int Index]{
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
	public void Deconstruct(out double X, out double Y){
		X = this.X;
		Y = this.Y;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out double X, out double Y, out double Z){
		X = this.X;
		Y = this.Y;
		Z = this.Z;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out double X, out double Y, out double Z, out double W){
		X = this.X;
		Y = this.Y;
		Z = this.Z;
		W = this.W;
	} 

	// ----------------------------------------------------------------------

	public double LengthSquared {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.LengthSquared4D(X, Y, Z, W); }
	public double Length {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.Length4D(X, Y, Z, W); } 
 

	public Vector4D Normalize{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get{
			double L = Length;
			return L > WL.Math.EpsilonD ? this / L : Vector4D.Zero;
		}
	} 
 

	public Vector4D Negative {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this * -1; }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D operator -(Vector4D A) => A.Negative; 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double DistanceSquared(Vector4D B){
		Vector256<double> Diff = Vector256.Subtract(this.ToSIMD(), B.ToSIMD());
		return Vector256.Dot(Diff, Diff);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double Distance(Vector4D B) => WL.Math.Distance4D(X, Y, Z, W, B.X, B.Y, B.Z, B.W);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double DistanceSquared(Vector4D A, Vector4D B) => A.DistanceSquared(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Distance(Vector4D A, Vector4D B) => A.Distance(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double Dot(Vector4D B) => Vector256.Dot(this.ToSIMD(), B.ToSIMD());
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Dot(Vector4D A, Vector4D B) => A.Dot(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Lerp(Vector4D B, double T){
		Vector256<double> Result = Vector256.Add(this.ToSIMD(), Vector256.Multiply(Vector256.Subtract(B.ToSIMD(), this.ToSIMD()), Vector256.Create(T)));
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D LerpSafe(Vector4D B, double T) => Lerp(B, WL.Math.Clamp01D(T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D Lerp(Vector4D A, Vector4D B, double T) => A.Lerp(B, T);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D LerpSafe(Vector4D A, Vector4D B, double T) => A.LerpSafe(B, T); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Min(Vector4D B){
		Vector256<double> Result = Vector256.Min(this.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Min(double B){
		Vector256<double> Result = Vector256.Min(this.ToSIMD(), Vector256.Create(B));
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D Min(Vector4D A, Vector4D B) => A.Min(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D Min(Vector4D A, double B) => A.Min(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Max(Vector4D B){
		Vector256<double> Result = Vector256.Max(this.ToSIMD(), B.ToSIMD());
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Max(double B){
		Vector256<double> Result = Vector256.Max(this.ToSIMD(), Vector256.Create(B));
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D Max(Vector4D A, Vector4D B) => A.Max(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D Max(Vector4D A, double B) => A.Max(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Clamp(Vector4D Min, Vector4D Max) => this.Min(Max).Max(Min);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Clamp(double Min, double Max) => this.Min(Max).Max(Min);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D Clamp(Vector4D A, Vector4D Min, Vector4D Max) => A.Clamp(Min, Max);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4D Clamp(Vector4D A, double Min, double Max) => A.Clamp(Min, Max); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Floor(){
		Vector256<double> Result = Vector256.Floor(this.ToSIMD());
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Round() => new Vector4D(WL.Math.RoundD(X), WL.Math.RoundD(Y), WL.Math.RoundD(Z), WL.Math.RoundD(W));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Ceil(){
		Vector256<double> Result = Vector256.Ceiling(this.ToSIMD());
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4D Abs(){
		Vector256<double> Result = Vector256.Abs(this.ToSIMD());
		return Unsafe.As<Vector256<double>, Vector4D>(ref Result);
	} 

	// ----------------------------------------------------------------------

	public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
		["XYZW"] = $"{X}|{Y}|{Z}|{W}"}; 

	public void __Unpack(Dictionary<string, object?> Data){
		string XYZW = WL.Packer.Get<string>(Data, "XYZW", "0|0|0|0")!; 

		string[] Parts = XYZW.Split('|');
		if (Parts.Length >= 4){
			double.TryParse(Parts[0], out X);
			double.TryParse(Parts[1], out Y);
			double.TryParse(Parts[2], out Z);
			double.TryParse(Parts[3], out W);
		}
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector4D Other) => X == Other.X && Y == Other.Y && Z == Other.Z && W == Other.W;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object? Object) => Object is Vector4D Other && Equals(Other); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector4D Left, Vector4D Right) => Left.Equals(Right);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector4D Left, Vector4D Right) => !(Left == Right); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToShortString() => $"{X}, {Y}, {Z}, {W}";
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString() => $"Vector4D({ToShortString()})"; 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
}