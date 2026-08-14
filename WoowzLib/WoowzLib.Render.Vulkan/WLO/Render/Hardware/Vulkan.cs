using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using WLO.Math;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace WLO.Render.Hardware;

public unsafe class Vulkan : WLI_Render.Hardware{
    public Vk                     __VK;
    public Instance               __Instance;
    private DebugUtilsMessengerEXT __DebugMessanger;
    public PhysicalDevice         __PhysicalDevice;
    public Device                 __Device;
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

            if(Viewport.W == 0){ Viewport = new Vector2I(800, 600); } // todo
            
            __CreateCommandPool();
            __CreateSyncObjects();
            
            __CreateResources(Viewport);
            __CreateImageView();
            __CreateRenderPass();
            __CreateFramebuffer();
            __CreateDescriptorPool();
            __CreateStagingBuffer(Viewport);
            
            WL.Logger.Debug("todo, Vulkan инициализирован!");
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при запуске рендера Vulkan!\nWLO.Render.Hardware.Vulkan.Start()", e);
        }
    }
    
    public void Stop(){
        __VK.DeviceWaitIdle(__Device);
        
        if (__DescriptorPool.Handle != 0) __VK.DestroyDescriptorPool(__Device, __DescriptorPool, null);
        if (__Framebuffer.Handle != 0) __VK.DestroyFramebuffer(__Device, __Framebuffer, null);
        if (__MainRenderPass.Handle != 0) __VK.DestroyRenderPass(__Device, __MainRenderPass, null);
        if (__RenderImageView.Handle != 0) __VK.DestroyImageView(__Device, __RenderImageView, null);
        
        if(__RenderFence.Handle != 0){ __VK.DestroyFence(__Device, __RenderFence, null); }
        if(__CommandPool.Handle != 0){ __VK.DestroyCommandPool(__Device, __CommandPool, null); }
        
        if(__StadingBuffer.Handle != 0){ __VK.DestroyBuffer(__Device, __StadingBuffer, null); }
        if(__StadingMemory.Handle != 0){
            __VK.UnmapMemory(__Device, __StadingMemory);
            __VK.FreeMemory(__Device, __StadingMemory, null);
        }
        
        if(__RenderImage.Handle != 0){ __VK.DestroyImage(__Device, __RenderImage, null); }
        if(__RenderImageMemory.Handle != 0){ __VK.FreeMemory(__Device, __RenderImageMemory, null); }
        
        if(__Device.Handle != 0){ __VK.DestroyDevice(__Device, null); }
        if(__EnableValidationLayers && __Instance.Handle != 0){
            if(__VK.TryGetInstanceExtension<ExtDebugUtils>(__Instance, out ExtDebugUtils? DebugUtils)){
                DebugUtils.DestroyDebugUtilsMessenger(__Instance, __DebugMessanger, null);
            }
        }

        if(__Instance.Handle != 0){ __VK.DestroyInstance(__Instance, null); }
    }
    
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
    
    // ----------------------------------------------------------------------
    
    private Image         __RenderImage;
    private DeviceMemory  __RenderImageMemory;
    private ImageView     __RenderImageView;
    private Buffer        __StadingBuffer;
    private DeviceMemory  __StadingMemory;
    private void*         __MappedPtr;
    private CommandPool   __CommandPool;
    private CommandBuffer __CommandBuffer;
    private Fence         __RenderFence;
    
    public RenderPass     __MainRenderPass;
    public DescriptorPool __DescriptorPool;
    private Framebuffer    __Framebuffer;
    
    private void __CreateResources(Vector2I Size){
        ImageCreateInfo ImageInfo = new ImageCreateInfo{
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent = new Extent3D((uint)Size.W, (uint)Size.H, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Format = Format.B8G8R8A8Unorm,
            Tiling = ImageTiling.Optimal,
            InitialLayout = ImageLayout.Undefined,
            Usage = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit,
            Samples = SampleCountFlags.Count1Bit,
            SharingMode = SharingMode.Exclusive
        };

        fixed(Image* ImagePtr = &__RenderImage){
            __VK.CreateImage(__Device, &ImageInfo, null, ImagePtr);
        }

        MemoryRequirements MemoryRequirements;
        __VK.GetImageMemoryRequirements(__Device, __RenderImage, &MemoryRequirements);

        MemoryAllocateInfo AllocateInfo = new MemoryAllocateInfo{
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = MemoryRequirements.Size,
            MemoryTypeIndex = __FindMemoryType(MemoryRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
        };

        fixed(DeviceMemory* MemoryPtr = &__RenderImageMemory){
            __VK.AllocateMemory(__Device, &AllocateInfo, null, MemoryPtr);
        }
        __VK.BindImageMemory(__Device, __RenderImage, __RenderImageMemory, 0);
    }

    public uint __FindMemoryType(uint TypeFilter, MemoryPropertyFlags Properties){
        PhysicalDeviceMemoryProperties MemoryProperties;
        __VK.GetPhysicalDeviceMemoryProperties(__PhysicalDevice, &MemoryProperties);

        for(uint i = 0; i < MemoryProperties.MemoryTypeCount; i++){
            if((TypeFilter & (1u << (int)i)) != 0 && (MemoryProperties.MemoryTypes[(int)i].PropertyFlags & Properties) == Properties){
                return i;
            }
        }

        throw new ExceptionWL("Не удалось найти подходящий тип памяти! todo");
    }

    private void __CreateStagingBuffer(Vector2I Size){
        uint BufferSize = (uint)(Size.W * Size.H * 4);

        BufferCreateInfo BufferInfo = new BufferCreateInfo{
            SType = StructureType.BufferCreateInfo,
            Size = BufferSize,
            Usage = BufferUsageFlags.TransferDstBit,
            SharingMode = SharingMode.Exclusive
        };

        fixed(Buffer* BufferPtr = &__StadingBuffer){
            __VK.CreateBuffer(__Device, &BufferInfo, null, BufferPtr);
        }

        MemoryRequirements MemoryRequirements;
        __VK.GetBufferMemoryRequirements(__Device, __StadingBuffer, &MemoryRequirements);

        MemoryAllocateInfo AllocateInfo = new MemoryAllocateInfo{
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = MemoryRequirements.Size,
            MemoryTypeIndex = __FindMemoryType(MemoryRequirements.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
        };

        fixed(DeviceMemory* MemoryPtr = &__StadingMemory){
            __VK.AllocateMemory(__Device, &AllocateInfo, null, MemoryPtr);
        }

        __VK.BindBufferMemory(__Device, __StadingBuffer, __StadingMemory, 0);

        void* Ptr;
        __VK.MapMemory(__Device, __StadingMemory, 0, BufferSize, 0, &Ptr);
        __MappedPtr = Ptr;
    }

    private void __CreateSyncObjects(){
        FenceCreateInfo Info = new FenceCreateInfo{
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        fixed(Fence* Ptr = &__RenderFence){
            __VK.CreateFence(__Device, &Info, null, Ptr);
        }
    }

    private void __CreateCommandPool(){
        CommandPoolCreateInfo Info = new CommandPoolCreateInfo{
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = __GraphicsQueueFamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        fixed(CommandPool* Ptr = &__CommandPool){
            __VK.CreateCommandPool(__Device, &Info, null, Ptr);
        }

        CommandBufferAllocateInfo AllocateInfo = new CommandBufferAllocateInfo{
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = __CommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        fixed(CommandBuffer* Ptr = &__CommandBuffer){
            __VK.AllocateCommandBuffers(__Device, &AllocateInfo, Ptr);
        }
    }

    private void __TransitionImageLayout(Image Image, ImageLayout Old, ImageLayout New){
        ImageMemoryBarrier Barrier = new ImageMemoryBarrier{
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = Old,
            NewLayout = New,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = Image,
            SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1)
        };

        PipelineStageFlags SrcStage;
        PipelineStageFlags DstStage;

        switch(Old){
            case ImageLayout.Undefined when New == ImageLayout.TransferDstOptimal:
                Barrier.SrcAccessMask = 0;
                Barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                SrcStage = PipelineStageFlags.TopOfPipeBit;
                DstStage = PipelineStageFlags.TransferBit;
                break;
            case ImageLayout.TransferDstOptimal when New == ImageLayout.TransferSrcOptimal:
                Barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                Barrier.DstAccessMask = AccessFlags.TransferReadBit;
                SrcStage = PipelineStageFlags.TransferBit;
                DstStage = PipelineStageFlags.TransferBit;
                break;
            case ImageLayout.TransferSrcOptimal when New == ImageLayout.TransferDstOptimal:
                Barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                Barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                SrcStage = PipelineStageFlags.TransferBit;
                DstStage = PipelineStageFlags.TransferBit;
                break;
            default:
                Barrier.SrcAccessMask = AccessFlags.MemoryReadBit | AccessFlags.MemoryWriteBit;
                Barrier.DstAccessMask = AccessFlags.MemoryReadBit | AccessFlags.MemoryWriteBit;
                SrcStage = PipelineStageFlags.AllCommandsBit;
                DstStage = PipelineStageFlags.AllCommandsBit;
                break;
        }
        
        __VK.CmdPipelineBarrier(__CommandBuffer, SrcStage, DstStage, 0, 0, null, 0, null, 1, &Barrier);
    }

    private void __CreateRenderPass(){
        AttachmentDescription ColorAttachment = new AttachmentDescription{
            Format = Format.B8G8R8A8Unorm,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.TransferSrcOptimal
        };

        AttachmentReference ColorAttachmentReference = new AttachmentReference{
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal
        };

        SubpassDescription Subpass = new SubpassDescription{
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &ColorAttachmentReference
        };

        RenderPassCreateInfo RenderPassInfo = new RenderPassCreateInfo{
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &ColorAttachment,
            SubpassCount = 1,
            PSubpasses = &Subpass
        };

        fixed(RenderPass* Ptr = &__MainRenderPass){
            __VK.CreateRenderPass(__Device, &RenderPassInfo, null, Ptr);
        }
    }

    private void __CreateImageView(){
        ImageViewCreateInfo ViewInfo = new ImageViewCreateInfo{
            SType = StructureType.ImageViewCreateInfo,
            Image = __RenderImage,
            ViewType = ImageViewType.Type2D,
            Format = Format.B8G8R8A8Unorm,
            SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1)
        };

        fixed(ImageView* Ptr = &__RenderImageView){
            __VK.CreateImageView(__Device, &ViewInfo, null, Ptr);
        }
    }

    private void __CreateFramebuffer(){
        fixed(ImageView* Attachment = &__RenderImageView){
            FramebufferCreateInfo FramebufferInfo = new FramebufferCreateInfo{
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = __MainRenderPass,
                AttachmentCount = 1,
                PAttachments = Attachment,
                Width = (uint)Viewport.W,
                Height = (uint)Viewport.H,
                Layers = 1
            };

            fixed(Framebuffer* Ptr = &__Framebuffer){
                __VK.CreateFramebuffer(__Device, &FramebufferInfo, null, Ptr);
            }
        }
    }

    private void __CreateDescriptorPool(){
        DescriptorPoolSize[] PoolSizes = [
            new DescriptorPoolSize(DescriptorType.CombinedImageSampler, 1000)
        ];

        fixed(DescriptorPoolSize* Sizes = PoolSizes){
            DescriptorPoolCreateInfo PoolInfo = new DescriptorPoolCreateInfo{
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)PoolSizes.Length,
                PPoolSizes = Sizes,
                MaxSets = 1000,
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit
            };

            fixed(DescriptorPool* Ptr = &__DescriptorPool){
                __VK.CreateDescriptorPool(__Device, &PoolInfo, null, Ptr);
            }
        }
    }
    
    // ----------------------------------------------------------------------

    public Vector2I Viewport{ get; set; }

    public void Clear(Color4B Color){
        throw new NotImplementedException();
    }
    
    public void FrameStart(){
        __VK.WaitForFences(__Device, 1, ref __RenderFence, true, ulong.MaxValue);
        __VK.ResetFences(__Device, 1, ref __RenderFence);

        CommandBufferBeginInfo BeginInfo = new CommandBufferBeginInfo{
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        __VK.BeginCommandBuffer(__CommandBuffer, &BeginInfo);
    }

    public void FrameStop(){
       BufferImageCopy Region = new BufferImageCopy{
           BufferOffset = 0,
           BufferRowLength = 0,
           BufferImageHeight = 0,
           ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
           ImageOffset = new Offset3D(0, 0, 0),
           ImageExtent = new Extent3D((uint)Viewport.W, (uint)Viewport.H, 1)
       };
       __VK.CmdCopyImageToBuffer(__CommandBuffer, __RenderImage, ImageLayout.TransferSrcOptimal, __StadingBuffer, 1, &Region);

       __VK.EndCommandBuffer(__CommandBuffer);

       fixed(CommandBuffer* CommandBufferPtr = &__CommandBuffer){
           SubmitInfo SubmitInfo = new SubmitInfo{
               SType = StructureType.SubmitInfo,
               CommandBufferCount = 1,
               PCommandBuffers = CommandBufferPtr
           };

           __VK.QueueSubmit(__GraphicsQueue, 1, &SubmitInfo, __RenderFence);
       }
    }
    
    public void RENDER_BEGIN(Color4B Color){
        ClearValue ClearColor = new ClearValue{
            Color = new ClearColorValue{
                Float32_0 = Color.R / 255f,
                Float32_1 = Color.G / 255f,
                Float32_2 = Color.B / 255f,
                Float32_3 = Color.A / 255f
            }
        };

        RenderPassBeginInfo RenderPassInfo = new RenderPassBeginInfo{
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = __MainRenderPass,
            Framebuffer = __Framebuffer,
            RenderArea = new Rect2D(new Offset2D(0, 0), new Extent2D((uint)Viewport.W, (uint)Viewport.H)),
            ClearValueCount = 1,
            PClearValues = &ClearColor
        };
        
        __VK.CmdBeginRenderPass(__CommandBuffer, &RenderPassInfo, SubpassContents.Inline);
    }

    public void RENDER_END(){
        __VK.CmdEndRenderPass(__CommandBuffer);
    }
    
    public void DrawFrameBuffer(FrameBuffer Buffer){
        __VK.WaitForFences(__Device, 1, ref __RenderFence, true, ulong.MaxValue);

        fixed(Color4B* Dst = Buffer.Pixels){
            System.Buffer.MemoryCopy(__MappedPtr, Dst, Buffer.Pixels.Length * 4, Buffer.Pixels.Length * 4);
        }
    }
}