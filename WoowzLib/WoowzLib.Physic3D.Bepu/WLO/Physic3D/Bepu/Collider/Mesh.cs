using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using WLO.Math;

namespace WLO.Physic3D.Bepu.Collider;

// TODO, SCALE

public struct Mesh : WLI.Physic3D.Bepu.Collider.Collider{
    public Vector3F[] Vertices;
    public int[]      Indices;
    public Vector3F   Scale;
    
    public Mesh(Vector3F[] Vertices, int[] Indices, Vector3F Scale){
        this.Vertices = Vertices;
        this.Indices = Indices;
        this.Scale = Scale;
    }

    public TypedIndex __AddToPhysic(Shapes Shapes, BufferPool Pool, Vector3F Scale) => Shapes.Add(__ToBepu(Vertices, Indices, Scale, Pool));
    public BodyInertia __ComputeInertia(float Mass, Vector3F Scale) => Box.__ToBepu(Scale).ComputeInertia(Mass);

    public static BepuPhysics.Collidables.Mesh __ToBepu(Vector3F[] Vertices, int[] Indices, Vector3F Scale, BufferPool Pool){
        Pool.Take<Triangle>(Indices.Length / 3, out Buffer<Triangle> Triangles);
        for(int i = 0; i < Indices.Length; i += 3){
            Triangles[i / 3] = new Triangle(Vertices[Indices[i]], Vertices[Indices[i + 1]], Vertices[Indices[i + 2]]);
        }
        
        return new BepuPhysics.Collidables.Mesh(Triangles, Scale, Pool);
    }
}