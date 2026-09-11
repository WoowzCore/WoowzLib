namespace WLO.Math;

public class Vector2D : IEquatable<Vector2D>, WLI.Packable{
	public double X;
	public double Y;

	public double W { get => X; set => X = value; }
	public double H { get => Y; set => Y = value; }

	public Vector2D(double X, double Y){
		this.X = X;
		this.Y = Y;
	}
	public Vector2D(double XY) : this(XY, XY){}
}