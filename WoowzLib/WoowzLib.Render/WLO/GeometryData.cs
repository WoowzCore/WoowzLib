namespace WLO;

public class GeometryData{
    public readonly List<Vertex> Vertices = [];
    public readonly List<uint>   Indices  = [];

    public GeometryData(){}
    public GeometryData(IEnumerable<Vertex> Vertices, IEnumerable<uint>? Indices = null){
        this.Vertices.AddRange(Vertices);
        if(Indices != null){ this.Indices.AddRange(Indices); }
    }
}