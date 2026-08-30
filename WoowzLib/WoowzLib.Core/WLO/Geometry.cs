using System.Runtime.InteropServices;

namespace WLO;

public readonly struct Geometry{
    public readonly List<Vertex> Vertices;
    public readonly List<uint  > Indices ;

    public Span<Vertex> VerticesSpan => CollectionsMarshal.AsSpan(Vertices);
    public Span<uint  > IndicesSpan  => CollectionsMarshal.AsSpan(Indices );
    
    public Geometry(){
        Vertices = [];
        Indices  = [];
    }
    public Geometry(IEnumerable<Vertex> Vertices, IEnumerable<uint>? Indices = null){
        this.Vertices = [];
        this.Indices  = [];
        
        this.Vertices.AddRange(Vertices);
        if(Indices != null){ this.Indices.AddRange(Indices); }
    }
}