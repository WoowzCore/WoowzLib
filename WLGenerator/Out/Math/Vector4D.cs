namespace WLO.Math;

public class Vector4D : IEquatable<Vector4D>, WLI.Packable{
	public double X;
	public double Y;
	public double Z;
	public double W;

	public Vector4D(double X, double Y, double Z, double W){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
		this.W = W;
	}
	public Vector4D(double XYZW) : this(XYZW, XYZW, XYZW, XYZW){}
}