using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using WLI_Render;
using WLI.Render;
using WLO.GPU;
using WLO.Math;

namespace WLO.Render.Hardware;

public class OpenGL : WLI_Render.Hardware, IEquatable<OpenGL>{
    #region Значения

        #region Главные

            public readonly uint ID;

            public static uint TotalID;
            
            public GL API{ get; private set; } = null!;

            public readonly Func<string, IntPtr> API_ProcessLoader;

            public bool APIIsReady => API != null!;
            
            public RenderPool Pool{ get; private set; } = null!;
            
        #endregion
        
        #region Логирование

            public WLI.Logger? CurrentLogger = null!;
        
            public DebugProc API_DebugLogger{ get; private set; } = null!;
            
            public readonly bool API_HasDebugLogger;

            public uint LogType_Initialization    = (uint)WLI.Logger.Type.Info;
            public uint LogType_InitDetails       = (uint)WLI.Logger.Type.Info;
            public uint LogType_DebugLogger_Info  = (uint)WLI.Logger.Type.Info;
            public uint LogType_DebugLogger_Warn  = (uint)WLI.Logger.Type.Warning;
            public uint LogType_DebugLogger_Error = (uint)WLI.Logger.Type.Error;
            public uint LogType_DebugLogger_Fatal = (uint)WLI.Logger.Type.Fatal;
            public uint LogType_Uniform           = (uint)WLI.Logger.Type.Warning;

            public bool DebugLogger_ThrowExceptionOnFatalErrors = true;
            
        #endregion
        
    #endregion
    // ----------------------------------------------------------------------
    #region Запуск/Остановка
    
        public bool IsStarted{ get; private set; }
    
        public OpenGL(Func<string, IntPtr> ProcessLoader, StartParameters? Parameters = null, bool StartImmediately = false){
            ID = TotalID; TotalID++;
            
            StartParameters Parameters__ = new StartParameters{
                DebugLogger = Parameters.HasValue && Parameters.Value.DebugLogger.HasValue ? Parameters.Value.DebugLogger.Value : false,
                UseThisLogger = Parameters.HasValue && Parameters.Value.UseThisLogger != null ? Parameters.Value.UseThisLogger : WL.Logger.CurrentLogger
            };
            
            API_ProcessLoader = ProcessLoader;
            API_HasDebugLogger = Parameters__.DebugLogger.Value;

            CurrentLogger = Parameters__.UseThisLogger;
            
            if(StartImmediately){ Start(); }
        }
        public struct StartParameters{
            public bool?       DebugLogger;
            public WLI.Logger? UseThisLogger;
        }
        
        public unsafe void Start(){
            try{
                if(IsStarted){ throw new ExceptionWL("Рендер OpenGL уже и так был запущен!"); }

                Log(LogType_Initialization, "Запуск OpenGL...");
                
                Log(LogType_InitDetails, $"Параметры! HasDebugLogger: {API_HasDebugLogger}");
                
                API = WL.OpenGL.CreateAPI(API_ProcessLoader);
                Log(LogType_InitDetails, $"API: {API}, ProcessLoader: {API_ProcessLoader}");
                
                if(API_HasDebugLogger){
                    API_DebugLogger = __DebugLoggerCallback;
                    WL.OpenGL.SetupDebugLogger(API, API_DebugLogger);
                    Log(LogType_InitDetails, $"DebugLogger: {API_DebugLogger}");
                }
                
                Pool = new RenderPool(this);
                Pool.Start();
                
                Log(LogType_InitDetails, $"RenderPool: {Pool}");
                
                Log(LogType_InitDetails, $"DefaultView: {Pool.DefaultView}");
                
                Log(LogType_Initialization, "OpenGL запущен!");
                
                IsStarted = true;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при запуске OpenGL!", e);
            } 
        }
        
        public bool Stop(){
            try{
                if(!IsStarted){ return false; }
                
                Log(LogType_Initialization, "Остановка OpenGL...");
                
                API = null!;
                
                Log(LogType_Initialization, "OpenGL остановлен!");
                
                IsStarted = false;
                return true;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при остановке OpenGL!", e);
            } 
        }
    
