using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using WLI_Render;
using WLO.GPU;
using WLO.Math;

namespace WLO.Render.Hardware;

/*

НЕ ПРАВИЛЬНО Я ДЕЛАЮ, НУЖНО ПРИМЕНЯТЬ ЗНАЧЕНИЯ ВО ВРЕМЯ РЕНДЕРА ИЛИ КАКИХ-ТО ДЕЙСТВИЙ А НЕ СРАЗУ ПРИ ПОЛУЧЕНИИ!

к примеру,

DepthTest = true, (будет просто в памяти лежать что true),

а когда рендер будет типо такого...

api.depthtest = DepthTest;
render();

что-бы меньше команд к видеокарте бла бла бла

 */

public class OpenGL : WLI_Render.Hardware, IEquatable<OpenGL>{
    #region Значения

        #region Главные

            public readonly uint ID;

            public static uint TotalID;
            
            public GL API{ get; private set; } = null!;

            public readonly Func<string, IntPtr> API_ProcessLoader;

            public bool APIIsReady => API != null!;
            
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
                
                DefaultRenderView = GLRenderView.GetExists(this, 0);
                CRenderView = DefaultRenderView;
                Log(LogType_InitDetails, $"CurrentRenderView[0]: {CRenderView}");
            
                Log(LogType_Initialization, "Установка значений...");
                __DepthTest = true;
                DepthTest = false;
                
                __CullFace = true;
                CullFace = false;
                
                __ScissorTest = true;
                ScissorTest = false;
                
                __StencilTest = true;
                StencilTest = false;
                
