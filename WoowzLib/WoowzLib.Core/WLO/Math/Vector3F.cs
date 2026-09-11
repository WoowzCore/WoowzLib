using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics; 
 
 /* 
	Класс Vector3F сгенерирован с помощью WLGenerator
	Сгенерирован: 2026.09.12 00:22:18
 */ 

namespace WLO.Math; 

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
 

	public Vector2F XY => new Vector2F(X, Y);
	public Vector2F YZ => new Vector2F(Y, Z);
	public Vector2F ZX => new Vector2F(Z, X);
	public Vector3F YZX => new Vector3F(Y, Z, X);
	public Vector3F ZXY => new Vector3F(Z, X, Y);
	public Vector2F XX => new Vector2F(X, X);
	public Vector3F XXX => new Vector3F(X, X, X);
	public Vector2F YY => new Vector2F(Y, Y);
	public Vector3F YYY => new Vector3F(Y, Y, Y);
	public Vector2F ZZ => new Vector2F(Z, Z);
	public Vector3F ZZZ => new Vector3F(Z, Z, Z);
	public Vector2F RG => new Vector2F(R, G);
	public Vector2F GB => new Vector2F(G, B);
	public Vector2F BR => new Vector2F(B, R);
	public Vector3F GBR => new Vector3F(G, B, R);
	public Vector3F BRG => new Vector3F(B, R, G);
	public Vector2F RR => new Vector2F(R, R);
	public Vector3F RRR => new Vector3F(R, R, R);
	public Vector2F GG => new Vector2F(G, G);
	public Vector3F GGG => new Vector3F(G, G, G);
	public Vector2F BB => new Vector2F(B, B);
	public Vector3F BBB => new Vector3F(B, B, B); 
 

	public Vector3F(float X, float Y, float Z){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
	}
	public Vector3F(float XYZ) : this(XYZ, XYZ, XYZ){}
	public Vector3F(Vector2F Vector, float Z) : this(Vector.X, Vector.Y, Z){} 
 

	public Vector2F To2F() => new Vector2F(X, Y);
	public Vector4F To4F() => new Vector4F(X, Y, Z, 0); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector128<float> ToSIMD() => Vector128.Create(X, Y, Z, 0); 
 

	public static Vector3F Zero => new Vector3F(0, 0, 0);
	public static Vector3F One => new Vector3F(1, 1, 1);
	public static Vector3F MOne => new Vector3F(-1, -1, -1);
	public static Vector3F Half => new Vector3F(0.5f, 0.5f, 0.5f);
	public static Vector3F MHalf => new Vector3F(-0.5f, -0.5f, -0.5f);
	public static Vector3F Right => new Vector3F(1, 0, 0);
	public static Vector3F Left => new Vector3F(-1, 0, 0);
	public static Vector3F AxisX => new Vector3F(1, 0, 0);
	public static Vector3F AxisMX => new Vector3F(-1, 0, 0);
	public static Vector3F Up => new Vector3F(0, 1, 0);
	public static Vector3F Down => new Vector3F(0, -1, 0);
	public static Vector3F AxisY => new Vector3F(0, 1, 0);
	public static Vector3F AxisMY => new Vector3F(0, -1, 0);
	public static Vector3F Front => new Vector3F(0, 0, 1);
	public static Vector3F Back => new Vector3F(0, 0, -1);
	public static Vector3F FrontGL => new Vector3F(0, 0, -1);
	public static Vector3F AxisZ => new Vector3F(0, 0, 1);
	public static Vector3F AxisMZ => new Vector3F(0, 0, -1);
	public static Vector3F MaxValue => new Vector3F(WL.Math.MaxValueF, WL.Math.MaxValueF, WL.Math.MaxValueF);
	public static Vector3F MinValue => new Vector3F(WL.Math.MinValueF, WL.Math.MinValueF, WL.Math.MinValueF);
	public static Vector3F NAN => new Vector3F(WL.Math.NANF, WL.Math.NANF, WL.Math.NANF); 

	// ----------------------------------------------------------------------

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Add(Vector3F B){{
			this = this + B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Add(float B){{
			this = this + B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator +(Vector3F A, Vector3F B) => new Vector3F(A.X + B.X, A.Y + B.Y, A.Z + B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator +(Vector3F A, float B) => new Vector3F(A.X + B, A.Y + B, A.Z + B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Sub(Vector3F B){{
			this = this - B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Sub(float B){{
			this = this - B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator -(Vector3F A, Vector3F B) => new Vector3F(A.X - B.X, A.Y - B.Y, A.Z - B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator -(Vector3F A, float B) => new Vector3F(A.X - B, A.Y - B, A.Z - B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Mul(Vector3F B){{
			this = this * B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Mul(float B){{
			this = this * B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator *(Vector3F A, Vector3F B) => new Vector3F(A.X * B.X, A.Y * B.Y, A.Z * B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F operator *(Vector3F A, float B) => new Vector3F(A.X * B, A.Y * B, A.Z * B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Div(Vector3F B){{
			this = this / B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Div(float B){{
			this = this / B;
			return this;
		}
	}
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
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Normalize(){{
			this = Normalized;
			return this;
		}
	}
	public Vector3F Normalized{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get{
			float L = Length;
			return L > WL.Math.EpsilonF ? this / L : Vector3F.Zero;
		}
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float DistanceSquared(Vector3F B) => WL.Math.DistanceSquared3F(X, Y, Z, B.X, B.Y, B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Distance(Vector3F B) => WL.Math.Distance3F(X, Y, Z, B.X, B.Y, B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquared(Vector3F A, Vector3F B) => A.DistanceSquared(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(Vector3F A, Vector3F B) => A.Distance(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F Lerp(Vector3F B, float T) => new Vector3F(WL.Math.LerpF(X, B.X, T), WL.Math.LerpF(Y, B.Y, T), WL.Math.LerpF(Z, B.Z, T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3F LerpSafe(Vector3F B, float T) => new Vector3F(WL.Math.LerpSafeF(X, B.X, T), WL.Math.LerpSafeF(Y, B.Y, T), WL.Math.LerpSafeF(Z, B.Z, T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F Lerp(Vector3F A, Vector3F B, float T) => A.Lerp(B, T);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3F LerpSafe(Vector3F A, Vector3F B, float T) => A.LerpSafe(B, T); 

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