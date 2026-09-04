using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using WLO.Math;

namespace WLO.Physic3D.Bepu;

/*
 * todo
 * я не знаю надо наверное тоже debug logger придумать, и в opengl тоже решить а то не удобно с ним взаимодействовать
 */

 public struct __PoseIntegratorCallbacks : IPoseIntegratorCallbacks{
        public readonly Bepu Owner;
        
        private Vector3Wide __GravityWide;

        public __PoseIntegratorCallbacks(Bepu Physic){
            Owner = Physic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void PrepareForIntegration(float DT) => __GravityWide = Vector3Wide.Broadcast(Owner.Gravity);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void IntegrateVelocity(Vector<int> BodyIndices, Vector3Wide Position, QuaternionWide Orientation, BodyInertiaWide LocalInertia, Vector<int> IntegrationMask, int WorkerIndex, Vector<float> DT, ref BodyVelocityWide Velocity){
            Velocity.Linear += __GravityWide * DT;
        }

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
        public bool AllowSubstepsForUnconstrainedBodies => false;
        public bool IntegrateVelocityForKinematics => false;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Initialize(Simulation Simulation){}
    }

    public struct __NarrowPhaseCallbacks : INarrowPhaseCallbacks{
        public readonly Bepu Owner;
        
        public __NarrowPhaseCallbacks(Bepu Physic){
            Owner = Physic;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Initialize(Simulation Simulation){}
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool AllowContactGeneration(int WorkerIndex, CollidableReference A, CollidableReference B, ref float SpeculativeMargin) => A.Mobility != CollidableMobility.Static || B.Mobility != CollidableMobility.Static;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool AllowContactGeneration(int WorkerIndex, CollidablePair Pair, int ChildIndexA, int ChildIndexB) => true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool ConfigureContactManifold<TManifold>(int WorkerIndex, CollidablePair Pair, ref TManifold Manifold, out PairMaterialProperties RairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>{
            WL.Logger.Debug($"[{WorkerIndex}] [{Pair}] [{Manifold}]");
            
            if (Manifold.Count > 0) {
                WL.Logger.Debug($"[COL] Hit detected! Points: {Manifold.Count} Objects: {Pair.A.BodyHandle} vs {Pair.B.StaticHandle}");
            }
            
            PhysicObject.PhysicMaterial MaterialA = Owner.__GetMaterial(Pair.A);
            PhysicObject.PhysicMaterial MaterialB = Owner.__GetMaterial(Pair.B);
            
            RairMaterial.FrictionCoefficient = MaterialA.Friction * MaterialB.Friction;
            RairMaterial.MaximumRecoveryVelocity = 2;
            
            RairMaterial.SpringSettings = new SpringSettings(
                MathF.Max(0.001f, MathF.Min(MaterialA.Frequency, MaterialB.Frequency)),
                MathF.Max(0.001f, MathF.Min(MaterialA.Damping, MaterialB.Damping))
            );
            
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int WorkerIndex, CollidablePair Pair, int ChildIndexA, int ChildIndexB, ref ConvexContactManifold Manifold) => true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Dispose(){}
    }

public class Bepu : IDisposable, WLI.Engine{
    public bool IsStarted{ get; private set; }

    public Bepu(bool StartImmediately = false){
        if(StartImmediately){ Start(); }
    }

    public void Start(){
        try{
            if(IsStarted){ throw new ExceptionWL("Физика Bepu уже и так была запущена!"); }

            Pool          = new BufferPool();
            __TDispatcher = new ThreadDispatcher(Environment.ProcessorCount);
            
            World = Simulation.Create<__NarrowPhaseCallbacks, __PoseIntegratorCallbacks>(
                Pool,
                new __NarrowPhaseCallbacks(this),
                new __PoseIntegratorCallbacks(this),
                new SolveDescription(8, 1)
            );
            
            IsStarted = true;
        }catch(Exception e){
            Exception realEx = e;
            while (realEx.InnerException != null) realEx = realEx.InnerException; // todo, что за говно с inner, надо изучить по лучше будет

            WL.Logger.Warn($"todo, [Physic3D] Error: {realEx.Message}");
            WL.Logger.Warn($"todo, [Physic3D] Stack: {realEx.StackTrace}");
            
            throw new ExceptionWL("todo, Произошла ошибка при запуске Bepu!", realEx);
        }
    }
    
    public bool Stop(){
        if(!IsStarted){ return false; }

        World.Dispose();
        __TDispatcher.Dispose();
        Pool.Clear();
        
        IsStarted = false;
        return true;
    }

    public void Dispose() => Stop();
    
    // ----------------------------------------------------------------------
    
    private ThreadDispatcher                          __TDispatcher = null!;
    private Solver                                    __Solver => World.Solver;
    private PoseIntegrator<__PoseIntegratorCallbacks> __PoseIntegrator => (PoseIntegrator<__PoseIntegratorCallbacks>)World.PoseIntegrator;

    public PhysicObject.PhysicMaterial __GetMaterial(CollidableReference Reference){
        PhysicObject? Object = null;
        if(Reference.Mobility == CollidableMobility.Static){
            __SHandleToObject.TryGetValue(Reference.StaticHandle.Value, out Object);
        }else{
            __BHandleToObject.TryGetValue(Reference.BodyHandle.Value, out Object);
        }

        return Object?.Material ?? PhysicObject.PhysicMaterial.Default;
    }
    
    // ----------------------------------------------------------------------

    private readonly Dictionary<int, PhysicObject> __BHandleToObject = [];
    private readonly Dictionary<int, PhysicObject> __SHandleToObject = [];

    internal void RegisterObject(PhysicObject PhysicObject){
        if(PhysicObject.__BHandle.HasValue){ __BHandleToObject[PhysicObject.__BHandle.Value.Value] = PhysicObject; }
        if(PhysicObject.__SHandle.HasValue){ __SHandleToObject[PhysicObject.__SHandle.Value.Value] = PhysicObject; }
    }

    internal void UnregisterObject(PhysicObject PhysicObject){
        if(PhysicObject.__BHandle.HasValue){ __BHandleToObject.Remove(PhysicObject.__BHandle.Value.Value); }
        if(PhysicObject.__SHandle.HasValue){ __SHandleToObject.Remove(PhysicObject.__SHandle.Value.Value); }
    }
    
    // ----------------------------------------------------------------------
    
    public Simulation World{ get; private set; } = null!;
    public BufferPool Pool{ get; private set; } = null!;
    
    // todo
    public int SubstepCount{
        get => __Solver.SubstepCount;
        set => __Solver.SubstepCount = value;
    }

    // todo
    public int VelocityIterationCount{
        get => __Solver.VelocityIterationCount;
        set => __Solver.VelocityIterationCount = value;
    }

    public Vector3F Gravity = new Vector3F(0, -10, 0);
    
    // ----------------------------------------------------------------------

    public void Update(float DT){
        if(!IsStarted){ return; }
        World.Timestep(DT, __TDispatcher);
    }

    public PhysicObject CreateObject(Vector3F Position = default, PhysicObject.PhysicType Type = PhysicObject.PhysicType.Dynamic) => new PhysicObject(this, Position, Type);
}