using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using WLO.Math;

namespace WLO.Physic3D.Bepu.Collider;

// TODO, SCALE

public struct Sphere : WLI.Physic3D.Bepu.Collider.Collider{
    public float Diameter;

    public float Radius{
        get => Diameter * 0.5f;
        set => Diameter = value * 2;
    }

    public Sphere(float Diameter) => this.Diameter = Diameter;

    public TypedIndex __AddToPhysic(Shapes Shapes, BufferPool Pool, Vector3F Scale) => Shapes.Add(__ToBepu(Radius));
    public BodyInertia __ComputeInertia(float Mass, Vector3F Scale) => __ToBepu(Radius).ComputeInertia(Mass);

    public static BepuPhysics.Collidables.Sphere __ToBepu(float Radius) => new BepuPhysics.Collidables.Sphere(Radius);
}