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
    
    // ----------------------------------------------------------------------
    
    private struct ColliderEntry{
        public WLI.Physic3D.Bepu.Collider.Collider Source;
        public TypedIndex                          Index;
        public BodyInertia                         Inertia;
        public Vector3                             Offset;
        public Quaternion                          Rotation;
    }
    
    private TypedIndex __CompoundIndex;
    
    private readonly List<ColliderEntry> __Colliders = [];

    public void AddCollider(WLI.Physic3D.Bepu.Collider.Collider Collider, Vector3F Offset = default, QuaternionF Rotation = default){
        TypedIndex Index = Collider.__AddToPhysic(Owner.World.Shapes, Owner.Pool, Scale);
        BodyInertia Inertia = Collider.__ComputeInertia(Mass, Scale);
    
        __Colliders.Add(new ColliderEntry{
            Source = Collider,
            Index = Index,
            Inertia = Inertia,
            Offset = Offset,
            Rotation = Rotation == default ? Quaternion.Identity : Rotation
        });
        __Rebuild();
    }

    public void RemoveCollider(WLI.Physic3D.Bepu.Collider.Collider Collider){
        int Index = __Colliders.FindIndex(C => C.Source == Collider);
        if(Index != -1){
            Owner.World.Shapes.Remove(__Colliders[Index].Index);
            __Colliders.RemoveAt(Index);
            __Rebuild();
        }
    }

    /*[Obsolete]
    public void ClearColliders(){
        __Colliders.Clear();
        __Rebuild();
    }*/
    
    // ----------------------------------------------------------------------

    public void __Rebuild(Vector3F? Position = null){
        if(!Active){ return; }

        RigidPose Pose = Position.HasValue ? new RigidPose(Position.Value, __LastRotation) : GetPose();
        __RemoveBodyAndStatic();

        if(__Colliders.Count == 0){ return; }

        TypedIndex FinalCollider;
    
        if(__Colliders.Count == 1 && __Colliders[0].Offset == Vector3.Zero && __Colliders[0].Rotation == Quaternion.Identity){
            FinalCollider = __Colliders[0].Index;
        }else{
            Owner.Pool.Take<CompoundChild>(__Colliders.Count, out Buffer<CompoundChild> Children);
        
            for(int i = 0; i < __Colliders.Count; i++){
                Children[i] = new CompoundChild{
                    ShapeIndex = __Colliders[i].Index,
                    LocalPose = new RigidPose(__Colliders[i].Offset * __Scale, __Colliders[i].Rotation)
                };
            }
            __CompoundIndex = Owner.World.Shapes.Add(new Compound(Children));
            FinalCollider = __CompoundIndex;
        
            Owner.Pool.Return(ref Children);
        }
    
        if(Type == PhysicType.Static){
            __SHandle = Owner.World.Statics.Add(new StaticDescription(Pose, FinalCollider));
        }else{
            BodyInertia Inertia = Type == PhysicType.Dynamic ? __CalculateTotalInertia() : new BodyInertia();

            BodyVelocity Velocity = new BodyVelocity(__LastVelocityLinear, __LastVelocityAngular);

            CollidableDescription Collidable = new CollidableDescription(FinalCollider, 0.2f);
            BodyActivityDescription Activity = new BodyActivityDescription(AlwaysAwake ? -1 : 0.01f);
        
            BodyDescription Description = BodyDescription.CreateDynamic(Pose, Velocity, Inertia, Collidable, Activity);
            __BHandle = Owner.World.Bodies.Add(Description);
            __UpdateActivity();
        }
        Owner.RegisterObject(this);
    }

    public BodyInertia __CalculateTotalInertia(){
        if(__Colliders.Count == 0){ return default; }

        BodyInertia Inertia = __Colliders[0].Source.__ComputeInertia(Mass, Scale);

        if(LockRotationPitch){ Inertia.InverseInertiaTensor.XX = 0; Inertia.InverseInertiaTensor.YX = 0; Inertia.InverseInertiaTensor.ZX = 0; }
        if(LockRotationYaw  ){ Inertia.InverseInertiaTensor.YY = 0; Inertia.InverseInertiaTensor.YX = 0; Inertia.InverseInertiaTensor.ZY = 0; }
        if(LockRotationRoll ){ Inertia.InverseInertiaTensor.ZZ = 0; Inertia.InverseInertiaTensor.ZX = 0; Inertia.InverseInertiaTensor.ZY = 0; }

        if(LockPositionX && LockPositionY && LockPositionZ){
            Inertia.InverseMass = 0;
        }
        
        // todo, нейронка говорит складывать инерции с учётом смещения по теореме Штейнера
        return Inertia;
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

    public void __ApplyPositionLocks(){
        if(!CanBody){ return; }
        if(!LockPositionX && !LockPositionY && !LockPositionZ){ return; }

        BodyReference Body = GetBody();
        Vector3 Velocity = Body.Velocity.Linear;
        if(LockPositionX){ Velocity.X = 0; }
        if(LockPositionY){ Velocity.Y = 0; }
        if(LockPositionZ){ Velocity.Z = 0; }
        Body.Velocity.Linear = Velocity;
    }

    public void __Update(){
        if(!CanBody){ return; }
        
        __ApplyPositionLocks();
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

    public void __RemoveBodyAndStatic(){
        Owner.UnregisterObject(this);
        
        if(__BHandle.HasValue){ Owner.World.Bodies .Remove(__BHandle.Value); __BHandle = null; }
        if(__SHandle.HasValue){ Owner.World.Statics.Remove(__SHandle.Value); __SHandle = null; }
        
        if(__CompoundIndex.Exists){
            Owner.World.Shapes.Remove(__CompoundIndex);
            __CompoundIndex = default;
        }
    }
    
    public void Dispose(){
        foreach(ColliderEntry Entry in __Colliders){
            Owner.World.Shapes.Remove(Entry.Index);
        }
    
        __Colliders.Clear();
        __RemoveBodyAndStatic();
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
            if(__Mass < 0.0001f){ __Mass = 0.0001f; }
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
            if(__Scale.X < 0.0001f){ __Scale.X = 0.0001f; }
            if(__Scale.Y < 0.0001f){ __Scale.Y = 0.0001f; }
            if(__Scale.Z < 0.0001f){ __Scale.Z = 0.0001f; }

            for(int i = 0; i < __Colliders.Count; i++){
                Owner.World.Shapes.Remove(__Colliders[i].Index);
                ColliderEntry Entry = __Colliders[i];
                Entry.Index = Entry.Source.__AddToPhysic(Owner.World.Shapes, Owner.Pool, __Scale);
                Entry.Inertia = Entry.Source.__ComputeInertia(Mass, __Scale);
                __Colliders[i] = Entry;
            }
            
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
                
                __RemoveBodyAndStatic();
            }
        }
    }

    private bool __LockPositionX;
    public bool LockPositionX{
        get => __LockPositionX;
        set{
            if(__LockPositionX == value){ return; } __LockPositionX = value;
            __UpdateInertia();
        }
    }
    
    private bool __LockPositionY;
    public bool LockPositionY{
        get => __LockPositionY;
        set{
            if(__LockPositionY == value){ return; } __LockPositionY = value;
            __UpdateInertia();
        }
    }
    
    private bool __LockPositionZ;
    public bool LockPositionZ{
        get => __LockPositionZ;
        set{
            if(__LockPositionZ == value){ return; } __LockPositionZ = value;
            __UpdateInertia();
        }
    }
    
    private bool __LockRotationPitch;
    public bool LockRotationPitch{
        get => __LockRotationPitch;
        set{
            if(__LockRotationPitch == value){ return; } __LockRotationPitch = value;
            __UpdateInertia();
        }
    }
    
    private bool __LockRotationYaw;
    public bool LockRotationYaw{
        get => __LockRotationYaw;
        set{
            if(__LockRotationYaw == value){ return; } __LockRotationYaw = value;
            __UpdateInertia();
        }
    }
    
    private bool __LockRotationRoll;
    public bool LockRotationRoll{
        get => __LockRotationRoll;
        set{
            if(__LockRotationRoll == value){ return; } __LockRotationRoll = value;
            __UpdateInertia();
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