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
    public Vk? API => __API;
        private Vk? __API;

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
        public uint GraphicsQueueFamilyIndex => __GraphicsQueueFamilyIndex;
        private uint __GraphicsQueueFamilyIndex;

        /**
         * Очередь команд
         * Содержит в себе список команд для видеокарты
         */
        public Queue GraphicsQueue => __GraphicsQueue;
        private Queue __GraphicsQueue;
        
        
        private DebugUtilsMessengerEXT __DebugMessanger;
        private bool                           __EnableValidationLayers = true;
        
    #endregion
    
    #region [ РЕСУРСЫ КАДРА ]

        /**
         * Сырой кусок видеопамяти, где лежат пиксели
         * <seealso cref="RenderImageView"/>
         */
        public Image RenderImage => __RenderImage;
        private Image __RenderImage;

        /**
         * Просмотр RenderImage, даёт понять как правильно смотреть
         * <seealso cref="RenderImage"/>
         */
        public ImageView RenderImageView => __RenderImageView;
        private ImageView __RenderImageView;

        /**
         * Указывает куда рисовать
         */
        public Framebuffer RenderFrameBuffer => __RenderFrameBuffer;
        private Framebuffer __RenderFrameBuffer;

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
        public CommandPool CommandPool => __CommandPool;
        private CommandPool __CommandPool;

        /**
         * Запись команд перед записью в очередь
         */
        public CommandBuffer CommandBuffer => __CommandBuffer;
        private CommandBuffer __CommandBuffer;

        /**
         * Синхронизирует GPU и CPU
         */
        public Fence RenderFence => __RenderFence;
        private Fence __RenderFence;

        /**
         * Общая память между GPU и CPU
         */
        public Buffer StagingBuffer => __StagingBuffer;
        private Buffer __StagingBuffer;

        
        private DeviceMemory __StagingMemory;
        private void*        __MappedPtr;
        
    #endregion

    public Vector2I Viewport{ get; set; }
    public bool IsStarted => __API != null;
    private GCHandle __VulkanInstanceHandle;
    
    
    /**
     * Какой Logger будет использоваться Vulkan
     */
    public WLI.Logger? CurrentLogger;
    
    /**
     * Vulkan находится в режиме рисования?
     */
    public bool IsRenderState{ get; private set; }

    /**
    * Проверяет, API Vulkan инициализирован?
    */
    public void CheckVulkan(bool? MustBeRendering = null){ if(!IsStarted){ throw new ExceptionWL("Vulkan не инициализирован!"); } if(MustBeRendering.HasValue && MustBeRendering.Value != this.IsRenderState){ throw new ExceptionWL($"Невозможно выполнить код, потому-что текущий режим Vulkan [{(this.IsRenderState ? "Рисования" : "НЕ Рисования")}] не подходит! Нужен режим [{(MustBeRendering.Value ? "Рисования" : "НЕ Рисования")}]"); } }
    
    /**
     * Запуск Vulkan, выдаст ошибку если запустить ещё раз
     */
    public void Start(){
        try{
            if(IsStarted){ throw new ExceptionWL("Vulkan уже был инициализирован!"); }

            CurrentLogger = WL.Logger.CurrentLogger;
            
            __API = Vk.GetApi();
        
            __CreateInstance();
            __SetupDebugMessanger();
            __PickPhysicalDevice();
            __CreateLogicalDevice();

            if(Viewport.W == 0){ Viewport = new Vector2I(800, 600); } // todo
            
            __CreateResources();

            Format PixelsFormat = Format.B8G8R8A8Unorm;
            
            InternalCreateImage(Viewport, PixelsFormat, ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferSrcBit, out __RenderImage, out __RenderImageMemory);
            
            __InitRenderPipeline();
            __CreateDescriptorPool();

            uint BufferSize = (uint)(Viewport.W * Viewport.H * 4);
            InternalCreateBuffer(BufferSize, BufferUsageFlags.TransferDstBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, out __StagingBuffer, out __StagingMemory);

            void* Ptr;
            __API.MapMemory(__Device, __StagingMemory, 0, BufferSize, 0, &Ptr);
            __MappedPtr = Ptr;
            
            CurrentLogger?.Debug("todo, Vulkan инициализирован!");
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при запуске рендера Vulkan!\nWLO.Render.Hardware.Vulkan.Start()", e);
        }
    }
    
    /**
     * Остановить Vulkan
     */
    public void Stop(){
        if(!IsStarted){ return; }
        __API!.DeviceWaitIdle(__Device);

        void Destroy<T>(ref T H, Action<T> Action) where T : unmanaged{ if(Unsafe.As<T, ulong>(ref H) != 0){ Action(H); H = default; } }
        
        Destroy(ref __DescriptorPool , H => __API.DestroyDescriptorPool(__Device, H, null));
        Destroy(ref __RenderFrameBuffer    , H => __API.DestroyFramebuffer(__Device, H, null));
        Destroy(ref __RenderPass     , H => __API.DestroyRenderPass(__Device, H, null));
        Destroy(ref __RenderImageView, H => __API.DestroyImageView(__Device, H, null));
        
        Destroy(ref __RenderFence, H => __API.DestroyFence(__Device, H, null));
        Destroy(ref __CommandPool, H => __API.DestroyCommandPool(__Device, H, null));
        
        Destroy(ref __StagingBuffer, H => __API.DestroyBuffer(__Device, H, null));
        Destroy(ref __StagingMemory, H => { __API.UnmapMemory(__Device, H); __API.FreeMemory(__Device, H, null); });
        
        Destroy(ref __RenderImage      , H => __API.DestroyImage(__Device, H, null));
        Destroy(ref __RenderImageMemory, H => __API.FreeMemory(__Device, H, null));
        
        Destroy(ref __Device, H => __API.DestroyDevice(__Device, null));
        
        if(__EnableValidationLayers && __Instance.Handle != 0){
            if(__API.TryGetInstanceExtension<ExtDebugUtils>(__Instance, out ExtDebugUtils? DebugUtils)){
                DebugUtils.DestroyDebugUtilsMessenger(__Instance, __DebugMessanger, null);
            }
        }

        if(__VulkanInstanceHandle.IsAllocated){ __VulkanInstanceHandle.Free(); }

        Destroy(ref __Instance, H => __API.DestroyInstance(__Instance, null));
    }
    
    // ----------------------------------------------------------------------

    /**
     * Выделение памяти
     */
    public void AllocateMemory(MemoryRequirements Requirements, MemoryPropertyFlags Properties, out DeviceMemory Memory){
        MemoryAllocateInfo Allocate = new MemoryAllocateInfo{ SType = StructureType.MemoryAllocateInfo, AllocationSize = Requirements.Size, MemoryTypeIndex = FindMemoryType(Requirements.MemoryTypeBits, Properties) };
        fixed(DeviceMemory* Ptr = &Memory){ __API!.AllocateMemory(__Device, &Allocate, null, Ptr); }
    }
    
    /**
     * Универсальный метод создания буфера с выделением памяти
     */
    public void InternalCreateBuffer(uint Size, BufferUsageFlags Usage, MemoryPropertyFlags Properties, out Buffer Buffer, out DeviceMemory Memory){
        BufferCreateInfo Info = new BufferCreateInfo{ SType = StructureType.BufferCreateInfo, Size = Size, Usage = Usage, SharingMode = SharingMode.Exclusive };
        fixed(Buffer* Ptr = &Buffer){ __API!.CreateBuffer(__Device, &Info, null, Ptr); }

        MemoryRequirements Requirements;
        __API!.GetBufferMemoryRequirements(__Device, Buffer, &Requirements);

        AllocateMemory(Requirements, Properties, out Memory);
        
        __API!.BindBufferMemory(__Device, Buffer, Memory, 0);
    }

    /**
     * Универсальный метод создания картинки с выделением памяти
     */
    public void InternalCreateImage(Vector2I Size, Format Format, ImageUsageFlags Usage, out Image Image, out DeviceMemory Memory){
        ImageCreateInfo Info = new ImageCreateInfo{ SType = StructureType.ImageCreateInfo, ImageType = ImageType.Type2D, Extent = new Extent3D((uint)Size.W, (uint)Size.H, 1), MipLevels = 1, ArrayLayers = 1, Format = Format, Tiling = ImageTiling.Optimal, InitialLayout = ImageLayout.Undefined, Usage = Usage, Samples = SampleCountFlags.Count1Bit, SharingMode = SharingMode.Exclusive };
        fixed(Image* Ptr = &Image){ __API!.CreateImage(__Device, &Info, null, Ptr); }

        MemoryRequirements Requirements;
        __API!.GetImageMemoryRequirements(__Device, Image, &Requirements);

        AllocateMemory(Requirements, MemoryPropertyFlags.DeviceLocalBit, out Memory);

        __API!.BindImageMemory(__Device, Image, Memory, 0);
    }
    
    /**
     * Находит подходящий индекс типа памяти видеокарты
     */
    public uint FindMemoryType(uint TypeFilter, MemoryPropertyFlags Properties){
        PhysicalDeviceMemoryProperties MemoryProperties;
        __API!.GetPhysicalDeviceMemoryProperties(__PhysicalDevice, &MemoryProperties);

        for(int i = 0; i < MemoryProperties.MemoryTypeCount; i++){
            if((TypeFilter & (1u << i)) != 0 && (MemoryProperties.MemoryTypes[i].PropertyFlags & Properties) == Properties){ return (uint)i; }
        }

        throw new ExceptionWL("Не удалось найти подходящий тип памяти! todo");
    }
    
    // ----------------------------------------------------------------------
    
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static Bool32 __DebugCallback(DebugUtilsMessageSeverityFlagsEXT Severity, DebugUtilsMessageTypeFlagsEXT Types, DebugUtilsMessengerCallbackDataEXT* CallbackData, void* UserData){
        if(UserData != null){
            GCHandle Handle = GCHandle.FromIntPtr((IntPtr)UserData);

            if(Handle.Target is Vulkan Instance){
                string Message = Marshal.PtrToStringAnsi((IntPtr)CallbackData->PMessage) ?? "Неизвестное Vulkan сообщение!";
    
                uint Level = (uint)WLI.Logger.Type.Debug;
                if(Severity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt)){
                    Level = (uint)WLI.Logger.Type.Error;
                }else if(Severity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.WarningBitExt)){
                    Level = (uint)WLI.Logger.Type.Warning;
                }

                Instance.CurrentLogger?.Log(Level, $"[Vulkan] {Message}");
            }
        }
        
        return Vk.False;
    }
    
    private void __SetupDebugMessanger(){
        CheckVulkan();
        
        if(!__EnableValidationLayers){ return; }
        
        if(!__API!.TryGetInstanceExtension<ExtDebugUtils>(__Instance, out ExtDebugUtils DebugUtils)){
            throw new ExceptionWL($"Не удалось найти расширение ExtDebugUtils! WLO.Render.Hardware.Vulkan.__VK.TryGetInstanceExtension<ExtDebugUtils>({__Instance}, out ExtDebugUtils {DebugUtils}) вернул false!");
        }
        
        __VulkanInstanceHandle = GCHandle.Alloc(this);
        void* UserDataPtr = (void*)GCHandle.ToIntPtr(__VulkanInstanceHandle);

        DebugUtilsMessengerCreateInfoEXT CreateInfo = new DebugUtilsMessengerCreateInfoEXT{ SType = StructureType.DebugUtilsMessengerCreateInfoExt, MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt, MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt, PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(&__DebugCallback), PUserData = UserDataPtr };

        fixed(DebugUtilsMessengerEXT* Ptr = &__DebugMessanger){
            DebugUtils.CreateDebugUtilsMessenger(__Instance, &CreateInfo, null, Ptr);
        }
    }

    private bool __CheckVulkanSDK(){
        uint LayerCount = 0;
        __API!.EnumerateInstanceLayerProperties(&LayerCount, null);
        LayerProperties* AvailableLayers = stackalloc LayerProperties[(int)LayerCount];
        __API!.EnumerateInstanceLayerProperties(&LayerCount, AvailableLayers);

        bool KhronosAvailable = false;
        for(int i = 0; i < LayerCount; i++){
            string Name = Marshal.PtrToStringAnsi((IntPtr)AvailableLayers[i].LayerName);
            if(Name == "VK_LAYER_KHRONOS_validation"){
                KhronosAvailable = true;
                break;
            }
        }
        
        if(__EnableValidationLayers && !KhronosAvailable){
            CurrentLogger?.Error("[Vulkan] VK_LAYER_KHRONOS_validation не найден в системе. Валидация отключена. (скачайте Vulkan SDK)");
            __EnableValidationLayers = false; 
        }

        return KhronosAvailable;
    }
    
    private void __CreateInstance(){
        fixed(byte* AppName = "WLO_APP"u8, EngineName = "WLO_ENGINE"u8){
            // todo, брать из WL.Core
            ApplicationInfo AppInfo = new ApplicationInfo{ SType = StructureType.ApplicationInfo, PApplicationName = AppName, ApplicationVersion = new Version32(1, 0, 0), PEngineName = EngineName, EngineVersion = new Version32(1, 0, 0), ApiVersion = Vk.Version12 };
            InstanceCreateInfo CreateInfo = new InstanceCreateInfo{ SType = StructureType.InstanceCreateInfo, PApplicationInfo = &AppInfo };
            
            if(__EnableValidationLayers){
                __CheckVulkanSDK();
                
                string[] Layers     = ["VK_LAYER_KHRONOS_validation"];
                string[] Extensions = ["VK_EXT_debug_utils"];
                CreateInfo.EnabledLayerCount = 1;
                CreateInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(Layers);
                CreateInfo.EnabledExtensionCount = 1;
                CreateInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(Extensions);
            }
            
            Result __Result_CreateInstance = __API!.CreateInstance(&CreateInfo, null, out __Instance);
            if(__Result_CreateInstance != Result.Success){ throw new ExceptionWL($"Произошла ошибка в WLO.Render.Hardware.Vulkan.__VK.CreateInstance({CreateInfo}, null, out {__Instance}), вернуло {__Result_CreateInstance}!"); }

            if(__EnableValidationLayers){
                SilkMarshal.Free((IntPtr)CreateInfo.PpEnabledLayerNames);
                SilkMarshal.Free((IntPtr)CreateInfo.PpEnabledExtensionNames);
            }
        }
    }

    private void __PickPhysicalDevice(){
        uint DeviceCount = 0;
        __API!.EnumeratePhysicalDevices(__Instance, &DeviceCount, null);

        if(DeviceCount == 0){ throw new ExceptionWL($"Не найдены видеокарты с поддержкой Vulkan! WLO.Render.Hardware.Vulkan.__VK.EnumeratePhysicalDevices({__Instance}, &DeviceCount, null) вернул 0!"); }

        PhysicalDevice* Devices = stackalloc PhysicalDevice[(int)DeviceCount];
        __API!.EnumeratePhysicalDevices(__Instance, &DeviceCount, Devices);

        for(int i = 0; i < DeviceCount; i++){
            if(__IsDeviceSuitable(Devices[i])){
                __PhysicalDevice = Devices[i];
                break;
            }
        }

        if(__PhysicalDevice.Handle == 0){ throw new ExceptionWL("Не найдена подходящая видеокарта для Vulkan! WLO.Render.Hardware.Vulkan.__IsDeviceSuitable(Devices[i]) все равны false!"); }

        PhysicalDeviceProperties Properties;
        __API!.GetPhysicalDeviceProperties(__PhysicalDevice, &Properties);
        CurrentLogger?.Debug($"todo, Выбрана видеокарта: {Marshal.PtrToStringAnsi((IntPtr)Properties.DeviceName)}");
    }

    private bool __IsDeviceSuitable(PhysicalDevice Device){
        uint QueueFamilyCount = 0;
        __API!.GetPhysicalDeviceQueueFamilyProperties(Device, &QueueFamilyCount, null);

        QueueFamilyProperties* Families = stackalloc QueueFamilyProperties[(int)QueueFamilyCount];
        __API!.GetPhysicalDeviceQueueFamilyProperties(Device, &QueueFamilyCount, Families);

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
            Result Result = __API!.CreateDevice(__PhysicalDevice, &DeviceCreateInfo, null, DevicePtr);
            if(Result != Result.Success){ throw new ExceptionWL($"Произошла ошибка при создании Device! WLO.Render.Hardware.Vulkan.__VK.CreateDevice({__PhysicalDevice}, &{DeviceCreateInfo}, null, ...) вернул {Result}!"); }
        }

        fixed(Queue* QueuePtr = &__GraphicsQueue){
            __API!.GetDeviceQueue(__Device, __GraphicsQueueFamilyIndex, 0, QueuePtr);
        }
    }

    private void __CreateResources(){
        // CommandPool
        
        CommandPoolCreateInfo CreateInfo_CommandPool = new CommandPoolCreateInfo{ SType = StructureType.CommandPoolCreateInfo, QueueFamilyIndex = __GraphicsQueueFamilyIndex, Flags = CommandPoolCreateFlags.ResetCommandBufferBit };

        fixed(CommandPool* Ptr = &__CommandPool){
            __API!.CreateCommandPool(__Device, &CreateInfo_CommandPool, null, Ptr);
        }

        CommandBufferAllocateInfo AllocateInfo = new CommandBufferAllocateInfo{ SType = StructureType.CommandBufferAllocateInfo, CommandPool = __CommandPool, Level = CommandBufferLevel.Primary, CommandBufferCount = 1 };

        fixed(CommandBuffer* Ptr = &__CommandBuffer){
            __API!.AllocateCommandBuffers(__Device, &AllocateInfo, Ptr);
        }
        
        // ----------------------------------------------------------------------
        // Fence
        
        FenceCreateInfo CreateInfo_Fence = new FenceCreateInfo{ SType = StructureType.FenceCreateInfo, Flags = FenceCreateFlags.SignaledBit };

        fixed(Fence* Ptr = &__RenderFence){
            __API!.CreateFence(__Device, &CreateInfo_Fence, null, Ptr);
        }
    }

    private void __InitRenderPipeline(){
        // ImageView
        
        ImageViewCreateInfo ViewInfo = new ImageViewCreateInfo{ SType = StructureType.ImageViewCreateInfo, Image = __RenderImage, ViewType = ImageViewType.Type2D, Format = Format.B8G8R8A8Unorm, SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1) };

        fixed(ImageView* Ptr = &__RenderImageView){
            __API!.CreateImageView(__Device, &ViewInfo, null, Ptr);
        }
        
        // ----------------------------------------------------------------------
        // RenderPass
        
        AttachmentDescription ColorAttachment          = new AttachmentDescription{ Format = Format.B8G8R8A8Unorm, Samples = SampleCountFlags.Count1Bit, LoadOp = AttachmentLoadOp.DontCare, StoreOp = AttachmentStoreOp.Store, StencilLoadOp = AttachmentLoadOp.DontCare, StencilStoreOp = AttachmentStoreOp.DontCare, InitialLayout = ImageLayout.Undefined, FinalLayout = ImageLayout.TransferSrcOptimal };
        AttachmentReference   ColorAttachmentReference = new AttachmentReference{ Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal };
        SubpassDescription    Subpass                  = new SubpassDescription{ PipelineBindPoint = PipelineBindPoint.Graphics, ColorAttachmentCount = 1, PColorAttachments = &ColorAttachmentReference };
        RenderPassCreateInfo  RenderPassInfo           = new RenderPassCreateInfo{ SType = StructureType.RenderPassCreateInfo, AttachmentCount = 1, PAttachments = &ColorAttachment, SubpassCount = 1, PSubpasses = &Subpass };

        fixed(RenderPass* Ptr = &__RenderPass){
            __API!.CreateRenderPass(__Device, &RenderPassInfo, null, Ptr);
        }
        
        // ----------------------------------------------------------------------
        // Framebuffer
        
        fixed(ImageView* Attachment = &__RenderImageView){
            FramebufferCreateInfo FramebufferInfo = new FramebufferCreateInfo{ SType = StructureType.FramebufferCreateInfo, RenderPass = __RenderPass, AttachmentCount = 1, PAttachments = Attachment, Width = (uint)Viewport.W, Height = (uint)Viewport.H, Layers = 1 };

            fixed(Framebuffer* Ptr = &__RenderFrameBuffer){
                __API!.CreateFramebuffer(__Device, &FramebufferInfo, null, Ptr);
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
                __API!.CreateDescriptorPool(__Device, &PoolInfo, null, Ptr);
            }
        }
    }
    
    // ----------------------------------------------------------------------

    public void Clear(Color4B Color){
        CheckVulkan(true);
        ClearAttachment ClearAttachment = new ClearAttachment{ AspectMask = ImageAspectFlags.ColorBit, ColorAttachment = 0, ClearValue = new ClearValue{ Color = new ClearColorValue{ Float32_0 = Color.R / 255f, Float32_1 = Color.G / 255f, Float32_2 = Color.B / 255f, Float32_3 = Color.A / 255f } } };
        ClearRect ClearRect = new ClearRect{ Rect = new Rect2D(new Offset2D(0, 0), new Extent2D((uint)Viewport.W, (uint)Viewport.H)), BaseArrayLayer = 0, LayerCount = 1 };
        __API!.CmdClearAttachments(__CommandBuffer, 1, &ClearAttachment, 1, &ClearRect);
    }
    
    public void FrameStart(){
        CheckVulkan(false);
        
        __API!.WaitForFences(__Device, 1, ref __RenderFence, true, ulong.MaxValue);
        __API!.ResetFences(__Device, 1, ref __RenderFence);

        CommandBufferBeginInfo BeginInfo = new CommandBufferBeginInfo{ SType = StructureType.CommandBufferBeginInfo, Flags = CommandBufferUsageFlags.OneTimeSubmitBit };

        __API!.BeginCommandBuffer(__CommandBuffer, &BeginInfo);
        
        RenderPassBeginInfo RenderPassInfo = new RenderPassBeginInfo{ SType = StructureType.RenderPassBeginInfo, RenderPass = __RenderPass, Framebuffer = __RenderFrameBuffer, RenderArea = new Rect2D(new Offset2D(0, 0), new Extent2D((uint)Viewport.W, (uint)Viewport.H)), ClearValueCount = 0, PClearValues = null };
        
        __API!.CmdBeginRenderPass(__CommandBuffer, &RenderPassInfo, SubpassContents.Inline);
        
        IsRenderState = true;
    }

    public void FrameStop(){
        CheckVulkan(true);
        IsRenderState = false;
        
        __API!.CmdEndRenderPass(__CommandBuffer);
        
       BufferImageCopy Region = new BufferImageCopy{ BufferOffset = 0, BufferRowLength = 0, BufferImageHeight = 0, ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1), ImageOffset = new Offset3D(0, 0, 0), ImageExtent = new Extent3D((uint)Viewport.W, (uint)Viewport.H, 1) };
       __API!.CmdCopyImageToBuffer(__CommandBuffer, __RenderImage, ImageLayout.TransferSrcOptimal, __StagingBuffer, 1, &Region);

       __API!.EndCommandBuffer(__CommandBuffer);

       fixed(CommandBuffer* CommandBufferPtr = &__CommandBuffer){
           SubmitInfo SubmitInfo = new SubmitInfo{ SType = StructureType.SubmitInfo, CommandBufferCount = 1, PCommandBuffers = CommandBufferPtr };

           __API.QueueSubmit(__GraphicsQueue, 1, &SubmitInfo, __RenderFence);
       }
    }
    
    public void DrawFrameBuffer(FrameBuffer Buffer){
        __API!.WaitForFences(__Device, 1, ref __RenderFence, true, ulong.MaxValue);

        fixed(Color4B* Dst = Buffer.Pixels){
            System.Buffer.MemoryCopy(__MappedPtr, Dst, Buffer.Pixels.Length * 4, Buffer.Pixels.Length * 4);
        }
    }
}