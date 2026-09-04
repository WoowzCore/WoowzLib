using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using WLO.Math;

namespace WLI.Physic3D.Bepu.Collider;

public interface Collider{
    TypedIndex __AddToPhysic(Shapes Shapes, BufferPool Pool, Vector3F Scale);
    BodyInertia __ComputeInertia(float Mass, Vector3F Scale);
}