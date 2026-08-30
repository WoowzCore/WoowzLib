using WLO;
using WLO.Math;

namespace WL;

public struct Geometry{
    // todo, creator's сделать тоже не зависящими от Vertex, Geometry, что-бы можно было генерить по отдельности ТО ЧТО НАДО ИМЕННО ТЕБЕ, Я ТЕБЕ ГОВОРЮ, ИМЕННО ТЕБЕ!
    
    public static WLO.Geometry Create(IEnumerable<Vertex> Vertices, IEnumerable<uint>? Indices = null) => new WLO.Geometry(Vertices, Indices);

    public static WLO.Geometry CreateTriangle(float Size = 0.5f, Color4B Color = default) => new WLO.Geometry([
        new Vertex(new Vector3F(-Size, -Size, 0), new Vector2F(0   , 0), new Vector3F(0, 0, 1), Color, 0),
        new Vertex(new Vector3F( Size, -Size, 0), new Vector2F(1   , 0), new Vector3F(0, 0, 1), Color, 1),
        new Vertex(new Vector3F( 0   ,  Size, 0), new Vector2F(0.5f, 1), new Vector3F(0, 0, 1), Color, 2)
    ], [0, 1, 2]);

    public static WLO.Geometry CreateQuad(float Size = 0.5f, Color4B Color = default) => new WLO.Geometry([
        new Vertex(new Vector3F(-Size, -Size, 0), new Vector2F(0, 0), new Vector3F(0, 0, 1), Color, 0),
        new Vertex(new Vector3F( Size, -Size, 0), new Vector2F(1, 0), new Vector3F(0, 0, 1), Color, 1),
        new Vertex(new Vector3F( Size,  Size, 0), new Vector2F(1, 1), new Vector3F(0, 0, 1), Color, 2),
        new Vertex(new Vector3F(-Size , Size, 0), new Vector2F(0, 1), new Vector3F(0, 0, 1), Color, 3)
    ], [0, 1, 2, 2, 3, 0]);

