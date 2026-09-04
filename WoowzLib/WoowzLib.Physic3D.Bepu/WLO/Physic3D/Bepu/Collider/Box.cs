using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using WLO.Math;

namespace WLO.Physic3D.Bepu.Collider;

public struct Box : WLI.Physic3D.Bepu.Collider.Collider{
    public Vector3F Size;

    public Box(Vector3F Size) => this.Size = Size;

    public TypedIndex __AddToPhysic(Shapes Shapes, BufferPool Pool, Vector3F Scale) => Shapes.Add(__ToBepu(Size * Scale));
    public BodyInertia __ComputeInertia(float Mass, Vector3F Scale) => __ToBepu(Size * Scale).ComputeInertia(Mass);

    public static BepuPhysics.Collidables.Box __ToBepu(Vector3F Size) => new BepuPhysics.Collidables.Box(Size.W, Size.H, Size.D);
}