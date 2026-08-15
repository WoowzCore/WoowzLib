using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
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

            public  Instance API_Instance => __Instance;
            private Instance                 __Instance;
            
            public Version32 API_Version{ get; private set; }
            
        #endregion
    
        public WLI.Logger? CurrentLogger = WL.Logger.CurrentLogger;

    #endregion
    // ----------------------------------------------------------------------
    #region Запуск и остановка
    
        // Vulkan запущен?
        public bool IsStarted{ get; private set; }
        
        public Vulkan(StartParameters? Properties = null, bool StartImmediately = false){
            __StartParameters = new StartParameters{
                UseThisLogger = Properties?.UseThisLogger,
                ProjectInfo   = Properties.HasValue && Properties.Value.ProjectInfo  .HasValue ? Properties.Value.ProjectInfo  .Value : WL.Core.ProjectInfo,
                EngineInfo    = Properties.HasValue && Properties.Value.EngineInfo   .HasValue ? Properties.Value.EngineInfo   .Value : WL.Core.EngineInfo,
                VulkanVersion = Properties.HasValue && Properties.Value.VulkanVersion.HasValue ? Properties.Value.VulkanVersion.Value : Vk.Version12
            };

            if(StartImmediately){ Start(); }
        }
        public struct StartParameters{
            public WLI.Logger?  UseThisLogger;
            public ProjectInfo? ProjectInfo;
            public ProjectInfo? EngineInfo;
            public Version32?   VulkanVersion;
        }
        private readonly StartParameters __StartParameters;

        /*
         * Запускает Vulkan
         */
        public void Start(){
            try{
                if(IsStarted){ throw new ExceptionWL("Vulkan уже запущен!"); }

                CurrentLogger = __StartParameters.UseThisLogger;

                CurrentLogger?.Info("Запуск Vulkan...");
                
                API_Version = __StartParameters.VulkanVersion!.Value;
                
                __API     = Vk     .GetApi();
                __ShaderC = Shaderc.GetApi();

                __Instance = __CreateInstance(API_Version, __StartParameters.ProjectInfo!.Value, __StartParameters.EngineInfo!.Value);
                
                CurrentLogger?.Info("Vulkan запущен!");
                
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

                CurrentLogger?.Info("Остановка Vulkan...");
                
                void Destroy<T>(ref T H, Action<T> Action) where T : unmanaged{ if(Unsafe.As<T, ulong>(ref H) != 0){ Action(H); H = default; } }
                
                Destroy(ref __Instance, H => __API.DestroyInstance(__Instance, null));
                
                __ShaderC = null!;
                __API     = null!;
                
                CurrentLogger?.Info("Vulkan остановлен!");
                
                IsStarted = false;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при остановке Vulkan!", e);
            }
        }

    #endregion
    // ----------------------------------------------------------------------
    #region Базовые компоненты

        // todo, add validation layers + check vulkan sdk, СМ. Vulkan_REFERENCE_DELETE_SOON
        private Instance __CreateInstance(Version32 VulkanVersion, ProjectInfo ProjectInfo, ProjectInfo EngineInfo){
            IntPtr Ptr_ProjectName = IntPtr.Zero;
            IntPtr Ptr_EngineName  = IntPtr.Zero;;
            
            try{
                Ptr_ProjectName = SilkMarshal.StringToPtr(ProjectInfo.Name);
                Ptr_EngineName  = SilkMarshal.StringToPtr(EngineInfo .Name);

                Version32 ProjectVersion = new Version32((uint)ProjectInfo.Version.Major, (uint)ProjectInfo.Version.Minor, (uint)System.Math.Max(0, ProjectInfo.Version.Build));
                Version32 EngineVersion  = new Version32((uint)EngineInfo .Version.Major, (uint)EngineInfo .Version.Minor, (uint)System.Math.Max(0, EngineInfo .Version.Build));

                ApplicationInfo VK_ApplicationInfo = new ApplicationInfo{ ApiVersion = VulkanVersion, PApplicationName = (byte*)Ptr_ProjectName, PEngineName = (byte*)Ptr_EngineName, ApplicationVersion = ProjectVersion, EngineVersion = EngineVersion, SType = StructureType.ApplicationInfo };

                InstanceCreateInfo VK_InstanceCreateInfo = new InstanceCreateInfo{ PApplicationInfo = &VK_ApplicationInfo, SType = StructureType.InstanceCreateInfo };

                WL.Vulkan.CheckResult(__API.CreateInstance(&VK_InstanceCreateInfo, null, out Instance Result_Instance), $"Ошибка в CreateInstance({VK_InstanceCreateInfo})!");
                
                return Result_Instance;
            }catch(Exception e){
                throw new ExceptionWL($"Произошла ошибка при создании Vulkan Instance!\nВерсия Vulkan: {VulkanVersion}\nИнформация об проекте: {ProjectInfo}\nИнформация об ядре: {EngineInfo}", e);
            }finally{
                if(Ptr_ProjectName != IntPtr.Zero){ SilkMarshal.Free(Ptr_ProjectName); }
                if(Ptr_EngineName  != IntPtr.Zero){ SilkMarshal.Free(Ptr_EngineName ); }
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