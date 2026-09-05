using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using WLO.Math;

namespace WLO.Physic3D.Bepu;

// todo, сохранять инерцию после возврата active?

public class PhysicObject : IDisposable{
    private readonly Bepu Owner;

    internal BodyHandle?   __BHandle;
    internal StaticHandle? __SHandle;

    public bool IsBody   => __BHandle.HasValue;
    public bool IsStatic => __SHandle.HasValue;

    public bool CanBody   => Active && IsBody;
    public bool CanStatic => Active && IsStatic;
    
    private Vector3    __LastPosition;
    private Quaternion __LastRotation = Quaternion.Identity;
    private Vector3    __LastVelocityLinear;
    private Vector3F   __LastVelocityAngular;
    
    public PhysicObject(Bepu Physic, Vector3F Position = default, PhysicType Type = PhysicType.Dynamic){
        Owner = Physic;
        __Type = Type;
        __LastPosition = Position;
    }

    public void Dispose() => __RemoveFromPhysic();
    
    // ----------------------------------------------------------------------
    
    private TypedIndex                                                                                __ColliderIndex;
    private List<(WLI.Physic3D.Bepu.Collider.Collider Collider, Vector3 Offset, Quaternion Rotation)> __Colliders = [];

    public void AddCollider(WLI.Physic3D.Bepu.Collider.Collider Collider, Vector3F Offset = default, Quaternion Rotation = default){
        __Colliders.Add((Collider, Offset, Rotation == default ? Quaternion.Identity : Rotation));
        __Rebuild();
    }

    public void RemoveCollider(WLI.Physic3D.Bepu.Collider.Collider Collider){
        __Colliders.RemoveAll(C => C.Collider == Collider);
        __Rebuild();
    }

    public void ClearColliders(){
        __Colliders.Clear();
        __Rebuild();
    }
    
    // ----------------------------------------------------------------------

    public void __Rebuild(Vector3F? Position = null){
        if(!Active){ return; }

        RigidPose Pose = Position.HasValue ? new RigidPose(Position.Value, __LastRotation) : GetPose();
        __RemoveFromPhysic();

        if(__Colliders.Count == 0){ return; }

        CollidableDescription ColliderDescription;
        
        if(__Colliders.Count == 1 && __Colliders[0].Offset == Vector3.Zero && __Colliders[0].Rotation == Quaternion.Identity){
            __ColliderIndex = __Colliders[0].Collider.__AddToPhysic(Owner.World.Shapes, Owner.Pool, Scale);
            ColliderDescription = new CollidableDescription(__ColliderIndex, 0.2f);
        }else{
            Owner.Pool.Take<CompoundChild>(__Colliders.Count, out Buffer<CompoundChild> Children);
            
            for(int i = 0; i < __Colliders.Count; i++){
                Children[i] = new CompoundChild{
                    ShapeIndex = __Colliders[i].Collider.__AddToPhysic(Owner.World.Shapes, Owner.Pool, Scale),
                    LocalPose = new RigidPose(__Colliders[i].Offset * __Scale, __Colliders[i].Rotation)
                };
            }
            __ColliderIndex = Owner.World.Shapes.Add(new Compound(Children));
            ColliderDescription = new CollidableDescription(__ColliderIndex, 0.2f);
            
            Owner.Pool.Return(ref Children);
        }

        BodyActivityDescription Activity = new BodyActivityDescription(AlwaysAwake ? -1 : 0.01f);
        
        if(Type == PhysicType.Static){
            __SHandle = Owner.World.Statics.Add(new StaticDescription(Pose, __ColliderIndex));
        }else{
            BodyInertia Inertia = Type == PhysicType.Dynamic ? __CalculateTotalInertia() : new BodyInertia();

            BodyVelocity Velocity = new BodyVelocity(__LastVelocityLinear, __LastVelocityAngular);
            
            BodyDescription Description = BodyDescription.CreateDynamic(Pose, Velocity, Inertia, ColliderDescription, Activity);
            __BHandle = Owner.World.Bodies.Add(Description);
            __UpdateActivity();
        }
        Owner.RegisterObject(this);
    }

    public BodyInertia __CalculateTotalInertia(){
        if(__Colliders.Count == 0){ return default; }

        if(__Colliders.Count == 1){ return __Colliders[0].Collider.__ComputeInertia(Mass, Scale); }

        // todo, нейронка говорит складывать инерции с учётом смещения по теореме Штейнера
        return __Colliders[0].Collider.__ComputeInertia(Mass, Scale);
    }
    
    public void __UpdateInertia(BodyReference Body){
        Body.LocalInertia = (Type == PhysicType.Kinematic) ? new BodyInertia() : __CalculateTotalInertia();
        __UpdateActivity(Body);
    }
    public void __UpdateInertia(){
        if(!CanBody){ return; }
        __UpdateInertia(GetBody());
    }

    public void __UpdateActivity(BodyReference Body){
        Body.Activity.SleepThreshold = AlwaysAwake ? -1 : 0.01f;
        if(AlwaysAwake){ Awake(Body); }
    }
    public void __UpdateActivity(){
        if(!CanBody){ return; }
        __UpdateActivity(GetBody());
    }

    public BodyReference   GetBody  () => Owner.World.Bodies [__BHandle!.Value];
    public StaticReference GetStatic() => Owner.World.Statics[__SHandle!.Value];

    public RigidPose GetPose(){
        if(IsBody  ){ return GetBody  ().Pose; }
        if(IsStatic){ return GetStatic().Pose; }
        return new RigidPose(__LastPosition, __LastRotation);
    }
    