    #endregion
    // ----------------------------------------------------------------------
    #region Остальное

        public void Log(uint Type, object Message, string Prefix = "GL"){
            CurrentLogger?.PrefixPush(Prefix);
            CurrentLogger?.Log(Type, Message);
            CurrentLogger?.PrefixPop();
        }

        private void __DebugLoggerCallback(GLEnum Source, GLEnum Type, int ID, GLEnum Severity, int L, nint MessagePtr, nint UserParam){
            string Message = Marshal.PtrToStringAnsi(MessagePtr, L);

            uint LogType = Severity switch{
                GLEnum.DebugSeverityHigh when DebugLogger_ThrowExceptionOnFatalErrors => throw new ExceptionWL($"Фатальная ошибка OpenGL: {Message}"),
                GLEnum.DebugSeverityHigh   => LogType_DebugLogger_Fatal,
                GLEnum.DebugSeverityMedium => LogType_DebugLogger_Error,
                GLEnum.DebugSeverityLow    => LogType_DebugLogger_Warn,
                var _                      => LogType_DebugLogger_Info
            };

            Log(LogType, Message, "GLDL");
        }
    
    #endregion
    // ----------------------------------------------------------------------

    public void FrameStart(){
        Pool.BindForView();
        API.Viewport(0, 0, (uint)Pool.GetView().Viewport.X, (uint)Pool.GetView().Viewport.Y);
    }
    
    public void FrameStop(){
        
    }
    
    // ----------------------------------------------------------------------
    
    public GLBuffer CreateBuffer(BufferTargetARB Target, uint Size) => GLBuffer.Create(this, Target, Size);

    public GLShader CreateShader(WLI.GPU.Shader.Type Stage, string Source) => new GLShader(this, Stage, Source);

    public GLProgram CreateProgram(params WLI.GPU.Shader[] Shaders) => GLProgram.Create(this, Shaders);

    public GLMesh CreateMesh<T>(WLI.GPU.VertexLayout Layout, T[] Vertices, uint[]? Indices = null) where T : unmanaged{
        GLMesh Mesh = GLMesh.Create(this);

        Bounds Bounds = new Bounds {
            Min = new Vector3F(float.MaxValue, float.MaxValue, float.MaxValue),
            Max = new Vector3F(float.MinValue, float.MinValue, float.MinValue)
        };

        VertexAttribute PositionAttribute = Layout.Attributes.FirstOrDefault(A => A.Name.ToLower().Contains("pos"));

        // todo, le super govno
        if(PositionAttribute.Count >= 3){
            uint PositionOffset = 0;
            foreach(VertexAttribute Attribute in Layout.Attributes){
                if(Attribute.Name == PositionAttribute.Name){ break; }
                PositionOffset += (uint)Attribute.Count * VertexAttribute.GetTypeSize(Attribute.Type);
            }

            unsafe{
                fixed(T* PtrVertices = Vertices){
                    byte* PtrByte = (byte*)PtrVertices;
                    uint Stride = Layout.Stride;
                    
                    for (int i = 0; i < Vertices.Length; i++) {
                        Vector3F P = *(Vector3F*)(PtrByte + (i * Stride) + PositionOffset);

                        if(P.X < Bounds.Min.X){ Bounds.Min.X = P.X; }
                        if(P.Y < Bounds.Min.Y){ Bounds.Min.Y = P.Y; }
                        if(P.Z < Bounds.Min.Z){ Bounds.Min.Z = P.Z; }
                        
                        if(P.X > Bounds.Max.X){ Bounds.Max.X = P.X; }
                        if(P.Y > Bounds.Max.Y){ Bounds.Max.Y = P.Y; }
                        if(P.Z > Bounds.Max.Z){ Bounds.Max.Z = P.Z; }
                    }
                }
            }
        }

        Mesh.Bounds = Bounds;
        
        unsafe{
            uint VSize = (uint)(Vertices.Length * sizeof(T));
            GLBuffer VBO = CreateBuffer(BufferTargetARB.ArrayBuffer, VSize);
            VBO.Update(Vertices);
        
            Mesh.AddVertexBuffer(VBO, Layout);

            if(Indices != null && Indices.Length > 0){
                uint ISize = (uint)(Indices.Length * sizeof(uint));
                GLBuffer EBO = CreateBuffer(BufferTargetARB.ElementArrayBuffer, ISize);
                EBO.Update(Indices);
            
                Mesh.SetIndexBuffer(EBO, (uint)Indices.Length);
            }

            Mesh.VertexCount = (uint)Vertices.Length;
        }
        
        return Mesh;
    }
    
