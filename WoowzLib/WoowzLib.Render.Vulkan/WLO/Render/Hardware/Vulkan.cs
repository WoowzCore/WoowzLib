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
    #region [ КОРНЕВЫЕ ОБЪЕКТЫ ]

        /**
        * API Vulkan
        */
        private Vk? __VK;

        /**
         * Экземпляр приложения (ссылка на приложение использующее Vulkan)
         */
        public Instance Instance => __Instance;
        private Instance __Instance;

        /**
         * Используемая видеокарта
         */
        public PhysicalDevice PhysicalDevice => __PhysicalDevice;
        private PhysicalDevice __PhysicalDevice;

        /**
         * Связь между видеокартой и Vulkan
         */
        public Device Device => __Device;
        private Device __Device;

        /**
         * Индекс семейства очередей
         * Индекс используемой видеокартой в данный момент группы, к примеру группы которая рисует треугольники, или копирует память, или делающая вычисления и т.д
         */
        private uint __GraphicsQueueFamilyIndex;
    
        /**
         * Очередь команд
         * Содержит в себе список команд для видеокарты
         */
        private Queue __GraphicsQueue;
        
        
        private DebugUtilsMessengerEXT __DebugMessanger;
        bool                           __EnableValidationLayers = true;
        
    #endregion
    
    #region [ РЕСУРСЫ КАДРА ]

        /**
         * Сырой кусок видеопамяти, где лежат пиксели
         * <seealso cref="__RenderImageView"/>
         */
        private Image __RenderImage;
        
        /**
         * Просмотр RenderImage, даёт понять как правильно смотреть
         * <seealso cref="__RenderImage"/>
         */
        private ImageView __RenderImageView;

        /**
         * Указывает куда рисовать
         */
        private Framebuffer __Framebuffer;
        
        /**
         * Описывает структуру кадра
         */
        public RenderPass RenderPass => __RenderPass;
        private RenderPass __RenderPass;
        
        private DeviceMemory   __RenderImageMemory;
        public  DescriptorPool __DescriptorPool;
        
    #endregion

    #region [ КОМАНДЫ И СИНХРОНИЗАЦИЯ ]

        /**
         * Пул команд, для оптимизации (берёшь команду, и освобождаешь сюда же)
         */
        private CommandPool __CommandPool;
        
        /**
         * Запись команд перед записью в очередь
         */
        private CommandBuffer __CommandBuffer;
        
        /**
         * Синхронизирует GPU и CPU
         */
        private Fence __RenderFence;
        
        /**
         * Общая память между GPU и CPU
         */
        private Buffer __StagingBuffer;

        
        private DeviceMemory __StagingMemory;
        private void*        __MappedPtr;
        
    #endregion

    public Vector2I Viewport{ get; set; }
    public bool IsStarted => __VK != null;

    /**
     * Vulkan находится в режиме рисования?
     */
    public bool IsRenderState => __IsRenderState;
    private bool __IsRenderState = false;
    
    /**
    * Проверяет, API Vulkan инициализирован?
    */
    public void CheckVulkan(bool? IsRenderState = null){ if(!IsStarted){ throw new ExceptionWL("Vulkan не инициализирован!"); } if(IsRenderState.HasValue && IsRenderState.Value != this.IsRenderState){ throw new ExceptionWL($"Невозможно выполнить код, потому-что текущий режим Vulkan [{(this.IsRenderState ? "Рисования" : "НЕ Рисования")}] не подходит! Нужен режим [{(IsRenderState.Value ? "Рисования" : "НЕ Рисования")}]"); } }
    
    public void Start(){
        try{
            if(IsStarted){ throw new ExceptionWL("Vulkan уже был инициализирован!"); }

            __VK = Vk.GetApi();
        
            __CreateInstance();
            __SetupDebugMessanger();
            __PickPhysicalDevice();
            __CreateLogicalDevice();

            if(Viewport.W == 0){ Viewport = new Vector2I(800, 600); } // todo
            
            __CreateCommandPool();
            __CreateSyncObjects();

            Format PixelsFormat = Format.B8G8R8A8Unorm;
            
            InternalCreateImage(Viewport, PixelsFormat, ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferSrcBit, out __RenderImage, out __RenderImageMemory);
            
            __InitRenderPipeline();
            __CreateDescriptorPool();

            uint BufferSize = (uint)(Viewport.W * Viewport.H * 4);
            InternalCreateBuffer(BufferSize, BufferUsageFlags.TransferDstBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, out __StagingBuffer, out __StagingMemory);

            void* Ptr;
            __VK.MapMemory(__Device, __StagingMemory, 0, BufferSize, 0, &Ptr);
            __MappedPtr = Ptr;
            
            WL.Logger.Debug("todo, Vulkan инициализирован!");
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при запуске рендера Vulkan!\nWLO.Render.Hardware.Vulkan.Start()", e);
        }
    }
    
    public void Stop(){
        if(!IsStarted){ return; }

        __VK.DeviceWaitIdle(__Device);
        
        if (__DescriptorPool.Handle != 0) __VK.DestroyDescriptorPool(__Device, __DescriptorPool, null);
        if (__Framebuffer.Handle != 0) __VK.DestroyFramebuffer(__Device, __Framebuffer, null);
        if (__RenderPass.Handle != 0) __VK.DestroyRenderPass(__Device, __RenderPass, null);
        if (__RenderImageView.Handle != 0) __VK.DestroyImageView(__Device, __RenderImageView, null);
        
        if(__RenderFence.Handle != 0){ __VK.DestroyFence(__Device, __RenderFence, null); }
        if(__CommandPool.Handle != 0){ __VK.DestroyCommandPool(__Device, __CommandPool, null); }
        
        if(__StagingBuffer.Handle != 0){ __VK.DestroyBuffer(__Device, __StagingBuffer, null); }
        if(__StagingMemory.Handle != 0){
            __VK.UnmapMemory(__Device, __StagingMemory);
            __VK.FreeMemory(__Device, __StagingMemory, null);
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
    
    // ----------------------------------------------------------------------

    /**
     * Выделение памяти
     */
    public void AllocateMemory(MemoryRequirements Requirements, MemoryPropertyFlags Properties, out DeviceMemory Memory){
        MemoryAllocateInfo Allocate = new MemoryAllocateInfo{ SType = StructureType.MemoryAllocateInfo, AllocationSize = Requirements.Size, MemoryTypeIndex = FindMemoryType(Requirements.MemoryTypeBits, Properties) };
        fixed(DeviceMemory* Ptr = &Memory){ __VK.AllocateMemory(__Device, &Allocate, null, Ptr); }
    }
    
    /**
     * Универсальный метод создания буфера с выделением памяти
     */
    public void InternalCreateBuffer(uint Size, BufferUsageFlags Usage, MemoryPropertyFlags Properties, out Buffer Buffer, out DeviceMemory Memory){
        BufferCreateInfo Info = new BufferCreateInfo{ SType = StructureType.BufferCreateInfo, Size = Size, Usage = Usage, SharingMode = SharingMode.Exclusive };
        fixed(Buffer* Ptr = &Buffer){ __VK.CreateBuffer(__Device, &Info, null, Ptr); }

        MemoryRequirements Requirements;
        __VK.GetBufferMemoryRequirements(__Device, Buffer, &Requirements);

        AllocateMemory(Requirements, Properties, out Memory);
        
        __VK.BindBufferMemory(__Device, Buffer, Memory, 0);
    }

    /**
     * Универсальный метод создания картинки с выделением памяти
     */
    public void InternalCreateImage(Vector2I Size, Format Format, ImageUsageFlags Usage, out Image Image, out DeviceMemory Memory){
        ImageCreateInfo Info = new ImageCreateInfo{ SType = StructureType.ImageCreateInfo, ImageType = ImageType.Type2D, Extent = new Extent3D((uint)Size.W, (uint)Size.H, 1), MipLevels = 1, ArrayLayers = 1, Format = Format, Tiling = ImageTiling.Optimal, InitialLayout = ImageLayout.Undefined, Usage = Usage, Samples = SampleCountFlags.Count1Bit, SharingMode = SharingMode.Exclusive };
        fixed(Image* Ptr = &Image){ __VK.CreateImage(__Device, &Info, null, Ptr); }

        MemoryRequirements Requirements;
        __VK.GetImageMemoryRequirements(__Device, Image, &Requirements);

        AllocateMemory(Requirements, MemoryPropertyFlags.DeviceLocalBit, out Memory);

        __VK.BindImageMemory(__Device, Image, Memory, 0);
    }
    
    /**
     * Находит подходящий индекс типа памяти видеокарты
     */
    public uint FindMemoryType(uint TypeFilter, MemoryPropertyFlags Properties){
        PhysicalDeviceMemoryProperties MemoryProperties;
        __VK.GetPhysicalDeviceMemoryProperties(__PhysicalDevice, &MemoryProperties);

        for(uint i = 0; i < MemoryProperties.MemoryTypeCount; i++){
            if((TypeFilter & (1u << (int)i)) != 0 && (MemoryProperties.MemoryTypes[(int)i].PropertyFlags & Properties) == Properties){
                return i;
            }
        }

        throw new ExceptionWL("Не удалось найти подходящий тип памяти! todo");
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
        CheckVulkan();
        
        if(!__EnableValidationLayers){ return; }

        if(!__VK.TryGetInstanceExtension<ExtDebugUtils>(__Instance, out ExtDebugUtils DebugUtils)){
            throw new ExceptionWL($"Не удалось найти расширение ExtDebugUtils! WLO.Render.Hardware.Vulkan.__VK.TryGetInstanceExtension<ExtDebugUtils>({__Instance}, out ExtDebugUtils {DebugUtils}) вернул false!");
        }

        DebugUtilsMessengerCreateInfoEXT CreateInfo = new DebugUtilsMessengerCreateInfoEXT{ SType = StructureType.DebugUtilsMessengerCreateInfoExt, MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt, MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt, PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(&__DebugCallback) };

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
        ApplicationInfo AppInfo = new ApplicationInfo{ SType = StructureType.ApplicationInfo, PApplicationName = (byte*) Marshal.StringToHGlobalAnsi("WL_TEST"), ApplicationVersion = new Version32(1, 0, 0), PEngineName = (byte*) Marshal.StringToHGlobalAnsi("WL_TEST"), EngineVersion = new Version32(1, 0, 0), ApiVersion = Vk.Version12 };

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
        DeviceQueueCreateInfo QueueCreateInfo = new DeviceQueueCreateInfo{ SType = StructureType.DeviceQueueCreateInfo, QueueFamilyIndex = __GraphicsQueueFamilyIndex, QueueCount = 1, PQueuePriorities = &QueuePriority };

        DeviceCreateInfo DeviceCreateInfo = new DeviceCreateInfo{ SType = StructureType.DeviceCreateInfo, QueueCreateInfoCount = 1, PQueueCreateInfos = &QueueCreateInfo, EnabledExtensionCount = 0, PpEnabledLayerNames = null };

        fixed(Device* DevicePtr = &__Device){
            Result Result = __VK.CreateDevice(__PhysicalDevice, &DeviceCreateInfo, null, DevicePtr);
            if(Result != Result.Success){ throw new ExceptionWL($"Произошла ошибка при создании Device! WLO.Render.Hardware.Vulkan.__VK.CreateDevice({__PhysicalDevice}, &{DeviceCreateInfo}, null, ...) вернул {Result}!"); }
        }

        fixed(Queue* QueuePtr = &__GraphicsQueue){
            __VK.GetDeviceQueue(__Device, __GraphicsQueueFamilyIndex, 0, QueuePtr);
        }
    }
    
    private void __CreateSyncObjects(){
        FenceCreateInfo Info = new FenceCreateInfo{ SType = StructureType.FenceCreateInfo, Flags = FenceCreateFlags.SignaledBit };

        fixed(Fence* Ptr = &__RenderFence){
            __VK.CreateFence(__Device, &Info, null, Ptr);
        }
    }

    private void __CreateCommandPool(){
        CommandPoolCreateInfo Info = new CommandPoolCreateInfo{ SType = StructureType.CommandPoolCreateInfo, QueueFamilyIndex = __GraphicsQueueFamilyIndex, Flags = CommandPoolCreateFlags.ResetCommandBufferBit };

        fixed(CommandPool* Ptr = &__CommandPool){
            __VK.CreateCommandPool(__Device, &Info, null, Ptr);
        }

        CommandBufferAllocateInfo AllocateInfo = new CommandBufferAllocateInfo{ SType = StructureType.CommandBufferAllocateInfo, CommandPool = __CommandPool, Level = CommandBufferLevel.Primary, CommandBufferCount = 1 };

        fixed(CommandBuffer* Ptr = &__CommandBuffer){
            __VK.AllocateCommandBuffers(__Device, &AllocateInfo, Ptr);
        }
    }

    private void __InitRenderPipeline(){
        // ImageView
        
        ImageViewCreateInfo ViewInfo = new ImageViewCreateInfo{ SType = StructureType.ImageViewCreateInfo, Image = __RenderImage, ViewType = ImageViewType.Type2D, Format = Format.B8G8R8A8Unorm, SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1) };

        fixed(ImageView* Ptr = &__RenderImageView){
            __VK.CreateImageView(__Device, &ViewInfo, null, Ptr);
        }
        
        // ----------------------------------------------------------------------
        // RenderPass
        
        AttachmentDescription ColorAttachment          = new AttachmentDescription{ Format = Format.B8G8R8A8Unorm, Samples = SampleCountFlags.Count1Bit, LoadOp = AttachmentLoadOp.DontCare, StoreOp = AttachmentStoreOp.Store, StencilLoadOp = AttachmentLoadOp.DontCare, StencilStoreOp = AttachmentStoreOp.DontCare, InitialLayout = ImageLayout.Undefined, FinalLayout = ImageLayout.TransferSrcOptimal };
        AttachmentReference   ColorAttachmentReference = new AttachmentReference{ Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal };
        SubpassDescription    Subpass                  = new SubpassDescription{ PipelineBindPoint = PipelineBindPoint.Graphics, ColorAttachmentCount = 1, PColorAttachments = &ColorAttachmentReference };
        RenderPassCreateInfo  RenderPassInfo           = new RenderPassCreateInfo{ SType = StructureType.RenderPassCreateInfo, AttachmentCount = 1, PAttachments = &ColorAttachment, SubpassCount = 1, PSubpasses = &Subpass };

        fixed(RenderPass* Ptr = &__RenderPass){
            __VK.CreateRenderPass(__Device, &RenderPassInfo, null, Ptr);
        }
        
        // ----------------------------------------------------------------------
        // Framebuffer
        
        fixed(ImageView* Attachment = &__RenderImageView){
            FramebufferCreateInfo FramebufferInfo = new FramebufferCreateInfo{ SType = StructureType.FramebufferCreateInfo, RenderPass = __RenderPass, AttachmentCount = 1, PAttachments = Attachment, Width = (uint)Viewport.W, Height = (uint)Viewport.H, Layers = 1 };

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
            DescriptorPoolCreateInfo PoolInfo = new DescriptorPoolCreateInfo{ SType = StructureType.DescriptorPoolCreateInfo, PoolSizeCount = (uint)PoolSizes.Length, PPoolSizes = Sizes, MaxSets = 1000, Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit };

            fixed(DescriptorPool* Ptr = &__DescriptorPool){
                __VK.CreateDescriptorPool(__Device, &PoolInfo, null, Ptr);
            }
        }
    }
    
    // ----------------------------------------------------------------------

    public void Clear(Color4B Color){
        CheckVulkan(true);

        ClearAttachment ClearAttachment = new ClearAttachment{ AspectMask = ImageAspectFlags.ColorBit, ColorAttachment = 0, ClearValue = new ClearValue{ Color = new ClearColorValue{ Float32_0 = Color.R / 255f, Float32_1 = Color.G / 255f, Float32_2 = Color.B / 255f, Float32_3 = Color.A / 255f } } };

        ClearRect ClearRect = new ClearRect{ Rect = new Rect2D(new Offset2D(0, 0), new Extent2D((uint)Viewport.W, (uint)Viewport.H)), BaseArrayLayer = 0, LayerCount = 1 };
        
        __VK.CmdClearAttachments(__CommandBuffer, 1, &ClearAttachment, 1, &ClearRect);
    }
    
    public void FrameStart(){
        CheckVulkan(false);
        
        __VK.WaitForFences(__Device, 1, ref __RenderFence, true, ulong.MaxValue);
        __VK.ResetFences(__Device, 1, ref __RenderFence);

        CommandBufferBeginInfo BeginInfo = new CommandBufferBeginInfo{ SType = StructureType.CommandBufferBeginInfo, Flags = CommandBufferUsageFlags.OneTimeSubmitBit };

        __VK.BeginCommandBuffer(__CommandBuffer, &BeginInfo);
        
        RenderPassBeginInfo RenderPassInfo = new RenderPassBeginInfo{ SType = StructureType.RenderPassBeginInfo, RenderPass = __RenderPass, Framebuffer = __Framebuffer, RenderArea = new Rect2D(new Offset2D(0, 0), new Extent2D((uint)Viewport.W, (uint)Viewport.H)), ClearValueCount = 0, PClearValues = null };
        
        __VK.CmdBeginRenderPass(__CommandBuffer, &RenderPassInfo, SubpassContents.Inline);
        
        __IsRenderState = true;
    }

    public void FrameStop(){
        CheckVulkan(true);
        __IsRenderState = false;
        
        __VK.CmdEndRenderPass(__CommandBuffer);
        
       BufferImageCopy Region = new BufferImageCopy{ BufferOffset = 0, BufferRowLength = 0, BufferImageHeight = 0, ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1), ImageOffset = new Offset3D(0, 0, 0), ImageExtent = new Extent3D((uint)Viewport.W, (uint)Viewport.H, 1) };
       __VK.CmdCopyImageToBuffer(__CommandBuffer, __RenderImage, ImageLayout.TransferSrcOptimal, __StagingBuffer, 1, &Region);

       __VK.EndCommandBuffer(__CommandBuffer);

       fixed(CommandBuffer* CommandBufferPtr = &__CommandBuffer){
           SubmitInfo SubmitInfo = new SubmitInfo{ SType = StructureType.SubmitInfo, CommandBufferCount = 1, PCommandBuffers = CommandBufferPtr };

           __VK.QueueSubmit(__GraphicsQueue, 1, &SubmitInfo, __RenderFence);
       }
    }
    
    public void DrawFrameBuffer(FrameBuffer Buffer){
        __VK.WaitForFences(__Device, 1, ref __RenderFence, true, ulong.MaxValue);

        fixed(Color4B* Dst = Buffer.Pixels){
            System.Buffer.MemoryCopy(__MappedPtr, Dst, Buffer.Pixels.Length * 4, Buffer.Pixels.Length * 4);
        }
    }
}