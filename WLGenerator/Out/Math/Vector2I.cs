namespace WLO.Math;

public class Vector2I : IEquatable<Vector2I>, WLI.Packable{
	public int X;
	public int Y;

	public int W { get => X; set => X = value; }
	public int H { get => Y; set => Y = value; }

	public Vector2I(int X, int Y){
		this.X = X;
		this.Y = Y;
	}
	public Vector2I(int XY) : this(XY, XY){}
}