    public GLTexture2D CreateTexture2D(Vector2I Size, uint Format = (uint)InternalFormat.Rgba) => GLTexture2D.Create(this, Size, (InternalFormat)Format);

    // ----------------------------------------------------------------------

    public void Clear(Color4B Color) => Clear(Color, true, true, true);

    public void Clear(Color4B Color, bool ColorBuffer, bool DepthBuffer = false, bool StencilBuffer = false){
        Pool.BindStates();
        
        if(ColorBuffer){ API.ClearColor(Color.R / 255f, Color.G / 255f, Color.B / 255f, Color.A / 255f); }

        ClearBufferMask Mask = 0;
        if(ColorBuffer  ){ Mask |= ClearBufferMask.ColorBufferBit  ; }
        if(DepthBuffer  ){ Mask |= ClearBufferMask.DepthBufferBit  ; }
        if(StencilBuffer){ Mask |= ClearBufferMask.StencilBufferBit; }

        if(Mask != 0){ API.Clear(Mask); }
    }
    
    public void Draw(uint Count, uint Start = 0){
        if(!Pool.CanDraw){ return; }
        Pool.BindForDraw();

        API.DrawArrays(PrimitiveType.Triangles, (int)Start, Count);
    }
    
    public unsafe void DrawIndexed(uint Count, uint StartIndex = 0, int BaseVertex = 0){
        if(!Pool.CanDraw){ return; }
        Pool.BindForDraw();
        
        void* Offset = (void*)(StartIndex * sizeof(uint));
        if(BaseVertex == 0){
            API.DrawElements(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, Offset);   
        }else{
            API.DrawElementsBaseVertex(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, Offset, BaseVertex);  
        }
    }

    public void Draw(GLMesh Mesh, GLProgram? Program = null){
        if(Program != null){ Pool.SetProgram(Program); }
        
        Pool.SetMesh(Mesh);
        
        if(Mesh.IndexCount > 0){
            DrawIndexed(Mesh.IndexCount);
        }else{
            Draw(Mesh.VertexCount);   
        }
    }
    
    // ----------------------------------------------------------------------

    public override string ToString() => $"OpenGL({ID})";

    public bool Equals(OpenGL? Other){
        if(Other is null){ return false; }
        if(ReferenceEquals(this, Other)){ return true; }

        return ID == Other.ID;
    }

    public override bool Equals(object? Object) => Object is OpenGL Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(ID);
    
    // ----------------------------------------------------------------------
    
    // todo, абабаба баг!!! будет баг!!!! если кто-то из вне поменяет что-то в opengl, это всё взлетит на воздух!!!!
    public class RenderPool{
        private readonly OpenGL Owner;

        // ----------------------------------------------------------------------

        public uint MaxTextureSlots{ get; private set; }

        public GLView DefaultView{ get; private set; } = null!;

        public RenderPool(OpenGL Render){ Owner = Render; }

