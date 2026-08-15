using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using WLO;

namespace WL;

public static unsafe class Vulkan{
    public static uint FindMemoryType(Vk API, PhysicalDevice GPU, uint TypeFilter, MemoryPropertyFlags Properties){
        PhysicalDeviceMemoryProperties VK_PDMP;
        API.GetPhysicalDeviceMemoryProperties(GPU, &VK_PDMP);
        for (int i = 0; i < VK_PDMP.MemoryTypeCount; i++) {
            if((TypeFilter & (1u << i)) != 0 && (VK_PDMP.MemoryTypes[i].PropertyFlags & Properties) == Properties){ return (uint)i; }
        }
            
        
        throw new Exception("Подходящий тип памяти не найден!");
        // todo add try/catch
    }
    
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static Bool32 DebugLoggerCallback(DebugUtilsMessageSeverityFlagsEXT Severity, DebugUtilsMessageTypeFlagsEXT Types, DebugUtilsMessengerCallbackDataEXT* CallbackData, void* UserData){
        if(UserData != null){
            GCHandle Self__ = GCHandle.FromIntPtr((IntPtr)UserData);

            if(Self__.Target is WLO.Render.Hardware.Vulkan Self){
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
    
    // ----------------------------------------------------------------------
    #region Создание главное

        public static DebugUtilsMessengerEXT SetupDebugLogger(Vk API, Instance Instance, GCHandle RenderPtr){
            try{
                if(!API.TryGetInstanceExtension(Instance, out ExtDebugUtils DebugUtils)){ throw new ExceptionWL("Не удалось найти расширение ExtDebugUtils!"); }

                DebugUtilsMessengerCreateInfoEXT VK_DUMCIEXT = new DebugUtilsMessengerCreateInfoEXT{ MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt, MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt, PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(&WL.Vulkan.DebugLoggerCallback), PUserData = (void*)GCHandle.ToIntPtr(RenderPtr), SType = StructureType.DebugUtilsMessengerCreateInfoExt };

                DebugUtilsMessengerEXT Result_DebugLogger;
                DebugUtils.CreateDebugUtilsMessenger(Instance, &VK_DUMCIEXT, null, &Result_DebugLogger);

                return Result_DebugLogger;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при создании DebugLogger Vulkan!", e);
            }
        }

    #endregion
    // ----------------------------------------------------------------------
    #region Создание графики

        public static (CommandPool, CommandBuffer, Fence) CreateRenderOther(Vk API, Device Device, uint GPUIndex){
            // CommandPool
            
            CommandPoolCreateInfo VK_CPCI = new CommandPoolCreateInfo{ QueueFamilyIndex = GPUIndex, Flags = CommandPoolCreateFlags.ResetCommandBufferBit, SType = StructureType.CommandPoolCreateInfo };

            CommandPool Result_CommandPool;
            API.CreateCommandPool(Device, &VK_CPCI, null, &Result_CommandPool);

            CommandBufferAllocateInfo VK_CBAI = new CommandBufferAllocateInfo{ CommandPool = Result_CommandPool, Level = CommandBufferLevel.Primary, CommandBufferCount = 1, SType = StructureType.CommandBufferAllocateInfo };

            CommandBuffer Result_CommandBuffer;
            API.AllocateCommandBuffers(Device, &VK_CBAI, &Result_CommandBuffer);
            
            // ----------------------------------------------------------------------
            // Fence
            
            FenceCreateInfo VK_FCI = new FenceCreateInfo{ Flags = FenceCreateFlags.SignaledBit, SType = StructureType.FenceCreateInfo };

            Fence Result_Fence;
            API.CreateFence(Device, &VK_FCI, null, &Result_Fence);

            return (Result_CommandPool, Result_CommandBuffer, Result_Fence);
        }

    #endregion
    // ----------------------------------------------------------------------
    #region Проверки

        public static void CheckResult(Result Received, string ExtraMessage = "*Нет дополнительной информации*", Result Target = Result.Success){
            if(Received != Target){
                throw new ExceptionWL($"Значения Vulkan Result [{Received}] и [{Target}] не равны!\nДополнительно: {ExtraMessage}");
            }
        }
    
        public static void CheckVulkanSDK(Vk API){
            uint LayerCount = 0;
            API.EnumerateInstanceLayerProperties(&LayerCount, null);
            LayerProperties* AvailableLayers = stackalloc LayerProperties[(int)LayerCount];
            API.EnumerateInstanceLayerProperties(&LayerCount, AvailableLayers);

            bool KhronosAvailable = false;
            for(int i = 0; i < LayerCount; i++){
                if(Marshal.PtrToStringAnsi((IntPtr)AvailableLayers[i].LayerName) == "VK_LAYER_KHRONOS_validation"){ KhronosAvailable = true; break; }
            }
            
            if(!KhronosAvailable){
                throw new ExceptionWL("Не найден Vulkan SDK! Для работы DebugLogger нужен Vulkan SDK!\nСкачать: https://vulkan.lunarg.com/sdk/home");
            }
        }

    #endregion
}