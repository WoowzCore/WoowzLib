using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics; 
 
 /* 
	Класс Vector4I сгенерирован с помощью WLGenerator
	Сгенерирован: 2026.09.12 00:22:18
 */ 

namespace WLO.Math; 

public struct Vector4I : IEquatable<Vector4I>, WLI.Packable{
	public int X;
	public int Y;
	public int Z;
	public int W; 
 

	public int R { get => X; set => X = value; }
	public int G { get => Y; set => Y = value; }
	public int B { get => Z; set => Z = value; }
	public int A { get => W; set => W = value; } 
 

	public Vector2I XY => new Vector2I(X, Y);
	public Vector2I YZ => new Vector2I(Y, Z);
	public Vector2I ZW => new Vector2I(Z, W);
	public Vector2I WX => new Vector2I(W, X);
	public Vector3I XYZ => new Vector3I(X, Y, Z);
	public Vector3I YZW => new Vector3I(Y, Z, W);
	public Vector3I ZWX => new Vector3I(Z, W, X);
	public Vector3I WXY => new Vector3I(W, X, Y);
	public Vector4I YZWX => new Vector4I(Y, Z, W, X);
	public Vector4I ZWXY => new Vector4I(Z, W, X, Y);
	public Vector4I WXYZ => new Vector4I(W, X, Y, Z);
	public Vector2I XX => new Vector2I(X, X);
	public Vector3I XXX => new Vector3I(X, X, X);
	public Vector4I XXXX => new Vector4I(X, X, X, X);
	public Vector2I YY => new Vector2I(Y, Y);
	public Vector3I YYY => new Vector3I(Y, Y, Y);
	public Vector4I YYYY => new Vector4I(Y, Y, Y, Y);
	public Vector2I ZZ => new Vector2I(Z, Z);
	public Vector3I ZZZ => new Vector3I(Z, Z, Z);
	public Vector4I ZZZZ => new Vector4I(Z, Z, Z, Z);
	public Vector2I WW => new Vector2I(W, W);
	public Vector3I WWW => new Vector3I(W, W, W);
	public Vector4I WWWW => new Vector4I(W, W, W, W);
	public Vector2I RG => new Vector2I(R, G);
	public Vector2I GB => new Vector2I(G, B);
	public Vector2I BA => new Vector2I(B, A);
	public Vector2I AR => new Vector2I(A, R);
	public Vector3I RGB => new Vector3I(R, G, B);
	public Vector3I GBA => new Vector3I(G, B, A);
	public Vector3I BAR => new Vector3I(B, A, R);
	public Vector3I ARG => new Vector3I(A, R, G);
	public Vector4I GBAR => new Vector4I(G, B, A, R);
	public Vector4I BARG => new Vector4I(B, A, R, G);
	public Vector4I ARGB => new Vector4I(A, R, G, B);
	public Vector2I RR => new Vector2I(R, R);
	public Vector3I RRR => new Vector3I(R, R, R);
	public Vector4I RRRR => new Vector4I(R, R, R, R);
	public Vector2I GG => new Vector2I(G, G);
	public Vector3I GGG => new Vector3I(G, G, G);
	public Vector4I GGGG => new Vector4I(G, G, G, G);
	public Vector2I BB => new Vector2I(B, B);
	public Vector3I BBB => new Vector3I(B, B, B);
	public Vector4I BBBB => new Vector4I(B, B, B, B);
	public Vector2I AA => new Vector2I(A, A);
	public Vector3I AAA => new Vector3I(A, A, A);
	public Vector4I AAAA => new Vector4I(A, A, A, A); 
 

	public Vector4I(int X, int Y, int Z, int W){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
		this.W = W;
	}
	public Vector4I(int XYZW) : this(XYZW, XYZW, XYZW, XYZW){}
	public Vector4I(Vector2I VectorA, Vector2I VectorB) : this(VectorA.X, VectorA.Y, VectorB.X, VectorB.Y){}
	public Vector4I(Vector3I Vector, int W) : this(Vector.X, Vector.Y, Vector.Z, W){}
	public Vector4I(int X, int Y, int Z) : this(X, Y, Z, 1){} /* <- Типо цвет */ 
 

	public Vector2I To2I() => new Vector2I(X, Y);
	public Vector3I To3I() => new Vector3I(X, Y, Z); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector128<int> ToSIMD() => Vector128.Create(X, Y, Z, W); 
 

	public static Vector4I Zero => new Vector4I(0, 0, 0, 0);
	public static Vector4I One => new Vector4I(1, 1, 1, 1);
	public static Vector4I MOne => new Vector4I(-1, -1, -1, -1);
	public static Vector4I Right => new Vector4I(1, 0, 0, 0);
	public static Vector4I Left => new Vector4I(-1, 0, 0, 0);
	public static Vector4I AxisX => new Vector4I(1, 0, 0, 0);
	public static Vector4I AxisMX => new Vector4I(-1, 0, 0, 0);
	public static Vector4I Up => new Vector4I(0, 1, 0, 0);
	public static Vector4I Down => new Vector4I(0, -1, 0, 0);
	public static Vector4I AxisY => new Vector4I(0, 1, 0, 0);
	public static Vector4I AxisMY => new Vector4I(0, -1, 0, 0);
	public static Vector4I Front => new Vector4I(0, 0, 1, 0);
	public static Vector4I Back => new Vector4I(0, 0, -1, 0);
	public static Vector4I FrontGL => new Vector4I(0, 0, -1, 0);
	public static Vector4I AxisZ => new Vector4I(0, 0, 1, 0);
	public static Vector4I AxisMZ => new Vector4I(0, 0, -1, 0);
	public static Vector4I Ana => new Vector4I(0, 0, 0, 1);
	public static Vector4I Kata => new Vector4I(0, 0, 0, -1);
	public static Vector4I AxisW => new Vector4I(0, 0, 0, 1);
	public static Vector4I AxisMW => new Vector4I(0, 0, 0, -1);
	public static Vector4I MaxValue => new Vector4I(WL.Math.MaxValueI, WL.Math.MaxValueI, WL.Math.MaxValueI, WL.Math.MaxValueI);
	public static Vector4I MinValue => new Vector4I(WL.Math.MinValueI, WL.Math.MinValueI, WL.Math.MinValueI, WL.Math.MinValueI); 