        public void Start(){
            Owner.API.GetInteger(GetPName.MaxCombinedTextureImageUnits, out int MaxTextureSlots__);
            MaxTextureSlots = (uint)MaxTextureSlots__;
            
            TargetTexture2D = new GLTexture2D[MaxTextureSlots];
            BoundTexture2D  = new uint       [MaxTextureSlots];
            
            // todo, узнать кол-во?
            TargetUniformBlock = new UniformBlock[32];
            BoundUniformBlock  = new uint[32];
            
            ForceReset();
            
            DefaultView = GLView.GetExists(Owner, 0);
            TargetView = DefaultView;
        }
        
        // ----------------------------------------------------------------------
        
        public readonly Dictionary<uint, GLView     > RegistryView      = new Dictionary<uint, GLView     >();
        public readonly Dictionary<uint, GLBuffer   > RegistryBuffer    = new Dictionary<uint, GLBuffer   >();
        public readonly Dictionary<uint, GLProgram  > RegistryProgram   = new Dictionary<uint, GLProgram  >();
        public readonly Dictionary<uint, GLMesh     > RegistryMesh      = new Dictionary<uint, GLMesh     >();
        public readonly Dictionary<uint, GLTexture2D> RegistryTexture2D = new Dictionary<uint, GLTexture2D>();
        
        // ----------------------------------------------------------------------
        
        private GLView TargetView = null!;
        private uint    BoundView  = 0;
        
        public void SetView(GLView? View, bool Immediately = false){
            View ??= DefaultView;
            TargetView = View;
            if(Immediately){ BindView(); }
        }

        public GLView GetView() => TargetView;
        public uint GetBoundView() => BoundView;
        
        // ----------------------------------------------------------------------
        
        private GLBuffer? TargetFBuffer = null;
        private uint      BoundFBuffer  = 0;
        
        public void SetFBuffer(GLBuffer? FBuffer, bool Immediately = false){
            if(FBuffer != null && FBuffer.Target != BufferTargetARB.ArrayBuffer){ throw new ExceptionWL("todo, not array buff"); }
            TargetFBuffer = FBuffer;
            if(Immediately){ BindFBuffer(); }
        }

        public GLBuffer? GetFBuffer() => TargetFBuffer;
        
        // ----------------------------------------------------------------------
        
        private GLBuffer? TargetIBuffer = null;
        private uint      BoundIBuffer  = 0;
        
        public void SetIBuffer(GLBuffer? IBuffer, bool Immediately = false){
            if(IBuffer != null && IBuffer.Target != BufferTargetARB.ElementArrayBuffer){ throw new ExceptionWL("todo, not ELEMENT array buff"); }
            TargetIBuffer = IBuffer;
            if(Immediately){ BindIBuffer(); }
        }

        public GLBuffer? GetIBuffer() => TargetIBuffer;
        
        // ----------------------------------------------------------------------
        
        private GLBuffer? TargetUBuffer = null;
        private uint      BoundUBuffer  = 0;
        
        public void SetUBuffer(GLBuffer? UBuffer, bool Immediately = false){
            if(UBuffer != null && UBuffer.Target != BufferTargetARB.UniformBuffer){ throw new ExceptionWL("todo, not uniform buff"); }
            TargetUBuffer = UBuffer;
            if(Immediately){ BindUBuffer(); }
        }

        public GLBuffer? GetUBuffer() => TargetUBuffer;
        
        // ----------------------------------------------------------------------

        public void SetBuffer(BufferTargetARB Target, GLBuffer? Buffer, bool Immediately = false){
            switch(Target){
                case BufferTargetARB.ArrayBuffer       : SetFBuffer(Buffer, Immediately); break;
                case BufferTargetARB.ElementArrayBuffer: SetIBuffer(Buffer, Immediately); break;
                case BufferTargetARB.UniformBuffer     : SetUBuffer(Buffer, Immediately); break;
                default:
                    WL.Logger.Warn($"todo, unknown buffer type [{Target}] (setbuffer)");
                    break;
            }
        }

