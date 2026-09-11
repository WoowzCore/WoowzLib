namespace WLO.Math;

public class Vector2F : IEquatable<Vector2F>, WLI.Packable{
	public float X;
	public float Y;

	public float W { get => X; set => X = value; }
	public float H { get => Y; set => Y = value; }

	public Vector2F(float X, float Y){
		this.X = X;
		this.Y = Y;
	}
	public Vector2F(float XY) : this(XY, XY){}
}