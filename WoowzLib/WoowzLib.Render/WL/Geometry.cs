using WLO;
using WLO.Math;

namespace WL;

// TODO: TTT OOO DD  OOO
// TODO:  T  O O D D O O
// TODO:  T  OOO DD  OOO

public static class Geometry{
    public static GeometryData Create(IEnumerable<Vertex> Vertices, IEnumerable<uint> Indices) => new GeometryData(Vertices, Indices);

    public static GeometryData CreateTriangle(float Size = 0.5f) => new GeometryData([
        new Vertex(new Vector3F(-Size, -Size, 0), new Vector2F(0   , 0), new Vector3F(0, 0, 1), default, 0),
        new Vertex(new Vector3F( Size, -Size, 0), new Vector2F(1   , 0), new Vector3F(0, 0, 1), default, 1),
        new Vertex(new Vector3F( 0   ,  Size, 0), new Vector2F(0.5f, 1), new Vector3F(0, 0, 1), default, 2)
    ], [0, 1, 2]);

    public static GeometryData CreateQuad(float Size = 0.5f) => new GeometryData([
        new Vertex(new Vector3F(-Size, -Size, 0), new Vector2F(0, 0), new Vector3F(0, 0, 1), default, 0),
        new Vertex(new Vector3F( Size, -Size, 0), new Vector2F(1, 0), new Vector3F(0, 0, 1), default, 1),
        new Vertex(new Vector3F( Size,  Size, 0), new Vector2F(1, 1), new Vector3F(0, 0, 1), default, 2),
        new Vertex(new Vector3F(-Size , Size, 0), new Vector2F(0, 1), new Vector3F(0, 0, 1), default, 3)
    ], [0, 1, 2, 2, 3, 0]);

    public static GeometryData CreateCube(float Size = 0.5f){
        GeometryData Result = new GeometryData();

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
    
    // объеденяет геометрию
    public static GeometryData Union(params GeometryData[] Geometries){
        GeometryData Result = new GeometryData();
        uint VertexOffset = 0;
        uint IDCounter = 0;

        foreach(GeometryData Geometry in Geometries){
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
    public static void ApplyTransform(GeometryData Geometry, Matrix4F Matrix){
        for(int i = 0; i < Geometry.Vertices.Count; i++){
            Vertex Vertex = Geometry.Vertices[i];
            Vertex.Position = Matrix * Vertex.Position;
            Vertex.Normal = Matrix.TransformNormal(Vertex.Normal).Normalized;
            Geometry.Vertices[i] = Vertex;
        }
    }

    // обновляет id у вертиксов
    public static void GenerateSequentialID(GeometryData Geometry){
        for(int i = 0; i < Geometry.Vertices.Count; i++){
            Vertex Vertex = Geometry.Vertices[i];
            Vertex.ID = (uint)i;
            Geometry.Vertices[i] = Vertex;
        }
    }
}