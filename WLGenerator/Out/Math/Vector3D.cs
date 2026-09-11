namespace WLO.Math;

public class Vector3D : IEquatable<Vector3D>, WLI.Packable{
	public double X;
	public double Y;
	public double Z;

	public double W { get => X; set => X = value; }
	public double H { get => Y; set => Y = value; }
	public double D { get => Z; set => Z = value; }

	public Vector3D(double X, double Y, double Z){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
	}
	public Vector3D(double XYZ) : this(XYZ, XYZ, XYZ){}
}