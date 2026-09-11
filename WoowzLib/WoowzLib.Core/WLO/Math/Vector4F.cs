using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics; 
 
 /* 
	Класс Vector4F сгенерирован с помощью WLGenerator
	Сгенерирован: 2026.09.12 00:22:18
 */ 

namespace WLO.Math; 

public struct Vector4F : IEquatable<Vector4F>, WLI.Packable{
	public float X;
	public float Y;
	public float Z;
	public float W; 
 

	public float R { get => X; set => X = value; }
	public float G { get => Y; set => Y = value; }
	public float B { get => Z; set => Z = value; }
	public float A { get => W; set => W = value; } 
 

	public Vector2F XY => new Vector2F(X, Y);
	public Vector2F YZ => new Vector2F(Y, Z);
	public Vector2F ZW => new Vector2F(Z, W);
	public Vector2F WX => new Vector2F(W, X);
	public Vector3F XYZ => new Vector3F(X, Y, Z);
	public Vector3F YZW => new Vector3F(Y, Z, W);
	public Vector3F ZWX => new Vector3F(Z, W, X);
	public Vector3F WXY => new Vector3F(W, X, Y);
	public Vector4F YZWX => new Vector4F(Y, Z, W, X);
	public Vector4F ZWXY => new Vector4F(Z, W, X, Y);
	public Vector4F WXYZ => new Vector4F(W, X, Y, Z);
	public Vector2F XX => new Vector2F(X, X);
	public Vector3F XXX => new Vector3F(X, X, X);
	public Vector4F XXXX => new Vector4F(X, X, X, X);
	public Vector2F YY => new Vector2F(Y, Y);
	public Vector3F YYY => new Vector3F(Y, Y, Y);
	public Vector4F YYYY => new Vector4F(Y, Y, Y, Y);
	public Vector2F ZZ => new Vector2F(Z, Z);
	public Vector3F ZZZ => new Vector3F(Z, Z, Z);
	public Vector4F ZZZZ => new Vector4F(Z, Z, Z, Z);
	public Vector2F WW => new Vector2F(W, W);
	public Vector3F WWW => new Vector3F(W, W, W);
	public Vector4F WWWW => new Vector4F(W, W, W, W);
	public Vector2F RG => new Vector2F(R, G);
	public Vector2F GB => new Vector2F(G, B);
	public Vector2F BA => new Vector2F(B, A);
	public Vector2F AR => new Vector2F(A, R);
	public Vector3F RGB => new Vector3F(R, G, B);
	public Vector3F GBA => new Vector3F(G, B, A);
	public Vector3F BAR => new Vector3F(B, A, R);
	public Vector3F ARG => new Vector3F(A, R, G);
	public Vector4F GBAR => new Vector4F(G, B, A, R);
	public Vector4F BARG => new Vector4F(B, A, R, G);
	public Vector4F ARGB => new Vector4F(A, R, G, B);
	public Vector2F RR => new Vector2F(R, R);
	public Vector3F RRR => new Vector3F(R, R, R);
	public Vector4F RRRR => new Vector4F(R, R, R, R);
	public Vector2F GG => new Vector2F(G, G);
	public Vector3F GGG => new Vector3F(G, G, G);
	public Vector4F GGGG => new Vector4F(G, G, G, G);
	public Vector2F BB => new Vector2F(B, B);
	public Vector3F BBB => new Vector3F(B, B, B);
	public Vector4F BBBB => new Vector4F(B, B, B, B);
	public Vector2F AA => new Vector2F(A, A);
	public Vector3F AAA => new Vector3F(A, A, A);
	public Vector4F AAAA => new Vector4F(A, A, A, A); 
 

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
 

	public Vector2F To2F() => new Vector2F(X, Y);
	public Vector3F To3F() => new Vector3F(X, Y, Z); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector128<float> ToSIMD() => Vector128.Create(X, Y, Z, W); 
 

