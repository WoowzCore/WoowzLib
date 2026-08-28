using WLO;
using WLO.Math;

namespace WL;

// TODO: TTT OOO DD  OOO
// TODO:  T  O O D D O O
// TODO:  T  OOO DD  OOO

public static partial class Geometry{
    public static WLO.Geometry Create(IEnumerable<Vertex> Vertices, IEnumerable<uint> Indices) => new WLO.Geometry(Vertices, Indices);

    public static WLO.Geometry CreateTriangle(float Size = 0.5f) => new WLO.Geometry([
        new Vertex(new Vector3F(-Size, -Size, 0), new Vector2F(0   , 0), new Vector3F(0, 0, 1), default, 0),
        new Vertex(new Vector3F( Size, -Size, 0), new Vector2F(1   , 0), new Vector3F(0, 0, 1), default, 1),
        new Vertex(new Vector3F( 0   ,  Size, 0), new Vector2F(0.5f, 1), new Vector3F(0, 0, 1), default, 2)
    ], [0, 1, 2]);

    public static WLO.Geometry CreateQuad(float Size = 0.5f) => new WLO.Geometry([
        new Vertex(new Vector3F(-Size, -Size, 0), new Vector2F(0, 0), new Vector3F(0, 0, 1), default, 0),
        new Vertex(new Vector3F( Size, -Size, 0), new Vector2F(1, 0), new Vector3F(0, 0, 1), default, 1),
        new Vertex(new Vector3F( Size,  Size, 0), new Vector2F(1, 1), new Vector3F(0, 0, 1), default, 2),
        new Vertex(new Vector3F(-Size , Size, 0), new Vector2F(0, 1), new Vector3F(0, 0, 1), default, 3)
    ], [0, 1, 2, 2, 3, 0]);

    public static WLO.Geometry CreateCube(float Size = 0.5f){
        WLO.Geometry Result = new WLO.Geometry();

        uint IDCounter = 0;
        
        void AddFace(Vector3F P1, Vector3F P2, Vector3F P3, Vector3F P4, Vector3F Normal){
            uint Offset = (uint)Result.Vertices.Count;
            Result.Vertices.Add(new Vertex(P1, new Vector2F(0, 0), Normal, default, IDCounter++));
            Result.Vertices.Add(new Vertex(P2, new Vector2F(1, 0), Normal, default, IDCounter++));
            Result.Vertices.Add(new Vertex(P3, new Vector2F(1, 1), Normal, default, IDCounter++));
            Result.Vertices.Add(new Vertex(P4, new Vector2F(0, 1), Normal, default, IDCounter++));
            Result.Indices.AddRange([Offset, Offset + 1, Offset + 2, Offset + 2, Offset + 3, Offset]);
        }

        AddFace(new Vector3F(-Size, -Size,  Size), new Vector3F( Size, -Size,  Size), new Vector3F( Size,  Size,  Size), new Vector3F(-Size,  Size,  Size), new Vector3F( 0,  0,  1));
        AddFace(new Vector3F( Size, -Size, -Size), new Vector3F(-Size, -Size, -Size), new Vector3F(-Size,  Size, -Size), new Vector3F( Size,  Size, -Size), new Vector3F( 0,  0, -1));
        
        AddFace(new Vector3F(-Size, -Size, -Size), new Vector3F(-Size, -Size,  Size), new Vector3F(-Size,  Size,  Size), new Vector3F(-Size,  Size, -Size), new Vector3F(-1,  0,  0));
        AddFace(new Vector3F( Size, -Size,  Size), new Vector3F( Size, -Size, -Size), new Vector3F( Size,  Size, -Size), new Vector3F( Size,  Size,  Size), new Vector3F( 1,  0,  0));
        
        AddFace(new Vector3F(-Size,  Size,  Size), new Vector3F( Size,  Size,  Size), new Vector3F( Size,  Size, -Size), new Vector3F(-Size,  Size, -Size), new Vector3F( 0,  1,  0));
        AddFace(new Vector3F(-Size, -Size, -Size), new Vector3F( Size, -Size, -Size), new Vector3F( Size, -Size,  Size), new Vector3F(-Size, -Size,  Size), new Vector3F( 0, -1,  0));
        
        return Result;
    }
    
    // todo, create sphere
    
    // ----------------------------------------------------------------------
    
    // объединяет геометрию
    public static WLO.Geometry Union(params WLO.Geometry[] Geometries){
        WLO.Geometry Result = new WLO.Geometry();
        uint VertexOffset = 0;
        uint IDCounter = 0;

        foreach(WLO.Geometry Geometry in Geometries){
            foreach(Vertex Vertex in Geometry.Vertices){
                Vertex NewVertex = Vertex;
                NewVertex.ID = IDCounter++;
                Result.Vertices.Add(NewVertex);
            }

            foreach(uint Index in Geometry.Indices){
                Result.Indices.Add(Index + VertexOffset);
            }

            VertexOffset += (uint)Geometry.Vertices.Count;
        }
        
        return Result;
    }

    // применяет матрицу
    public static void ApplyTransform(WLO.Geometry Geometry, Matrix4F Matrix){
        for(int i = 0; i < Geometry.Vertices.Count; i++){
            Vertex Vertex = Geometry.Vertices[i];
            Vertex.Position = Matrix * Vertex.Position;
            Vertex.Normal = Matrix.TransformNormal(Vertex.Normal).Normalized;
            Geometry.Vertices[i] = Vertex;
        }
    }

    // обновляет id у вертиксов
    public static void GenerateSequentialID(WLO.Geometry Geometry){
        for(int i = 0; i < Geometry.Vertices.Count; i++){
            Vertex Vertex = Geometry.Vertices[i];
            Vertex.ID = (uint)i;
            Geometry.Vertices[i] = Vertex;
        }
    }

    public static void ToCenter(WLO.Geometry Geometry){
        Bounds Bounds = GetBounds(Geometry);
        ApplyTransform(Geometry, Matrix4F.CreateTranslation(-Bounds.Center));
    }
    
    // ----------------------------------------------------------------------

    public static Bounds GetBounds(WLO.Geometry Geometry){
        if(Geometry.Vertices.Count == 0){ return new Bounds(); }

        Vector3F Min = new Vector3F(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3F Max = new Vector3F(float.MinValue, float.MinValue, float.MinValue);

        foreach(Vertex Vertex in Geometry.Vertices){
            Vector3F P = Vertex.Position;
            if(P.X < Min.X){ Min.X = P.X; }
            if(P.Y < Min.Y){ Min.Y = P.Y; }
            if(P.Z < Min.Z){ Min.Z = P.Z; }
            
            if(P.X > Max.X){ Max.X = P.X; }
            if(P.Y > Max.Y){ Max.Y = P.Y; }
            if(P.Z > Max.Z){ Max.Z = P.Z; }
        }
        
        return new Bounds(Min, Max);
    }
}