    public static WLO.Geometry CreateCube(float Size = 0.5f, Color4B Color = default){
        WLO.Geometry Result = new WLO.Geometry();

        uint IDCounter = 0;
        
        void AddFace(Vector3F P1, Vector3F P2, Vector3F P3, Vector3F P4, Vector3F Normal){
            uint Offset = (uint)Result.Vertices.Count;
            Result.Vertices.Add(new Vertex(P1, new Vector2F(0, 0), Normal, Color, IDCounter++));
            Result.Vertices.Add(new Vertex(P2, new Vector2F(1, 0), Normal, Color, IDCounter++));
            Result.Vertices.Add(new Vertex(P3, new Vector2F(1, 1), Normal, Color, IDCounter++));
            Result.Vertices.Add(new Vertex(P4, new Vector2F(0, 1), Normal, Color, IDCounter++));
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
    // Получение информации
    
    /// Получает объём вершин
    public static Bounds3D GetBounds3D(Span<Vector3F> Positions){
        if(Positions.Length == 0){ return new Bounds3D(); }

        Vector3F Min = Vector3F.MinValue;
        Vector3F Max = Vector3F.MaxValue;

        foreach(Vector3F Position in Positions){ WL.Math.Expand3D(ref Min, ref Max, Position); }
        
        return new Bounds3D(Min, Max);
    }
    /// <inheritdoc cref="GetBounds3D(Span{Vector3F})"/>
    public static Bounds3D GetBounds3D(Span<Vertex> Vertices){
        if(Vertices.IsEmpty){ return new Bounds3D(); }

        Vector3F Min = Vector3F.MinValue;
        Vector3F Max = Vector3F.MaxValue;

        foreach(Vertex Vertex in Vertices){ WL.Math.Expand3D(ref Min, ref Max, Vertex.Position); }
        
        return new Bounds3D(Min, Max);
    }
    /// <inheritdoc cref="GetBounds3D(Span{Vector3F})"/>
    public static Bounds3D GetBounds3D(WLO.Geometry Geometry) => GetBounds3D(Geometry.VerticesSpan);
    
    
    
    // ----------------------------------------------------------------------
    // Изменение информации

    /// Добавляет (копирует) исходную геометрию в конец целевых списков (автоматически корректирует индексы) (<paramref name="UpdateID"/> можно смело ставить <c>true</c>, оно работает быстрее чем <see cref="GenerateSequentialID(Span{Vertex},uint)"/>)
    public static void Add(List<Vertex> TargetVertices, List<uint> TargetIndices, ReadOnlySpan<Vertex> SourceVertices, ReadOnlySpan<uint> SourceIndices, bool UpdateID = true){
        if(SourceVertices.IsEmpty){ return; }

        TargetVertices.EnsureCapacity(TargetVertices.Capacity + SourceVertices.Length);
        TargetIndices .EnsureCapacity(TargetIndices .Capacity + SourceIndices .Length);
        
        uint VertexOffset = (uint)TargetVertices.Count;

        for(int i = 0; i < SourceVertices.Length; i++){
            Vertex Vertex = SourceVertices[i];

            if(UpdateID){ Vertex.ID = VertexOffset + (uint)i; }
            
            TargetVertices.Add(Vertex);
        }

        foreach(uint i in SourceIndices){
            TargetIndices.Add(i + VertexOffset);
        }
    }
    /// <inheritdoc cref="Add(List{Vertex},List{uint},ReadOnlySpan{Vertex},ReadOnlySpan{uint},bool)"/>
    public static void Add(WLO.Geometry Target, WLO.Geometry Source, bool UpdateID = true) => Add(Target.Vertices, Target.Indices, Source.VerticesSpan, Source.IndicesSpan, UpdateID);
    
    
    
    /// Объединяет геометрии в одну геометрию
    public static WLO.Geometry Union(WLO.Geometry[] Geometries, bool UpdateID = true, bool EnsureCapacity = true){
        WLO.Geometry Result = new WLO.Geometry();
        if(Geometries.Length == 0){ return Result; }

        if(EnsureCapacity){
            int TotalV = 0;
            int TotalI = 0;
            foreach(WLO.Geometry Geometry in Geometries){
                TotalV += Geometry.Vertices.Count;
                TotalI += Geometry.Indices .Count;
            }

            Result.Vertices.EnsureCapacity(TotalV);
            Result.Indices .EnsureCapacity(TotalI);
        }

        foreach(WLO.Geometry Geometry in Geometries){ Add(Result, Geometry, UpdateID); }

        return Result;
    }
    /// <inheritdoc cref="Union(WLO.Geometry[],bool,bool)"/>
    public static WLO.Geometry Union(bool UpdateID, params WLO.Geometry[] Geometries) => Union(Geometries, UpdateID);
    /// <inheritdoc cref="Union(WLO.Geometry[],bool,bool)"/>
    public static WLO.Geometry Union(params WLO.Geometry[] Geometries) => Union(Geometries, true);
    
    
    
    /// Применяет трансформацию вершинам
    public static void ApplyTransform(Span<Vertex> Vertices, Matrix4F Matrix, bool UpdateNormals = true, bool UpdatePositions = true){
        if(!UpdatePositions && !UpdateNormals){ return; }

        for(int i = 0; i < Vertices.Length; i++){
            ref Vertex Vertex = ref Vertices[i];
            if(UpdatePositions){ Vertex.Position = Matrix * Vertex.Position;                         }
            if(UpdateNormals  ){ Vertex.Normal   = Matrix.TransformNormal(Vertex.Normal).Normalized; }
        }
    }
    /// <inheritdoc cref="ApplyTransform(Span{Vertex},Matrix4F,bool,bool)"/>
    public static void ApplyTransform(WLO.Geometry Geometry, Matrix4F Matrix, bool UpdateNormals = true, bool UpdatePositions = true) => ApplyTransform(Geometry.VerticesSpan, Matrix, UpdateNormals, UpdatePositions);


    
    /// Двигает вершины на указанное расстояние
    public static void Move(Span<Vertex> Vertices, Vector3F Offset){
        if(Offset == Vector3F.Zero){ return; }
        for(int i = 0; i < Vertices.Length; i++){ Vertices[i].Position += Offset; }
    }
    /// <inheritdoc cref="Move(Span{Vertex},Vector3F)"/>
    public static void Move(WLO.Geometry Geometry, Vector3F Offset) => Move(Geometry.VerticesSpan, Offset);
    /// <inheritdoc cref="Move(Span{Vertex},Vector3F)"/>
    public static void Move(Span<Vertex> Vertices, Vector2F Offset) => Move(Vertices, Offset.To3F());
    /// <inheritdoc cref="Move(Span{Vertex},Vector3F)"/>
    public static void Move(WLO.Geometry Geometry, Vector2F Offset) => Move(Geometry.VerticesSpan, Offset.To3F());
    
    
    
    /// Делает ID вершин уникальными, использует метод <see cref="Math.GenerateSequential(Span{uint},uint)"/>>
    public static void GenerateSequentialID(Span<Vertex> Vertices, uint Start = 0){
        for(int i = 0; i < Vertices.Length; i++){ Vertices[i].ID = Start + (uint)i; }
    }
    /// <inheritdoc cref="GenerateSequentialID(Span{Vertex},uint)"/>
    public static void GenerateSequentialID(WLO.Geometry Geometry, uint Start = 0) => GenerateSequentialID(Geometry.VerticesSpan, Start);

    
    
    /// Перемещает вершины так, что-бы визуально они все были в указанной позиции
    public static void ToPosition(Span<Vertex> Vertices, Vector3F Position, Bounds3D? Bounds = null){
        Bounds3D Bounds__ = Bounds ?? GetBounds3D(Vertices);
        Move(Vertices, -Bounds__.Center + Position);
    }
    /// <inheritdoc cref="ToPosition(Span{Vertex},Vector3F,Bounds3D?)"/>
    public static void ToPosition(WLO.Geometry Geometry, Vector3F Position, Bounds3D? Bounds = null) => ToPosition(Geometry.VerticesSpan, Position, Bounds);
    
    
    
    /// Перемещает вершины так, что-бы визуально они все были в центре
    public static void ToCenter(Span<Vertex> Vertices, Bounds3D? Bounds = null) => ToPosition(Vertices, Vector3F.Zero, Bounds);
    /// <inheritdoc cref="ToCenter(Span{Vertex},Bounds3D?)"/>
    public static void ToCenter(WLO.Geometry Geometry, Bounds3D? Bounds = null) => ToCenter(Geometry.VerticesSpan, Bounds);
}