	public static Vector4F Zero => new Vector4F(0, 0, 0, 0);
	public static Vector4F One => new Vector4F(1, 1, 1, 1);
	public static Vector4F MOne => new Vector4F(-1, -1, -1, -1);
	public static Vector4F Half => new Vector4F(0.5f, 0.5f, 0.5f, 0.5f);
	public static Vector4F MHalf => new Vector4F(-0.5f, -0.5f, -0.5f, -0.5f);
	public static Vector4F Right => new Vector4F(1, 0, 0, 0);
	public static Vector4F Left => new Vector4F(-1, 0, 0, 0);
	public static Vector4F AxisX => new Vector4F(1, 0, 0, 0);
	public static Vector4F AxisMX => new Vector4F(-1, 0, 0, 0);
	public static Vector4F Up => new Vector4F(0, 1, 0, 0);
	public static Vector4F Down => new Vector4F(0, -1, 0, 0);
	public static Vector4F AxisY => new Vector4F(0, 1, 0, 0);
	public static Vector4F AxisMY => new Vector4F(0, -1, 0, 0);
	public static Vector4F Front => new Vector4F(0, 0, 1, 0);
	public static Vector4F Back => new Vector4F(0, 0, -1, 0);
	public static Vector4F FrontGL => new Vector4F(0, 0, -1, 0);
	public static Vector4F AxisZ => new Vector4F(0, 0, 1, 0);
	public static Vector4F AxisMZ => new Vector4F(0, 0, -1, 0);
	public static Vector4F Ana => new Vector4F(0, 0, 0, 1);
	public static Vector4F Kata => new Vector4F(0, 0, 0, -1);
	public static Vector4F AxisW => new Vector4F(0, 0, 0, 1);
	public static Vector4F AxisMW => new Vector4F(0, 0, 0, -1);
	public static Vector4F MaxValue => new Vector4F(WL.Math.MaxValueF, WL.Math.MaxValueF, WL.Math.MaxValueF, WL.Math.MaxValueF);
	public static Vector4F MinValue => new Vector4F(WL.Math.MinValueF, WL.Math.MinValueF, WL.Math.MinValueF, WL.Math.MinValueF);
	public static Vector4F NAN => new Vector4F(WL.Math.NANF, WL.Math.NANF, WL.Math.NANF, WL.Math.NANF); 

	// ----------------------------------------------------------------------

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Add(Vector4F B){{
			this = this + B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Add(float B){{
			this = this + B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator +(Vector4F A, Vector4F B){
		Vector128<float> Result = Vector128.Add(A.ToSIMD(), B.ToSIMD());
		return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator +(Vector4F A, float B){
		Vector128<float> Result = Vector128.Add(A.ToSIMD(), Vector128.Create(B));
		return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Sub(Vector4F B){{
			this = this - B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Sub(float B){{
			this = this - B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator -(Vector4F A, Vector4F B){
		Vector128<float> Result = Vector128.Subtract(A.ToSIMD(), B.ToSIMD());
		return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator -(Vector4F A, float B){
		Vector128<float> Result = Vector128.Subtract(A.ToSIMD(), Vector128.Create(B));
		return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Mul(Vector4F B){{
			this = this * B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Mul(float B){{
			this = this * B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator *(Vector4F A, Vector4F B){
		Vector128<float> Result = Vector128.Multiply(A.ToSIMD(), B.ToSIMD());
		return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator *(Vector4F A, float B){
		Vector128<float> Result = Vector128.Multiply(A.ToSIMD(), Vector128.Create(B));
		return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Div(Vector4F B){{
			this = this / B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Div(float B){{
			this = this / B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator /(Vector4F A, Vector4F B){
		Vector128<float> Result = Vector128.Divide(A.ToSIMD(), B.ToSIMD());
		return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F operator /(Vector4F A, float B){
		Vector128<float> Result = Vector128.Divide(A.ToSIMD(), Vector128.Create(B));
		return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
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
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Normalize(){{
			this = Normalized;
			return this;
		}
	}
	public Vector4F Normalized{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get{
			float L = Length;
			return L > WL.Math.EpsilonF ? this / L : Vector4F.Zero;
		}
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float DistanceSquared(Vector4F B) => WL.Math.DistanceSquared4F(X, Y, Z, W, B.X, B.Y, B.Z, B.W);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Distance(Vector4F B) => WL.Math.Distance4F(X, Y, Z, W, B.X, B.Y, B.Z, B.W);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquared(Vector4F A, Vector4F B) => A.DistanceSquared(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(Vector4F A, Vector4F B) => A.Distance(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F Lerp(Vector4F B, float T) => new Vector4F(WL.Math.LerpF(X, B.X, T), WL.Math.LerpF(Y, B.Y, T), WL.Math.LerpF(Z, B.Z, T), WL.Math.LerpF(W, B.W, T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4F LerpSafe(Vector4F B, float T) => new Vector4F(WL.Math.LerpSafeF(X, B.X, T), WL.Math.LerpSafeF(Y, B.Y, T), WL.Math.LerpSafeF(Z, B.Z, T), WL.Math.LerpSafeF(W, B.W, T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F Lerp(Vector4F A, Vector4F B, float T) => A.Lerp(B, T);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4F LerpSafe(Vector4F A, Vector4F B, float T) => A.LerpSafe(B, T); 

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