        public GLBuffer? GetBuffer(BufferTargetARB Target){
            switch (Target) {
                case BufferTargetARB.ArrayBuffer       : return GetFBuffer();
                case BufferTargetARB.ElementArrayBuffer: return GetIBuffer();
                case BufferTargetARB.UniformBuffer     : return GetUBuffer();
                default:
                    WL.Logger.Warn($"todo, unknown buffer type [{Target}] (getbuffer)");
                    return null;
            }
        }
        
        // ----------------------------------------------------------------------
        
        private GLProgram? TargetProgram = null;
        private uint       BoundProgram  = 0;
        
        public void SetProgram(GLProgram? Program, bool Immediately = false){
            TargetProgram = Program;
            if(Immediately){ BindProgram(); }
        }

        public GLProgram? GetProgram() => TargetProgram;
        public uint GetBoundProgram() => BoundProgram;
        
        // ----------------------------------------------------------------------
        
        private GLMesh? TargetMesh = null;
        private uint    BoundMesh  = 0;

        public void SetMesh(GLMesh? Mesh, bool Immediately = false){
            TargetMesh = Mesh;
            if(Immediately){ BindMesh(); }
        }

        public GLMesh? GetMesh() => TargetMesh;
        public uint GetBoundMesh() => BoundMesh;
        
        // ----------------------------------------------------------------------
        
        private GLTexture2D?[] TargetTexture2D = null!;
        private uint[]         BoundTexture2D  = null!;

        private ulong __ActiveSlotsMask = 0;
        private uint  __MaxUsedSlots    = 0;
        
        public void SetTexture2D(GLTexture2D? Texture2D, uint Slot = 0, bool Immediately = false){
            if(Slot > MaxTextureSlots){ WL.Logger.Warn($"todo, slot [{Slot}] > maxtextureslots {MaxTextureSlots}!"); return; }
            TargetTexture2D[Slot] = Texture2D;

            if(Slot < 64){
                if(Texture2D != null){
                    __ActiveSlotsMask |= (1UL << (int)Slot);
                }else{
                    __ActiveSlotsMask &= ~(1UL << (int)Slot);
                }
            }else{
                if(Slot > __MaxUsedSlots){ __MaxUsedSlots = Slot; }
            }
            
            if(Immediately){ BindTexture2D(Slot); }
        }

        public GLTexture2D? GetTexture2D(uint Slot = 0) => TargetTexture2D[Slot] ?? null;
        
        // ----------------------------------------------------------------------

        private WLI.Render.UniformBlock?[] TargetUniformBlock = null!;
        private uint[]                     BoundUniformBlock  = null!;
        
        private ulong __ActiveUniformBlockMask = 0;
        
        public void SetUniformBlock<T>(UniformBlock<T>? UniformBlock, uint Slot = 0, bool Immediately = false) where T : unmanaged{
            if(Slot >= TargetUniformBlock.Length){ return; }

            TargetUniformBlock[Slot] = UniformBlock;

            uint ID = UniformBlock?.ID ?? 0;

            if(ID != 0){
                __ActiveUniformBlockMask |= (1UL << (int)Slot);
            }else{
                __ActiveUniformBlockMask &= ~(1UL << (int)Slot);
            }

            if(Immediately){ BindUniformBlock(Slot); }
        }
        
        // ----------------------------------------------------------------------
        
        public void BindView(){
            uint ID = TargetView.ID;
            if(BoundView == ID){ return; }
            BoundView = ID;
            Owner.API.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
        }
        
        public void BindFBuffer(){
            uint ID = TargetFBuffer?.ID ?? 0;
            if(BoundFBuffer == ID){ return; }
            BoundFBuffer = ID;
            Owner.API.BindBuffer(BufferTargetARB.ArrayBuffer, ID);
        }
        
        public void BindIBuffer(){
            uint ID = TargetIBuffer?.ID ?? 0;
            if(BoundIBuffer == ID){ return; }
            BoundIBuffer = ID;
            Owner.API.BindBuffer(BufferTargetARB.ElementArrayBuffer, ID);
        }
        
