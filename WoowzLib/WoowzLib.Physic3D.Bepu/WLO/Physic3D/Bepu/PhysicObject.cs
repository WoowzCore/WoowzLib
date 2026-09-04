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

        RigidPose Pose = Position.HasValue ? new RigidPose(Position.Value, __LastRotation) : __GetPose();
        __RemoveFromPhysic();

        if(__Colliders.Count == 0){ return; }

        CollidableDescription ColliderDescription;
        
        if(__Colliders.Count == 1 && __Colliders[0].Offset == Vector3.Zero && __Colliders[0].Rotation == Quaternion.Identity){
            __ColliderIndex = __Colliders[0].Collider.__AddToPhysic(Owner.World.Shapes, Owner.Pool, Size);
            ColliderDescription = new CollidableDescription(__ColliderIndex, 0.1f);
        }else{
            Owner.Pool.Take<CompoundChild>(__Colliders.Count, out Buffer<CompoundChild> Children);
            for(int i = 0; i < __Colliders.Count; i++){
                Children[i] = new CompoundChild{
                    ShapeIndex = __Colliders[i].Collider.__AddToPhysic(Owner.World.Shapes, Owner.Pool, Size),
                    LocalPose = new RigidPose(__Colliders[i].Offset * __Size, __Colliders[i].Rotation)
                };
            }
            __ColliderIndex = Owner.World.Shapes.Add(new Compound(Children));
            ColliderDescription = new CollidableDescription(__ColliderIndex, 0.1f);
        }

        BodyActivityDescription Activity = new BodyActivityDescription(AlwaysAwake ? -1 : 0.01f);
        
        if(Type == PhysicType.Static){
            __SHandle = Owner.World.Statics.Add(new StaticDescription(Pose, __ColliderIndex));
        }else{
            BodyInertia Inertia = Type == PhysicType.Dynamic ? __CalculateTotalInertia() : new BodyInertia();
            BodyDescription Description = BodyDescription.CreateDynamic(Pose, Inertia, ColliderDescription, Activity);
            __BHandle = Owner.World.Bodies.Add(Description);
            __UpdateActivity();
        }
        Owner.RegisterObject(this);
    }

    public BodyInertia __CalculateTotalInertia(){
        if(__Colliders.Count == 0){ return default; }

        float SafeMass = Mass <= 0 ? 0.001f : Mass;
        Vector3F SafeSize = new Vector3F(
            MathF.Max(0.001f, Size.X),
            MathF.Max(0.001f, Size.Y),
            MathF.Max(0.001f, Size.Z)
        );

        if(__Colliders.Count == 1){ return __Colliders[0].Collider.__ComputeInertia(SafeMass, SafeSize); }

        // todo, нейронка говорит складывать инерции с учётом смещения по теореме Штейнера
        return __Colliders[0].Collider.__ComputeInertia(SafeMass, SafeSize);
    }
    
    public void __UpdateInertia(){
        if(!CanBody){ return; }
        BodyReference Body = __GetBody();
        Body.LocalInertia = (Type == PhysicType.Kinematic) ? new BodyInertia() : __CalculateTotalInertia();
        __UpdateActivity();
    }

    public void __UpdateActivity(){
        if(!CanBody){ return; }
        BodyReference Body = __GetBody();
        BodyActivity Activity = Body.Activity;
        Activity.SleepThreshold = AlwaysAwake ? -1 : 0.01f;
        if(AlwaysAwake){ Awake(); }
    }

    public BodyReference   __GetBody  () => Owner.World.Bodies [__BHandle!.Value];
    public StaticReference __GetStatic() => Owner.World.Statics[__SHandle!.Value];

    public RigidPose __GetPose(){
        if(IsBody  ){ return __GetBody  ().Pose; }
        if(IsStatic){ return __GetStatic().Pose; }
        return new RigidPose(__LastPosition, __LastRotation);
    }
    
    public void __SetPose(RigidPose Pose){
        if(Active){
            if(IsBody  ){ BodyReference   Body   = __GetBody  (); Body  .Pose = Pose; }
            if(IsStatic){ StaticReference Static = __GetStatic(); Static.Pose = Pose; }
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
        if(CanBody){ __GetBody().ApplyImpulse(Impulse, WorldOffset); }
    }
    
    public void ImpulseLinear(Vector3F Impulse){
        if(CanBody){ __GetBody().ApplyLinearImpulse(Impulse); }
    }
    
    public void ImpulseAngular(Vector3F Impulse){
        if(CanBody){ __GetBody().ApplyAngularImpulse(Impulse); }
    }

    public Vector3F GetVelocityAtPoint(Vector3F WorldPoint = default){
        if(!CanBody){ return Vector3F.Zero; }
        BodyReference Body = __GetBody();
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

    public bool IsAwake => CanBody && __GetBody().Awake;
    public bool IsSleep => !IsAwake;
    
    public void Awake(){ if(CanBody){ BodyReference Body = __GetBody(); Body.Awake = true; } }
    public void Sleep(){ if(CanBody){ BodyReference Body = __GetBody(); Body.Awake = false; } }
    
    private Vector3 __Size = Vector3.One;
    public Vector3F Size{
        get => __Size;
        set{
            Vector3 Value = value;
            if(__Size == Value){ return; } __Size = Value;
            __Rebuild();
        }
    }
    
    public Vector3F Position{
        get => __GetPose().Position;
        set{
            RigidPose Pose = __GetPose();
            Pose.Position = value;
            __SetPose(Pose);
        }
    }
    
    // todo, rotation quaternion
    public Quaternion QuaternionWIP{
        get => __GetPose().Orientation;
        set{
            RigidPose Pose = __GetPose();
            Pose.Orientation = value;
            __SetPose(Pose);
        }
    }

    public Vector3F VelocityLinear{
        get => CanBody ? __GetBody().Velocity.Linear : Vector3F.Zero;
        set{
            if(CanBody){ BodyReference Body = __GetBody(); Body.Velocity.Linear = value; }
        }
    }
    
    public Vector3F VelocityAngular{
        get => CanBody ? __GetBody().Velocity.Angular : Vector3F.Zero;
        set{
            if(CanBody){ BodyReference Body = __GetBody(); Body.Velocity.Angular = value; }
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
                RigidPose Pose = __GetPose();
                __LastPosition = Pose.Position;
                __LastRotation = Pose.Orientation;
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

        public PhysicMaterial(float Friction = 1, float Frequency = 30, float Damping = 1){
            this.Friction = Friction;
            this.Frequency = Frequency;
            this.Damping = Damping;
        }

        public static PhysicMaterial Default => new PhysicMaterial(1, 30, 1);
    }
}