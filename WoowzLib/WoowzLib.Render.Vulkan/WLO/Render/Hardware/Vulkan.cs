using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using WLI_Render;
using WLI.GPU;
using WLO.Math;
using Buffer = WLI.GPU.Buffer;

namespace WLO.Render.Hardware;

public unsafe class Vulkan : WLI_Render.Hardware{
    // ----------------------------------------------------------------------
    #region Параметры

        #region Системное Vulkan

            public  Vk API => __API;
            private Vk        __API;

            public  Shaderc API_ShaderC => __ShaderC;
            private Shaderc                __ShaderC;

        #endregion
    
        public WLI.Logger? CurrentLogger = WL.Logger.CurrentLogger;

    #endregion
    // ----------------------------------------------------------------------
    #region Запуск и остановка
    
        public Vulkan(StartParameters? Properties = null){
            __StartParameters = Properties ?? new StartParameters();
        }
        public struct StartParameters{
            public WLI.Logger? UseThisLogger;
        }
        private readonly StartParameters __StartParameters;

        public bool IsStarted{ get; private set; }

        /*
         * Запускает Vulkan
         */
        public void Start(){
            try{
                if(IsStarted){ throw new ExceptionWL("Vulkan уже запущен!"); }

                CurrentLogger = __StartParameters.UseThisLogger;
                
                __API     = Vk     .GetApi();
                __ShaderC = Shaderc.GetApi();
                
                IsStarted = true;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при запуске Vulkan!", e);
            }
        }
        
        /**
         * Останавливает Vulkan
         */
        public void Stop(){
            try{
                if(!IsStarted){ throw new ExceptionWL("Vulkan даже не был запущен!"); }

                IsStarted = false;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при остановке Vulkan!", e);
            }
        }

    #endregion
    // ----------------------------------------------------------------------
    
    public Buffer CreateBuffer(uint Usage, uint Size){
        throw new NotImplementedException();
    }
    public Texture CreateTexture(Vector2I Size, uint Format){
        throw new NotImplementedException();
    }
    public Shader CreateShader(string VertexSource, string FragmentSource){
        throw new NotImplementedException();
    }
    public Mesh CreateMesh<T>(T[] Vertices, uint[]? Indices = null) where T : unmanaged{
        throw new NotImplementedException();
    }
    
    public RenderView CurrentRenderView{ get; }
    
    public void FrameStart(RenderView? Target = null){
        
    }
    
    public void FrameStop(){
        
    }
}