        public void BindUBuffer(){
            uint ID = TargetUBuffer?.ID ?? 0;
            if(BoundUBuffer == ID){ return; }
            BoundUBuffer = ID;
            Owner.API.BindBuffer(BufferTargetARB.UniformBuffer, ID);
        }
        
        public void BindProgram(){
            uint ID = TargetProgram?.ID ?? 0;
            if(BoundProgram == ID){ return; }
            BoundProgram = ID;
            Owner.API.UseProgram(ID);
        }
        
        public void BindMesh(){
            if(TargetMesh != null){
                if(TargetMesh.Indices is GLBuffer Indexes){
                    SetIBuffer(Indexes);
                }
            }else{
                SetIBuffer(null);   
            }
            
            uint ID = TargetMesh?.ID ?? 0;
            if(BoundMesh == ID){ return; }
            BoundMesh = ID;
            Owner.API.BindVertexArray(ID);
        }

        private uint ActiveTexture2DSlot = 0;
        public void BindTexture2D(uint Slot = 0){
            uint ID = TargetTexture2D[Slot]?.ID ?? 0;
            if(BoundTexture2D[Slot] == ID){ return; }
            BoundTexture2D[Slot] = ID;

            if(ActiveTexture2DSlot != Slot){ Owner.API.ActiveTexture((GLEnum)((uint)TextureUnit.Texture0 + Slot)); ActiveTexture2DSlot = Slot; }
            Owner.API.BindTexture(TextureTarget.Texture2D, ID);
        }

        public void BindTexture2DAll(){
            if(__ActiveSlotsMask != 0){
                ulong Mask = __ActiveSlotsMask;
                while(Mask != 0){
                    int Slot = BitOperations.TrailingZeroCount(Mask);
                    BindTexture2D((uint)Slot);
                    Mask &= ~(1UL << Slot);
                }
            }

            if(__MaxUsedSlots >= 64){
                for(uint i = 64; i <= __MaxUsedSlots; i++){
                    if(TargetTexture2D[i] != null){ BindTexture2D(i); }
                }
            }
        }

        public void BindUniformBlock(uint Slot){
            uint ID = TargetUniformBlock[Slot]?.ID ?? 0;
            if(BoundUniformBlock[Slot] == ID){ return; }

            BoundUniformBlock[Slot] = ID;
            Owner.API.BindBufferBase(BufferTargetARB.UniformBuffer, Slot, ID);
        }

        public void BindUniformBlockAll(){
            if(__ActiveUniformBlockMask == 0){ return; }

            ulong Mask = __ActiveUniformBlockMask;
            while(Mask != 0){
                int Slot = BitOperations.TrailingZeroCount(Mask);
                BindUniformBlock((uint)Slot);
                Mask &= ~(1UL << Slot);
            }
        }
        
        // ----------------------------------------------------------------------

        private bool TargetDepthTest;
        private bool BoundDepthTest;

        public void SetDepthTest(bool DepthTest, bool Immediately = false){
            TargetDepthTest = DepthTest;
            if(Immediately){ ApplyCap(EnableCap.DepthTest, TargetDepthTest, ref BoundDepthTest); }
        }
        public bool GetDepthTest() => TargetDepthTest;
        
        private bool TargetCullFace;
        private bool BoundCullFace;
        
        // todo, сделать заместо bool, тип CullFace Mode
        public void SetCullFace(bool CullFace, bool Immediately = false){
            TargetCullFace = CullFace;
            if(Immediately){ ApplyCap(EnableCap.CullFace, TargetCullFace, ref BoundCullFace); }
        }
        public bool GetCullFace() => TargetCullFace;
        
        private bool TargetScissorTest;
        private bool BoundScissorTest;
        
        public void SetScissorTest(bool ScissorTest, bool Immediately = false){
            TargetScissorTest = ScissorTest;
            if(Immediately){ ApplyCap(EnableCap.ScissorTest, TargetScissorTest, ref BoundScissorTest); }
        }
        public bool GetScissorTest() => TargetScissorTest;
        
        private bool TargetStencilTest;
        private bool BoundStencilTest;
        
