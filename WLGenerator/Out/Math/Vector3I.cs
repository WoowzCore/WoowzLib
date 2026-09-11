namespace WLO.Math;

public class Vector3I : IEquatable<Vector3I>, WLI.Packable{
	public int X;
	public int Y;
	public int Z;

	public int W { get => X; set => X = value; }
	public int H { get => Y; set => Y = value; }
	public int D { get => Z; set => Z = value; }

	public Vector3I(int X, int Y, int Z){
		this.X = X;
		this.Y = Y;
		this.Z = Z;
	}
	public Vector3I(int XYZ) : this(XYZ, XYZ, XYZ){}
}