    public void SetPose(RigidPose Pose){
        if(Active){
            if(IsBody  ){ GetBody  ().Pose = Pose; }
            if(IsStatic){ GetStatic().Pose = Pose; }
        }

        __LastPosition = Pose.Position;
        __LastRotation = Pose.Orientation;
    }

    public void __RemoveFromPhysic(){
        Owner.UnregisterObject(this);
        if(__ColliderIndex.Exists){
            Owner.World.Shapes.RecursivelyRemoveAndDispose(__ColliderIndex, Owner.Pool);
            __ColliderIndex = default;
        }
        if(__BHandle.HasValue){ Owner.World.Bodies .Remove(__BHandle.Value); __BHandle = null; }
        if(__SHandle.HasValue){ Owner.World.Statics.Remove(__SHandle.Value); __SHandle = null; }
    }
    
    // ----------------------------------------------------------------------

    public void Impulse(Vector3F Impulse, Vector3F WorldOffset = default){
        if(CanBody){ GetBody().ApplyImpulse(Impulse, WorldOffset); }
    }
    
    public void ImpulseLinear(Vector3F Impulse){
        if(CanBody){ GetBody().ApplyLinearImpulse(Impulse); }
    }
    
    public void ImpulseAngular(Vector3F Impulse){
        if(CanBody){ GetBody().ApplyAngularImpulse(Impulse); }
    }

    public Vector3F GetVelocityAtPoint(Vector3F WorldPoint = default){
        if(!CanBody){ return Vector3F.Zero; }
        BodyReference Body = GetBody();
        Vector3 Offset = (Vector3)WorldPoint - Body.Pose.Position;
        return Body.Velocity.Linear + Vector3.Cross(Body.Velocity.Angular, Offset);
    }
    
    private PhysicType __Type;
    public PhysicType Type{
        get => __Type;
        set{
            if(__Type == value){ return; } __Type = value;
            __Rebuild();
        }
    }

    private float __Mass = 1;
    public float Mass{
        get => __Mass;
        set{
            if(__Mass == value){ return; } __Mass = value;
            __UpdateInertia();
        }
    }

    private bool __AlwaysAwake = false;
    public bool AlwaysAwake{
        get => __AlwaysAwake;
        set{
            if(__AlwaysAwake == value){ return; } __AlwaysAwake = value;
            __UpdateActivity();
        }
    }

    public bool __IsAwake(BodyReference Body) => Body.Awake;
    public bool IsAwake => CanBody && __IsAwake(GetBody());
    public bool IsSleep => !IsAwake;
    
    public void Awake(BodyReference Body){ Body.Awake = true; }
    public void Awake(){ if(!CanBody){ return; } Awake(GetBody()); }
    
    public void Sleep(BodyReference Body){ Body.Awake = false; }
    public void Sleep(){ if(!CanBody){ return; } Sleep(GetBody()); }
    
    private Vector3 __Scale = Vector3.One;
    public Vector3F Scale{
        get => __Scale;
        set{
            Vector3 Value = value;
            if(__Scale == Value){ return; } __Scale = Value;
            __Rebuild();
        }
    }
    
    public Vector3F Position{
        get => GetPose().Position;
        set{
            RigidPose Pose = GetPose();
            Pose.Position = value;
            SetPose(Pose);
        }
    }
    
    public QuaternionF Rotation{
        get => GetPose().Orientation;
        set{
            RigidPose Pose = GetPose();
            Pose.Orientation = value;
            SetPose(Pose);
        }
    }

    public Vector3F VelocityLinear{
        get => CanBody ? GetBody().Velocity.Linear : Vector3F.Zero;
        set{
            if(CanBody){ GetBody().Velocity.Linear = value; }
        }
    }
    
    public Vector3F VelocityAngular{
        get => CanBody ? GetBody().Velocity.Angular : Vector3F.Zero;
        set{
            if(CanBody){ GetBody().Velocity.Angular = value; }
        }
    }

    private PhysicMaterial __Material = PhysicMaterial.Default;
    public PhysicMaterial Material{
        get => __Material;
        set => __Material = value;
    }

    private float __DampingLinear = 0.1f;
    public float DampingLinear{
        get => __DampingLinear;
        set{
            if(__DampingLinear == value){ return; } __DampingLinear = value;
            __UpdateInertia();
        }
    }
    
    private float __DampingAngular = 0.01f;
    public float DampingAngular{
        get => __DampingAngular;
        set{
            if(__DampingAngular == value){ return; } __DampingAngular = value;
            __UpdateInertia();
        }
    }

    private bool __Active = true;
    public bool Active{
        get => __Active;
        set{
            if(__Active == value){ return; } __Active = value;

            if(__Active){
                __Rebuild();
            }else{
                RigidPose Pose = GetPose();
                __LastPosition = Pose.Position;
                __LastRotation = Pose.Orientation;

                if(IsBody){
                    BodyReference Body = GetBody();
                    __LastVelocityLinear  = Body.Velocity.Linear;
                    __LastVelocityAngular = Body.Velocity.Angular;
                }else{
                    __LastVelocityLinear  = default;
                    __LastVelocityAngular = default;
                }
                
                __RemoveFromPhysic();
            }
        }
    }
    
    // ----------------------------------------------------------------------
    
    public enum PhysicType{ Dynamic, Kinematic, Static }
    
    public struct PhysicMaterial{
        public float Friction;
        public float Frequency;
        public float Damping;

        public PhysicMaterial(float Friction = 1, float Frequency = 90, float Damping = 2){
            this.Friction = Friction;
            this.Frequency = Frequency;
            this.Damping = Damping;
        }

        public static PhysicMaterial Default => new PhysicMaterial(1, 90, 2);
    }
}