using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics; 
 
 /* 
	Класс Vector3I сгенерирован с помощью WLGenerator
	Сгенерирован: 2026.09.12 00:22:18
 */ 

namespace WLO.Math; 

public struct Vector3I : IEquatable<Vector3I>, WLI.Packable{
	public int X;
	public int Y;
	public int Z; 
 

	public int W { get => X; set => X = value; }
	public int H { get => Y; set => Y = value; }
	public int D { get => Z; set => Z = value; } 
 

	public int R { get => X; set => X = value; }
	public int G { get => Y; set => Y = value; }
	public int B { get => Z; set => Z = value; } 
 

	public Vector2I XY => new Vector2I(X, Y);
	public Vector2I YZ => new Vector2I(Y, Z);
	public Vector2I ZX => new Vector2I(Z, X);
	public Vector3I YZX => new Vector3I(Y, Z, X);
	public Vector3I ZXY => new Vector3I(Z, X, Y);
	public Vector2I XX => new Vector2I(X, X);
	public Vector3I XXX => new Vector3I(X, X, X);
	public Vector2I YY => new Vector2I(Y, Y);
	public Vector3I YYY => new Vector3I(Y, Y, Y);
	public Vector2I ZZ => new Vector2I(Z, Z);
	public Vector3I ZZZ => new Vector3I(Z, Z, Z);
	public Vector2I RG => new Vector2I(R, G);
	public Vector2I GB => new Vector2I(G, B);
	public Vector2I BR => new Vector2I(B, R);
	public Vector3I GBR => new Vector3I(G, B, R);
	public Vector3I BRG => new Vector3I(B, R, G);
	public Vector2I RR => new Vector2I(R, R);
	public Vector3I RRR => new Vector3I(R, R, R);
	public Vector2I GG => new Vector2I(G, G);
	public Vector3I GGG => new Vector3I(G, G, G);
	public Vector2I BB => new Vector2I(B, B);
	public Vector3I BBB => new Vector3I(B, B, B); 
 

	public Vector3I(int X, int Y, int Z){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
	}
	public Vector3I(int XYZ) : this(XYZ, XYZ, XYZ){}
	public Vector3I(Vector2I Vector, int Z) : this(Vector.X, Vector.Y, Z){} 
 

	public Vector2I To2I() => new Vector2I(X, Y);
	public Vector4I To4I() => new Vector4I(X, Y, Z, 0); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector128<int> ToSIMD() => Vector128.Create(X, Y, Z, 0); 
 

	public static Vector3I Zero => new Vector3I(0, 0, 0);
	public static Vector3I One => new Vector3I(1, 1, 1);
	public static Vector3I MOne => new Vector3I(-1, -1, -1);
	public static Vector3I Right => new Vector3I(1, 0, 0);
	public static Vector3I Left => new Vector3I(-1, 0, 0);
	public static Vector3I AxisX => new Vector3I(1, 0, 0);
	public static Vector3I AxisMX => new Vector3I(-1, 0, 0);
	public static Vector3I Up => new Vector3I(0, 1, 0);
	public static Vector3I Down => new Vector3I(0, -1, 0);
	public static Vector3I AxisY => new Vector3I(0, 1, 0);
	public static Vector3I AxisMY => new Vector3I(0, -1, 0);
	public static Vector3I Front => new Vector3I(0, 0, 1);
	public static Vector3I Back => new Vector3I(0, 0, -1);
	public static Vector3I FrontGL => new Vector3I(0, 0, -1);
	public static Vector3I AxisZ => new Vector3I(0, 0, 1);
	public static Vector3I AxisMZ => new Vector3I(0, 0, -1);
	public static Vector3I MaxValue => new Vector3I(WL.Math.MaxValueI, WL.Math.MaxValueI, WL.Math.MaxValueI);
	public static Vector3I MinValue => new Vector3I(WL.Math.MinValueI, WL.Math.MinValueI, WL.Math.MinValueI); 

	// ----------------------------------------------------------------------

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I Add(Vector3I B){{
			this = this + B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I Add(int B){{
			this = this + B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I operator +(Vector3I A, Vector3I B) => new Vector3I(A.X + B.X, A.Y + B.Y, A.Z + B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I operator +(Vector3I A, int B) => new Vector3I(A.X + B, A.Y + B, A.Z + B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I Sub(Vector3I B){{
			this = this - B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I Sub(int B){{
			this = this - B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I operator -(Vector3I A, Vector3I B) => new Vector3I(A.X - B.X, A.Y - B.Y, A.Z - B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I operator -(Vector3I A, int B) => new Vector3I(A.X - B, A.Y - B, A.Z - B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I Mul(Vector3I B){{
			this = this * B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I Mul(int B){{
			this = this * B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I operator *(Vector3I A, Vector3I B) => new Vector3I(A.X * B.X, A.Y * B.Y, A.Z * B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I operator *(Vector3I A, int B) => new Vector3I(A.X * B, A.Y * B, A.Z * B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I Div(Vector3I B){{
			this = this / B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I Div(int B){{
			this = this / B;
			return this;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I operator /(Vector3I A, Vector3I B) => new Vector3I(A.X / B.X, A.Y / B.Y, A.Z / B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I operator /(Vector3I A, int B) => new Vector3I(A.X / B, A.Y / B, A.Z / B); 
 

	public int this[int Index]{
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

	// ----------------------------------------------------------------------

	public float LengthSquared {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.LengthSquared3I(X, Y, Z); }
	public float Length {[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => WL.Math.Length3I(X, Y, Z); } 
 
 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float DistanceSquared(Vector3I B) => WL.Math.DistanceSquared3I(X, Y, Z, B.X, B.Y, B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Distance(Vector3I B) => WL.Math.Distance3I(X, Y, Z, B.X, B.Y, B.Z);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquared(Vector3I A, Vector3I B) => A.DistanceSquared(B);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(Vector3I A, Vector3I B) => A.Distance(B); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I Lerp(Vector3I B, float T) => new Vector3I(WL.Math.LerpI(X, B.X, T), WL.Math.LerpI(Y, B.Y, T), WL.Math.LerpI(Z, B.Z, T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3I LerpSafe(Vector3I B, float T) => new Vector3I(WL.Math.LerpSafeI(X, B.X, T), WL.Math.LerpSafeI(Y, B.Y, T), WL.Math.LerpSafeI(Z, B.Z, T));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I Lerp(Vector3I A, Vector3I B, float T) => A.Lerp(B, T);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3I LerpSafe(Vector3I A, Vector3I B, float T) => A.LerpSafe(B, T); 

	// ----------------------------------------------------------------------

	public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
		["XYZ"] = $"{X}|{Y}|{Z}"}; 

	public void __Unpack(Dictionary<string, object?> Data){
		string XYZ = WL.Packer.Get<string>(Data, "XYZ", "0|0|0")!; 

		string[] Parts = XYZ.Split('|');
		if (Parts.Length >= 3){
			int.TryParse(Parts[0], out X);
			int.TryParse(Parts[1], out Y);
			int.TryParse(Parts[2], out Z);
		}
	} 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector3I Other) => X == Other.X && Y == Other.Y && Z == Other.Z;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object? Object) => Object is Vector3I Other && Equals(Other); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector3I Left, Vector3I Right) => Left.Equals(Right);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector3I Left, Vector3I Right) => !(Left == Right); 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToShortString() => $"{X}, {Y}, {Z}";
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString() => $"Vector3I({ToShortString()})"; 
 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => HashCode.Combine(X, Y, Z);
}