        public void SetStencilTest(bool StencilTest, bool Immediately = false){
            TargetStencilTest = StencilTest;
            if(Immediately){ ApplyCap(EnableCap.StencilTest, TargetStencilTest, ref BoundStencilTest); }
        }
        public bool GetStencilTest() => TargetStencilTest;
        
        private (BlendingFactor Source, BlendingFactor Destination)? TargetBlend;
        private (BlendingFactor Source, BlendingFactor Destination)? BoundBlend;
        
        public void SetBlend((BlendingFactor Source, BlendingFactor Destination)? Blend, bool Immediately = false){
            TargetBlend = Blend;
            if(Immediately){ ApplyBlend(); }
        }
        public (BlendingFactor Source, BlendingFactor Destination)? GetBlend() => TargetBlend;
        
        private bool TargetFrameSRGB;
        private bool BoundFrameSRGB;

        public void SetFrameSRGB(bool FrameSRGB, bool Immediately = false){
            TargetFrameSRGB = FrameSRGB;
            if(Immediately){ ApplyCap(EnableCap.FramebufferSrgb, TargetFrameSRGB, ref BoundFrameSRGB); }
        }
        public bool GetFrameSRGB() => TargetFrameSRGB;
        
        private void ApplyCap(EnableCap Cap, bool Target, ref bool Bound){
            if(Target != Bound){
                if(Target){
                    Owner.API.Enable(Cap);   
                }else{
                    Owner.API.Disable(Cap);
                }
                Bound = Target;
            }
        }

        private void ApplyBlend(){
            if(TargetBlend != BoundBlend){
                if(TargetBlend == null){
                    Owner.API.Disable(EnableCap.Blend);
                }else{
                    Owner.API.Enable(EnableCap.Blend); // мне лень тут делать микрооптимизацию
                    Owner.API.BlendFunc(TargetBlend.Value.Source, TargetBlend.Value.Destination);
                }
                BoundBlend = TargetBlend;
            }
        }
        
        public void BindStates(){
            ApplyCap(EnableCap.DepthTest      , TargetDepthTest  , ref BoundDepthTest  );
            ApplyCap(EnableCap.CullFace       , TargetCullFace   , ref BoundCullFace   );
            ApplyCap(EnableCap.ScissorTest    , TargetScissorTest, ref BoundScissorTest);
            ApplyCap(EnableCap.StencilTest    , TargetStencilTest, ref BoundStencilTest);
            ApplyCap(EnableCap.FramebufferSrgb, TargetFrameSRGB  , ref BoundFrameSRGB  );
            
            ApplyBlend();
        }
        
        // ----------------------------------------------------------------------
        
        public void BindForDraw(){
            BindStates();
            
            BindProgram();
            BindMesh();
            BindUniformBlockAll();
            BindTexture2DAll();
        }

        public void BindForView(){
            BindView();
        }

        public void ForceReset(){
            const uint NullState = uint.MaxValue;

            BoundView = NullState;
            BoundProgram = NullState;
            BoundMesh = NullState;
            BoundFBuffer = NullState;
            BoundIBuffer = NullState;
            BoundUBuffer = NullState;
            ActiveTexture2DSlot = NullState;
            
            Array.Fill(BoundTexture2D, NullState);
            Array.Fill(BoundUniformBlock, NullState);
            
            Owner.API.Enable(EnableCap.DepthTest);
            BoundDepthTest = true;
            Owner.API.Disable(EnableCap.CullFace);
            BoundCullFace = false;
            Owner.API.Disable(EnableCap.ScissorTest);
            BoundScissorTest = false;
            Owner.API.Disable(EnableCap.StencilTest);
            BoundStencilTest = false;
            Owner.API.Disable(EnableCap.Blend);
            BoundBlend = null;
            Owner.API.Disable(EnableCap.FramebufferSrgb);
            BoundFrameSRGB = false;
        }

        public bool CanDraw => TargetProgram != null && TargetMesh != null;
    }
}