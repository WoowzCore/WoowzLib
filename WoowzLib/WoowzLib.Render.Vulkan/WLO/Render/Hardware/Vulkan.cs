using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using WLO.Math;

namespace WLO.Render.Hardware;

public unsafe class Vulkan : WLI_Render.Hardware{
    private Vk                     __VK;
    private Instance               __Instance;
    private DebugUtilsMessengerEXT __DebugMessanger;
    private PhysicalDevice         __PhysicalDevice;
    private Device                 __Device;
    private Queue                  __GraphicsQueue;
    private uint                   __GraphicsQueueFamilyIndex;

    bool __EnableValidationLayers = true;
    
    public void Start(){
        try{
            __VK = Vk.GetApi();
        
            __CreateInstance();
            __SetupDebugMessanger();
            __PickPhysicalDevice();
            __CreateLogicalDevice();
            
            WL.Logger.Debug("todo, Vulkan инициализирован!");
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при запуске рендера Vulkan!\nWLO.Render.Hardware.Vulkan.Start()", e);
        }
    }
    
    // ----------------------------------------------------------------------

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static Bool32 __DebugCallback(
        DebugUtilsMessageSeverityFlagsEXT severity,
        DebugUtilsMessageTypeFlagsEXT types,
        DebugUtilsMessengerCallbackDataEXT* pCallbackData,
        void* pUserData)
    {
        string message = Marshal.PtrToStringAnsi((IntPtr)pCallbackData->PMessage) ?? "Unknown Vulkan Message";
    
        uint level = (uint)WLI.Logger.Type.Debug;
        if (severity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt)) level = (uint)WLI.Logger.Type.Error;
        else if (severity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.WarningBitExt)) level = (uint)WLI.Logger.Type.Warning;

        WL.Logger.Log(level, $"[Vulkan] {message}");
        
        return Vk.False;
    }
    
    private void __SetupDebugMessanger(){
        if(!__EnableValidationLayers){ return; }

        if(!__VK.TryGetInstanceExtension<ExtDebugUtils>(__Instance, out ExtDebugUtils DebugUtils)){
            throw new ExceptionWL($"Не удалось найти расширение ExtDebugUtils! WLO.Render.Hardware.Vulkan.__VK.TryGetInstanceExtension<ExtDebugUtils>({__Instance}, out ExtDebugUtils {DebugUtils}) вернул false!");
        }

        DebugUtilsMessengerCreateInfoEXT CreateInfo = new DebugUtilsMessengerCreateInfoEXT{
            SType = StructureType.DebugUtilsMessengerCreateInfoExt,
            MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
            
            MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                          DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
                          DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
            
            PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(&__DebugCallback)
        };

        fixed(DebugUtilsMessengerEXT* MessagerPtr = &__DebugMessanger){
            DebugUtils.CreateDebugUtilsMessenger(__Instance, &CreateInfo, null, MessagerPtr);
        }
    }
    
    private void __CreateInstance(){
        uint LayerCount = 0;
        __VK.EnumerateInstanceLayerProperties(&LayerCount, null);
        LayerProperties* AvailableLayers = stackalloc LayerProperties[(int)LayerCount];
        __VK.EnumerateInstanceLayerProperties(&LayerCount, AvailableLayers);

        bool KhronosAvailable = false;
        for(int i = 0; i < LayerCount; i++){
            string Name = Marshal.PtrToStringAnsi((IntPtr)AvailableLayers[i].LayerName);
            if(Name == "VK_LAYER_KHRONOS_validation"){
                KhronosAvailable = true;
                break;
            }
        }
        
        if (__EnableValidationLayers && !KhronosAvailable) {
            WL.Logger.Error("[Vulkan] VK_LAYER_KHRONOS_validation не найден в системе. Валидация отключена. (скачайте Vulkan SDK)");
            __EnableValidationLayers = false; 
        }
        
        // ----------------------------------------------------------------------
        
        // todo, брать из WL.Core
        ApplicationInfo AppInfo = new ApplicationInfo{
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*) Marshal.StringToHGlobalAnsi("WL_TEST"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*) Marshal.StringToHGlobalAnsi("WL_TEST"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version12
        };

        InstanceCreateInfo CreateInfo = new InstanceCreateInfo{
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &AppInfo
        };
        
        if(__EnableValidationLayers){
            string[] __ValidationLayers = ["VK_LAYER_KHRONOS_validation"];
            CreateInfo.EnabledLayerCount = (uint)__ValidationLayers.Length;
            CreateInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(__ValidationLayers);
        }

        if(__EnableValidationLayers){
            string[] __Extensions = ["VK_EXT_debug_utils"];
            CreateInfo.EnabledExtensionCount = (uint)__Extensions.Length;
            CreateInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(__Extensions);
        }else{
            CreateInfo.EnabledExtensionCount = 0;
            CreateInfo.PpEnabledExtensionNames = null;
        }

        Result __Result_CreateInstance = __VK.CreateInstance(&CreateInfo, null, out __Instance);
        if(__Result_CreateInstance != Result.Success){
            throw new ExceptionWL($"Произошла ошибка в WLO.Render.Hardware.Vulkan.__VK.CreateInstance({CreateInfo}, null, out {__Instance}), вернуло {__Result_CreateInstance}!");
        }

        SilkMarshal.Free((IntPtr)AppInfo.PApplicationName);
        SilkMarshal.Free((IntPtr)AppInfo.PEngineName);
    }

    private void __PickPhysicalDevice(){
        uint DeviceCount = 0;
        __VK.EnumeratePhysicalDevices(__Instance, &DeviceCount, null);

        if(DeviceCount == 0){ throw new ExceptionWL($"Не найдены видеокарты с поддержкой Vulkan! WLO.Render.Hardware.Vulkan.__VK.EnumeratePhysicalDevices({__Instance}, &DeviceCount, null) вернул 0!"); }

        PhysicalDevice* Devices = stackalloc PhysicalDevice[(int)DeviceCount];
        __VK.EnumeratePhysicalDevices(__Instance, &DeviceCount, Devices);

        for(int i = 0; i < DeviceCount; i++){
            if(__IsDeviceSuitable(Devices[i])){
                __PhysicalDevice = Devices[i];
                break;
            }
        }

        if(__PhysicalDevice.Handle == 0){ throw new ExceptionWL("Не найдена подходящая видеокарта для Vulkan! WLO.Render.Hardware.Vulkan.__IsDeviceSuitable(Devices[i]) все равны false!"); }

        PhysicalDeviceProperties Properties;
        __VK.GetPhysicalDeviceProperties(__PhysicalDevice, &Properties);
        WL.Logger.Debug($"todo, Выбрана видеокарта: {Marshal.PtrToStringAnsi((IntPtr)Properties.DeviceName)}");
    }

    private bool __IsDeviceSuitable(PhysicalDevice Device){
        uint QueueFamilyCount = 0;
        __VK.GetPhysicalDeviceQueueFamilyProperties(Device, &QueueFamilyCount, null);

        QueueFamilyProperties* Families = stackalloc QueueFamilyProperties[(int)QueueFamilyCount];
        __VK.GetPhysicalDeviceQueueFamilyProperties(Device, &QueueFamilyCount, Families);

        for(uint i = 0; i < QueueFamilyCount; i++){
            if(Families[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit)){
                __GraphicsQueueFamilyIndex = i;
                return true;
            }
        }

        return false;
    }

    private void __CreateLogicalDevice(){
        float QueuePriority = 1f;
        DeviceQueueCreateInfo QueueCreateInfo = new DeviceQueueCreateInfo{
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = __GraphicsQueueFamilyIndex,
            QueueCount = 1,
            PQueuePriorities = &QueuePriority
        };

        DeviceCreateInfo DeviceCreateInfo = new DeviceCreateInfo{
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &QueueCreateInfo,
            EnabledExtensionCount = 0,
            PpEnabledLayerNames = null
        };

        fixed(Device* DevicePtr = &__Device){
            Result Result = __VK.CreateDevice(__PhysicalDevice, &DeviceCreateInfo, null, DevicePtr);
            if(Result != Result.Success){ throw new ExceptionWL($"Произошла ошибка при создании Device! WLO.Render.Hardware.Vulkan.__VK.CreateDevice({__PhysicalDevice}, &{DeviceCreateInfo}, null, ...) вернул {Result}!"); }
        }

        fixed(Queue* QueuePtr = &__GraphicsQueue){
            __VK.GetDeviceQueue(__Device, __GraphicsQueueFamilyIndex, 0, QueuePtr);
        }
    }

    public void Clear(Color4B Color){
        throw new NotImplementedException();
    }

    public void DrawFrameBuffer(FrameBuffer Buffer){
        throw new NotImplementedException();
    }
    public Vector2I Viewport{ get; set; }

    public void FrameStart(){
        throw new NotImplementedException();
    }

    public void FrameStop(){
        throw new NotImplementedException();
    }

    public void Stop(){
        if(__Device.Handle != 0){ __VK.DestroyDevice(__Device, null); }
        if(__EnableValidationLayers && __Instance.Handle != 0){
            if(__VK.TryGetInstanceExtension<ExtDebugUtils>(__Instance, out ExtDebugUtils? DebugUtils)){
                DebugUtils.DestroyDebugUtilsMessenger(__Instance, __DebugMessanger, null);
            }
        }

        if(__Instance.Handle != 0){ __VK.DestroyInstance(__Instance, null); }
    }
}