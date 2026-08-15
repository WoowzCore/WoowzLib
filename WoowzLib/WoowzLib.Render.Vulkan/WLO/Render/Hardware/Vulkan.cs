using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
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

            public GCHandle API_Self{ get; private set; }

            public  Instance API_Instance => __Instance;
            private Instance                 __Instance;

            public  PhysicalDevice API_GPU => __GPU;
            private PhysicalDevice            __GPU;

            public  uint API_GPUIndex => __GPUIndex;
            private uint                 __GPUIndex;

            public  Device API_Device => __Device;
            private Device               __Device;

            public  Queue API_Queue => __Queue;
            private Queue              __Queue;
            
            public Version32 API_Version{ get; private set; }
            
            public bool API_HasDebugLogger{ get; private set; }

            public DebugUtilsMessengerEXT API_DebugLogger{ get; private set; }

            #endregion

        #region Логирование

            public WLI.Logger? CurrentLogger = WL.Logger.CurrentLogger;

            public uint LogType_Initialization    = (uint)WLI.Logger.Type.Info;
            public uint LogType_InitDetail        = (uint)WLI.Logger.Type.Info;
            public uint LogType_GPU               = (uint)WLI.Logger.Type.Info;
            public uint LogType_DebugLogger_Info  = (uint)WLI.Logger.Type.Info;
            public uint LogType_DebugLogger_Warn  = (uint)WLI.Logger.Type.Warning;
            public uint LogType_DebugLogger_Error = (uint)WLI.Logger.Type.Error;
            
        #endregion

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
                VulkanVersion = Properties.HasValue && Properties.Value.VulkanVersion.HasValue ? Properties.Value.VulkanVersion.Value : Vk.Version12,
                DebugLogger   = Properties.HasValue && Properties.Value.DebugLogger  .HasValue ? Properties.Value.DebugLogger  .Value : false,
            };

            if(StartImmediately){ Start(); }
        }
        public struct StartParameters{
            public WLI.Logger?  UseThisLogger;
            public ProjectInfo? ProjectInfo;
            public ProjectInfo? EngineInfo;
            public Version32?   VulkanVersion;
            public bool?        DebugLogger;
        }
        private readonly StartParameters __StartParameters;

        /*
         * Запускает Vulkan
         */
        public void Start(){
            try{
                if(IsStarted){ throw new ExceptionWL("Vulkan уже запущен!"); }

                CurrentLogger = __StartParameters.UseThisLogger;

                Log(LogType_Initialization, "Запуск Vulkan...");
                
                API_Version = __StartParameters.VulkanVersion!.Value;
                Log(LogType_InitDetail, $"Версия Vulkan: {API_Version.ToString()}");
                
                __API     = Vk     .GetApi();
                __ShaderC = Shaderc.GetApi();
                Log(LogType_InitDetail, $"API: {__API}, ShaderC: {__ShaderC}");

                API_Self = GCHandle.Alloc(this);
                Log(LogType_InitDetail, $"Ссылка на Vulkan: {API_Self}");
                
                API_HasDebugLogger = __StartParameters.DebugLogger!.Value;
                
                __Instance = __CreateInstance(API_Version, __StartParameters.ProjectInfo!.Value, __StartParameters.EngineInfo!.Value);
                Log(LogType_InitDetail, $"Создан Instance: {__Instance}, DebugLogger нужно создать?: {API_HasDebugLogger}");

                if(API_HasDebugLogger){
                    API_DebugLogger = __SetupDebugLogger();
                    Log(LogType_InitDetail, $"Создан DebugLogger: {API_DebugLogger}");
                }
                
                (__GPU, uint? __GPUIndex__) = __PickGPU();
                if(__GPUIndex__ != null){ __GPUIndex = __GPUIndex__.Value; }
                Log(LogType_InitDetail, $"Выбран GPU: {__GPU}, GPUIndex: {__GPUIndex}");

                PhysicalDeviceProperties VK_PhysicalDeviceProperties;
                __API.GetPhysicalDeviceProperties(__GPU, &VK_PhysicalDeviceProperties);
                Log(LogType_GPU, $"Выбрана видеокарта: {Marshal.PtrToStringAnsi((IntPtr)VK_PhysicalDeviceProperties.DeviceName)}");

                (__Device, __Queue) = __CreateDevice(__GPU, __GPUIndex);
                Log(LogType_InitDetail, $"Создан Device: {__Device}, Queue: {__Queue}");
                
                Log(LogType_Initialization, "Vulkan запущен!");
                
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

                Log(LogType_Initialization, "Остановка Vulkan...");
                
                void Destroy<T>(ref T H, Action<T> Action) where T : unmanaged{ if(Unsafe.As<T, ulong>(ref H) != 0){ Action(H); H = default; } }

                __Queue = default;
                Destroy(ref __Device, H => __API.DestroyDevice(H, null));
                
                __GPUIndex = 0;
                __GPU = default;

                if(API_HasDebugLogger && __Instance.Handle != 0){
                    if(__API.TryGetInstanceExtension<ExtDebugUtils>(__Instance, out ExtDebugUtils? DebugUtils)){
                        DebugUtils.DestroyDebugUtilsMessenger(__Instance, API_DebugLogger, null);
                        API_DebugLogger = default;
                    }
                }
                
                Destroy(ref __Instance, H => __API.DestroyInstance(H, null));
                
                __ShaderC = null!;
                __API     = null!;
                
                Log(LogType_Initialization, "Vulkan остановлен!");
                
                IsStarted = false;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при остановке Vulkan!", e);
            }
        }

    #endregion
    // ----------------------------------------------------------------------
    #region Базовые компоненты

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

                if(API_HasDebugLogger){
                    __CheckVulkanSDK();
                    
                    string[] Layers     = ["VK_LAYER_KHRONOS_validation"];
                    string[] Extensions = ["VK_EXT_debug_utils"         ];

                    VK_InstanceCreateInfo.EnabledLayerCount       = (uint)Layers.Length;
                    VK_InstanceCreateInfo.PpEnabledLayerNames     = (byte**)SilkMarshal.StringArrayToPtr(Layers);
                    VK_InstanceCreateInfo.EnabledExtensionCount   = (uint)Extensions.Length;
                    VK_InstanceCreateInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(Extensions);
                }
                
                WL.Vulkan.CheckResult(__API.CreateInstance(&VK_InstanceCreateInfo, null, out Instance Result_Instance), $"Ошибка в CreateInstance({VK_InstanceCreateInfo})!");

                if(API_HasDebugLogger){
                    SilkMarshal.Free((IntPtr)VK_InstanceCreateInfo.PpEnabledLayerNames    );
                    SilkMarshal.Free((IntPtr)VK_InstanceCreateInfo.PpEnabledExtensionNames);
                }
                
                return Result_Instance;
            }catch(Exception e){
                throw new ExceptionWL($"Произошла ошибка при создании Vulkan Instance!\nВерсия Vulkan: {VulkanVersion}\nИнформация об проекте: {ProjectInfo}\nИнформация об ядре: {EngineInfo}", e);
            }finally{
                if(Ptr_ProjectName != IntPtr.Zero){ SilkMarshal.Free(Ptr_ProjectName); }
                if(Ptr_EngineName  != IntPtr.Zero){ SilkMarshal.Free(Ptr_EngineName ); }
            }
        }

        private void __CheckVulkanSDK(){
            uint LayerCount = 0;
            __API.EnumerateInstanceLayerProperties(&LayerCount, null);
            LayerProperties* AvailableLayers = stackalloc LayerProperties[(int)LayerCount];
            __API.EnumerateInstanceLayerProperties(&LayerCount, AvailableLayers);

            bool KhronosAvailable = false;
            for(int i = 0; i < LayerCount; i++){
                if(Marshal.PtrToStringAnsi((IntPtr)AvailableLayers[i].LayerName) == "VK_LAYER_KHRONOS_validation"){ KhronosAvailable = true; break; }
            }
        
            if(!KhronosAvailable){
                throw new ExceptionWL("Не найден Vulkan SDK! Для работы DebugLogger нужен Vulkan SDK!\nСкачать: https://vulkan.lunarg.com/sdk/home");
            }
        }
        
        private DebugUtilsMessengerEXT __SetupDebugLogger(){
            try{
                if(!__API.TryGetInstanceExtension(__Instance, out ExtDebugUtils DebugUtils)){ throw new ExceptionWL("Не удалось найти расширение ExtDebugUtils!"); }

                DebugUtilsMessengerCreateInfoEXT CreateInfo = new DebugUtilsMessengerCreateInfoEXT{ MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt, MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt, PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(&__DebugLoggerCallback), PUserData = (void*)GCHandle.ToIntPtr(API_Self), SType = StructureType.DebugUtilsMessengerCreateInfoExt };

                DebugUtilsMessengerEXT Result_DebugLogger;
                DebugUtils.CreateDebugUtilsMessenger(__Instance, &CreateInfo, null, &Result_DebugLogger);

                return Result_DebugLogger;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при создании DebugLogger Vulkan!", e);
            }
        }

        private (PhysicalDevice, uint?) __PickGPU(){
            try{
                uint GPUsCount = 0;
                __API.EnumeratePhysicalDevices(__Instance, &GPUsCount, null);

                if(GPUsCount == 0){ throw new ExceptionWL("Не найдены видеокарты с поддержкой Vulkan!"); }

                PhysicalDevice* GPUs = stackalloc PhysicalDevice[(int)GPUsCount];
                __API!.EnumeratePhysicalDevices(__Instance, &GPUsCount, GPUs);

                PhysicalDevice Result_GPU = default;
                uint? Result_GPUIndex = null;
                
                for(int i = 0; i < GPUsCount; i++){
                    uint? GPUIndex = __GPUSuitable(GPUs[i]);
                    
                    if(GPUIndex != null){
                        Result_GPU = GPUs[i];
                        Result_GPUIndex = GPUIndex;
                        break;
                    }
                }

                if(Result_GPU.Handle == 0){ throw new ExceptionWL("Не найдена подходящая видеокарта для Vulkan!"); }
                
                return (Result_GPU, Result_GPUIndex);
            }catch(Exception e){
                throw new ExceptionWL($"Не получилось получить видеокарту Vulkan!", e);
            }
        }

        private uint? __GPUSuitable(PhysicalDevice GPU){
            uint QueueFamilyCount = 0;
            __API.GetPhysicalDeviceQueueFamilyProperties(GPU, &QueueFamilyCount, null);

            QueueFamilyProperties* Families = stackalloc QueueFamilyProperties[(int)QueueFamilyCount];
            __API.GetPhysicalDeviceQueueFamilyProperties(GPU, &QueueFamilyCount, Families);

            for(uint i = 0; i < QueueFamilyCount; i++){ if(Families[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit)){ return i; } }

            return null;
        }

        private (Device, Queue) __CreateDevice(PhysicalDevice GPU, uint GPUIndex){
            try{
                float QueuePriority = 1f;
                DeviceQueueCreateInfo QueueCreateInfo = new DeviceQueueCreateInfo{ QueueFamilyIndex = GPUIndex, QueueCount = 1, PQueuePriorities = &QueuePriority, SType = StructureType.DeviceQueueCreateInfo };

                DeviceCreateInfo DeviceCreateInfo = new DeviceCreateInfo{ QueueCreateInfoCount = 1, PQueueCreateInfos = &QueueCreateInfo, EnabledExtensionCount = 0, PpEnabledLayerNames = null, SType = StructureType.DeviceCreateInfo };

                Device Result_Device;
                WL.Vulkan.CheckResult(__API.CreateDevice(GPU, &DeviceCreateInfo, null, &Result_Device), "Ошибка при создании Device!");
                
                Queue Result_Queue;
                __API.GetDeviceQueue(Result_Device, GPUIndex, 0, &Result_Queue);

                return (Result_Device, Result_Queue);
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при создании Vulkan Device & Queue!", e);
            }
        }
        
    #endregion
    // ----------------------------------------------------------------------
    #region Остальное

        private void Log(uint Type, object Message){
            CurrentLogger?.PrefixPush("VK");
            CurrentLogger?.Log(Type, Message);
            CurrentLogger?.PrefixPop();
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static Bool32 __DebugLoggerCallback(DebugUtilsMessageSeverityFlagsEXT Severity, DebugUtilsMessageTypeFlagsEXT Types, DebugUtilsMessengerCallbackDataEXT* CallbackData, void* UserData){
            if(UserData != null){
                GCHandle Self__ = GCHandle.FromIntPtr((IntPtr)UserData);

                if(Self__.Target is Vulkan Self){
                    uint Level = Self.LogType_DebugLogger_Info;
                    if(Severity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.WarningBitExt)){
                        Level = Self.LogType_DebugLogger_Warn;
                    }else if(Severity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt)){
                        Level = Self.LogType_DebugLogger_Error;
                    }
                    
                    Self.Log(Level, Marshal.PtrToStringAnsi((IntPtr)CallbackData -> PMessage) ?? "Неизвестное Vulkan сообщение!");
                }
            }
            
            return Vk.False;
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