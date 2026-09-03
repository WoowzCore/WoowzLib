using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;

namespace WLO.Physic3D;

public class Bepu : IDisposable{
    public  Simulation        Simulation{ get; private set; }
    private BufferPool        __BufferPool;
    private ThreadDispatcher  __ThreadDispatcher;
    
    public Bepu(){}

    public void Start(){
        __BufferPool = new BufferPool();
        __ThreadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);
        
        Simulation = Simulation.Create(
            __BufferPool,
            new NarrowPhaseCallbacks(),
            new PoseIntegratorCallbacks(),
            new SolveDescription(8, 1)
        );
    }

    public void Update(float DT){
        Simulation.Timestep(DT, __ThreadDispatcher);
    }

    public void Dispose(){
        Simulation.Dispose();
        __ThreadDispatcher.Dispose();
        __BufferPool.Clear();
    }

    private struct NarrowPhaseCallbacks : INarrowPhaseCallbacks{
        public void Initialize(Simulation simulation){}
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin){
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB){
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, [UnscopedRef] out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>{
            pairMaterial.FrictionCoefficient = 1;
            pairMaterial.MaximumRecoveryVelocity = 2;
            pairMaterial.SpringSettings = new SpringSettings(30, 1);
            
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold){
            return true;
        }
        
        public void Dispose(){}
    }
    
    private struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks{
        public Vector3     Gravity;
        public Vector3Wide __GravityWide;

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
        public bool AllowSubstepsForUnconstrainedBodies => false;
        public bool IntegrateVelocityForKinematics => false;
        
        public PoseIntegratorCallbacks(Vector3 Gravity){
            this.Gravity = Gravity;
            __GravityWide = default;
        }
        
        public void Initialize(Simulation simulation){}
        
        public void PrepareForIntegration(float dt){
            __GravityWide = Vector3Wide.Broadcast(Gravity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity){
            velocity.Linear += __GravityWide * dt;
        }
    }
}