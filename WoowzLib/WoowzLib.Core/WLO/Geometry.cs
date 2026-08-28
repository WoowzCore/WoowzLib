namespace WLO;

public class Geometry{
    public readonly List<Vertex> Vertices = [];
    public readonly List<uint>   Indices  = [];

    public Geometry(){}
    public Geometry(IEnumerable<Vertex> Vertices, IEnumerable<uint>? Indices = null){
        this.Vertices.AddRange(Vertices);
        if(Indices != null){ this.Indices.AddRange(Indices); }
    }
}