                __Blend = (BlendingFactor.Zero, BlendingFactor.Zero);
                Blend = null;
                
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
        API.Viewport(0, 0, (uint)CRenderView.Viewport.X, (uint)CRenderView.Viewport.Y);
    }
    
    public void FrameStop(){
        
    }
    
    // ----------------------------------------------------------------------
    
    public WLI.GPU.Buffer CreateBuffer(uint Usage, uint Size) => GLBuffer.Create(this, BufferTargetARB.ArrayBuffer, Size);

    public WLI.GPU.Shader CreateShader(WLI.GPU.Shader.Type Stage, string Source) => new GLShader(this, Stage, Source);

    public WLI.GPU.Program CreateProgram(params WLI.GPU.Shader[] Shaders) => GLProgram.Create(this, Shaders);

    public unsafe WLI.GPU.Mesh CreateMesh<T>(WLI.GPU.VertexLayout Layout, T[] Vertices, uint[]? Indices = null) where T : unmanaged{
        GLMesh Mesh = GLMesh.Create(this);

        uint VSize = (uint)(Vertices.Length * sizeof(T));
        GLBuffer VBO = (GLBuffer)CreateBuffer((uint)BufferTargetARB.ArrayBuffer, VSize);
        VBO.Update(Vertices);
        
        Mesh.AddVertexBuffer(VBO, Layout);

        if(Indices != null && Indices.Length > 0){
            uint ISize = (uint)(Indices.Length * sizeof(uint));
            GLBuffer EBO = (GLBuffer)CreateBuffer((uint)BufferTargetARB.ElementArrayBuffer, ISize);
            EBO.Update(Indices);
            
            Mesh.SetIndexBuffer(EBO, (uint)Indices.Length);
        }

        Mesh.VertexCount = (uint)Vertices.Length;
        
        return Mesh;
    }
    
    public WLI.GPU.Texture CreateTexture(Vector2I Size, uint Format = (uint)InternalFormat.Rgba) => GLTexture2D.Create(this, Size, (InternalFormat)Format);
    
    // ----------------------------------------------------------------------

    public RenderView DefaultRenderView{ get; private set; }
    
    public readonly Dictionary<uint, GLRenderView> Registry_RenderView = new Dictionary<uint, GLRenderView>();
    private RenderView __CRenderView  = null!;
    public RenderView CRenderView{
        get => __CRenderView;
        set{
            value ??= DefaultRenderView;

            uint OldID = (__CRenderView as GLRenderView)?.ID ?? uint.MaxValue;
            uint NewID = (value         as GLRenderView)?.ID ?? 0;
            if(OldID == NewID){ return; }
            API.BindFramebuffer(FramebufferTarget.Framebuffer, NewID);
            __CRenderView = value;
        }
    }
    
    public readonly Dictionary<uint, GLProgram> Registry_Program = new Dictionary<uint, GLProgram>();
    private WLI.GPU.Program? __CurrentProgram = null!;
    public WLI.GPU.Program? CProgram{
        get => __CurrentProgram;
        set{
            uint OldID = __CurrentProgram?.ID ?? 0;
            uint NewID = value?.ID ?? 0;
            if(OldID == NewID){ return; }
            API.UseProgram(NewID);
            __CurrentProgram = value;
        }
    }
    
    public readonly Dictionary<uint, GLMesh> Registry_Mesh = new Dictionary<uint, GLMesh>();
    private WLI.GPU.Mesh? __CurrentMesh = null!;
    public WLI.GPU.Mesh? CMesh{
        get => __CurrentMesh;
        set{
            uint OldID = __CurrentMesh?.ID ?? 0;
            uint NewID = value?.ID ?? 0;
            if(OldID == NewID){ return; }
            API.BindVertexArray(NewID);
            __CurrentMesh = value;
            // todo, мне ИИ утверждает, что BindVertexArray, изменяет CIBuffer, надо будет проверить!!!
        }
    }
    
    private WLI.GPU.Buffer? __CFBuffer = null!;
    public WLI.GPU.Buffer? CFBuffer{
        get => __CFBuffer;
        set{
            uint OldID = __CFBuffer?.ID ?? 0;
            uint NewID = value?.ID ?? 0;
            if(OldID == NewID){ return; }
            API.BindBuffer(BufferTargetARB.ArrayBuffer, NewID);
            __CFBuffer = value;
        }
    }
    
    private WLI.GPU.Buffer? __CIBuffer = null!;
    public WLI.GPU.Buffer? CIBuffer{
        get => __CIBuffer;
        set{
            uint OldID = __CIBuffer?.ID ?? 0;
            uint NewID = value?.ID ?? 0;
            if(OldID == NewID){ return; }
            API.BindBuffer(BufferTargetARB.ElementArrayBuffer, NewID);
            __CIBuffer = value;
        }
    }

    public readonly Dictionary<uint, GLBuffer> Registry_Buffer = new Dictionary<uint, GLBuffer>();
    public void SetCBuffer(BufferTargetARB Target, WLI.GPU.Buffer? Buffer){
        switch(Target){
            case BufferTargetARB.ArrayBuffer: CFBuffer = Buffer; break;
            case BufferTargetARB.ElementArrayBuffer: CIBuffer = Buffer; break;
            default: throw new ExceptionWL();
        }
    }

    public WLI.GPU.Buffer? GetCBuffer(BufferTargetARB Target){
        return Target switch{
            BufferTargetARB.ArrayBuffer => CFBuffer,
            BufferTargetARB.ElementArrayBuffer => CIBuffer,
            var _ => throw new ExceptionWL()
        };
    }
    
    public readonly Dictionary<uint, GLTexture2D> Registry_Texture2D = new Dictionary<uint, GLTexture2D>();
    public WLI.GPU.Texture? CTexture2D{
        get => __TextureSlots[__CTextureSlot];
        set => SetCTexture2D(__CTextureSlot, value);
    }

    private          uint               __CTextureSlot = 0;
    private readonly WLI.GPU.Texture?[] __TextureSlots = new WLI.GPU.Texture[32 /* todo, get max opengl textures count */];

    // todo, см позже, что-то тут не чисто...
    public void SetCTexture2D(uint Slot, WLI.GPU.Texture? Texture2D){
        if(Slot >= __TextureSlots.Length){ throw new ExceptionWL("todo"); }

        uint NewID = Texture2D?.ID ?? 0;
        uint OldID = __TextureSlots[Slot]?.ID ?? 0;
        
        if(NewID == OldID){ return; }

        if(__CTextureSlot != Slot){
            API.ActiveTexture(TextureUnit.Texture0 + (int)Slot);
            __CTextureSlot = Slot;
        }
        
        API.BindTexture(TextureTarget.Texture2D, NewID);

        __TextureSlots[Slot] = Texture2D;
    }
    
    // ----------------------------------------------------------------------

    private bool __DepthTest;
    public bool DepthTest{
        get => __DepthTest;
        set{
            if(__DepthTest == value){ return; }
            if(value){ API.Enable(EnableCap.DepthTest); }else{ API.Disable(EnableCap.DepthTest); }
            __DepthTest = value;
        }
    }
    
    private bool __CullFace;
    public bool CullFace{
        get => __CullFace;
        set{
            if(__CullFace == value){ return; }
            if(value){ API.Enable(EnableCap.CullFace); }else{ API.Disable(EnableCap.CullFace); }
            __CullFace = value;
        }
    }
    
    private bool __ScissorTest;
    public bool ScissorTest{
        get => __ScissorTest;
        set{
            if(__ScissorTest == value){ return; }
            if(value){ API.Enable(EnableCap.ScissorTest); }else{ API.Disable(EnableCap.ScissorTest); }
            __ScissorTest = value;
        }
    }
    
    private bool __StencilTest;
    public bool StencilTest{
        get => __StencilTest;
        set{
            if(__StencilTest == value){ return; }
            if(value){ API.Enable(EnableCap.StencilTest); }else{ API.Disable(EnableCap.StencilTest); }
            __StencilTest = value;
        }
    }
    
    private (BlendingFactor, BlendingFactor)? __Blend;
    public (BlendingFactor, BlendingFactor)? Blend{
        get => __Blend;
        set{
            if(__Blend == value){ return; }
            if(value == null){
                API.Disable(EnableCap.Blend);
            }else{
                if(__Blend == null){ API.Enable(EnableCap.Blend); }
                API.BlendFunc(value.Value.Item1, value.Value.Item2);
            }
            __Blend = value;
        }
    }
    
    // ----------------------------------------------------------------------
    
    public void Clear(Color4B Color){
        API.ClearColor(Color.R / 255f, Color.G / 255f, Color.B / 255f, Color.A / 255f);
        API.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
    
    public void Draw(uint Count, uint Start = 0){
        if(CProgram == null! || CMesh == null!){ return; }

        API.DrawArrays(PrimitiveType.Triangles, (int)Start, Count);
    }
    
    public unsafe void DrawIndexed(uint Count, uint StartIndex = 0, int BaseVertex = 0){
        if(CProgram == null! || CMesh == null!){ return; }
        
        void* Offset = (void*)(StartIndex * sizeof(uint));
        if(BaseVertex == 0){
            API.DrawElements(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, Offset);   
        }else{
            API.DrawElementsBaseVertex(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, Offset, BaseVertex);  
        }
    }

    public void Draw(WLI.GPU.Mesh Mesh, WLI.GPU.Program? Program){
        if(Program != null){ CProgram = Program; }
        
        CMesh = Mesh;
        
        if(Mesh.IndexCount > 0){
            DrawIndexed(Mesh.IndexCount);
        }else{
            Draw(Mesh.VertexCount);   
        }
    }

    public void Draw(WLI.GPU.Mesh Mesh) => Draw(Mesh, null);
    
    // ----------------------------------------------------------------------

    public override string ToString() => $"OpenGL({ID})";

    public bool Equals(OpenGL? Other){
        if(Other is null){ return false; }
        if(ReferenceEquals(this, Other)){ return true; }

        return ID == Other.ID;
    }

    public override bool Equals(object? Object) => Object is OpenGL Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(ID);
}