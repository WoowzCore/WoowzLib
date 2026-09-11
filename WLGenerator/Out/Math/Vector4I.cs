namespace WLO.Math;

public class Vector4I : IEquatable<Vector4I>, WLI.Packable{
	public int X;
	public int Y;
	public int Z;
	public int W;

	public Vector4I(int X, int Y, int Z, int W){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
		this.W = W;
	}
	public Vector4I(int XYZW) : this(XYZW, XYZW, XYZW, XYZW){}
}