	// ----------------------------------------------------------------------

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I Add(Vector4I B){{
			this = this + B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I Add(int B){{
			this = this + B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I operator +(Vector4I A, Vector4I B){
		Vector128<int> Result = Vector128.Add(A.ToSIMD(), B.ToSIMD());
		return new Vector4I(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I operator +(Vector4I A, int B){
		Vector128<int> Result = Vector128.Add(A.ToSIMD(), Vector128.Create(B));
		return new Vector4I(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I Sub(Vector4I B){{
			this = this - B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I Sub(int B){{
			this = this - B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I operator -(Vector4I A, Vector4I B){
		Vector128<int> Result = Vector128.Subtract(A.ToSIMD(), B.ToSIMD());
		return new Vector4I(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I operator -(Vector4I A, int B){
		Vector128<int> Result = Vector128.Subtract(A.ToSIMD(), Vector128.Create(B));
		return new Vector4I(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I Mul(Vector4I B){{
			this = this * B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I Mul(int B){{
			this = this * B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I operator *(Vector4I A, Vector4I B){
		Vector128<int> Result = Vector128.Multiply(A.ToSIMD(), B.ToSIMD());
		return new Vector4I(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I operator *(Vector4I A, int B){
		Vector128<int> Result = Vector128.Multiply(A.ToSIMD(), Vector128.Create(B));
		return new Vector4I(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I Div(Vector4I B){{
			this = this / B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I Div(int B){{
			this = this / B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I operator /(Vector4I A, Vector4I B){
		Vector128<int> Result = Vector128.Divide(A.ToSIMD(), B.ToSIMD());
		return new Vector4I(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I operator /(Vector4I A, int B){
		Vector128<int> Result = Vector128.Divide(A.ToSIMD(), Vector128.Create(B));
		return new Vector4I(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
	} 
 

	public int this[int Index]{
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
	public void Deconstruct(out int X, out int Y){
		X = this.X;
		Y = this.Y;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out int X, out int Y, out int Z){
		X = this.X;
		Y = this.Y;
		Z = this.Z;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out int X, out int Y, out int Z, out int W){
		X = this.X;
		Y = this.Y;
		Z = this.Z;
		W = this.W;
	} 

	// ----------------------------------------------------------------------

	public float LengthSquared {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.LengthSquared4I(X, Y, Z, W); }
	public float Length {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.Length4I(X, Y, Z, W); } 
 
 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float DistanceSquared(Vector4I B) => WL.Math.DistanceSquared4I(X, Y, Z, W, B.X, B.Y, B.Z, B.W);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Distance(Vector4I B) => WL.Math.Distance4I(X, Y, Z, W, B.X, B.Y, B.Z, B.W);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquared(Vector4I A, Vector4I B) => A.DistanceSquared(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(Vector4I A, Vector4I B) => A.Distance(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I Lerp(Vector4I B, float T) => new Vector4I(WL.Math.LerpI(X, B.X, T), WL.Math.LerpI(Y, B.Y, T), WL.Math.LerpI(Z, B.Z, T), WL.Math.LerpI(W, B.W, T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector4I LerpSafe(Vector4I B, float T) => new Vector4I(WL.Math.LerpSafeI(X, B.X, T), WL.Math.LerpSafeI(Y, B.Y, T), WL.Math.LerpSafeI(Z, B.Z, T), WL.Math.LerpSafeI(W, B.W, T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I Lerp(Vector4I A, Vector4I B, float T) => A.Lerp(B, T);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4I LerpSafe(Vector4I A, Vector4I B, float T) => A.LerpSafe(B, T); 

	// ----------------------------------------------------------------------

	public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
		["XYZW"] = $"{X}|{Y}|{Z}|{W}"}; 

	public void __Unpack(Dictionary<string, object?> Data){
		string XYZW = WL.Packer.Get<string>(Data, "XYZW", "0|0|0|0")!; 

		string[] Parts = XYZW.Split('|');
		if (Parts.Length >= 4){
			int.TryParse(Parts[0], out X);
			int.TryParse(Parts[1], out Y);
			int.TryParse(Parts[2], out Z);
			int.TryParse(Parts[3], out W);
		}
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector4I Other) => X == Other.X && Y == Other.Y && Z == Other.Z && W == Other.W;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object? Object) => Object is Vector4I Other && Equals(Other); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector4I Left, Vector4I Right) => Left.Equals(Right);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector4I Left, Vector4I Right) => !(Left == Right); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToShortString() => $"{X}, {Y}, {Z}, {W}";
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString() => $"Vector4I({ToShortString()})"; 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
}