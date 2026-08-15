using Silk.NET.OpenGL;
using WLI_Render;
using WLI.GPU;
using WLO.GPU;
using WLO.Math;
using Buffer = WLI.GPU.Buffer;
using Shader = WLI.GPU.Shader;
using Texture = WLI.GPU.Texture;

namespace WLO.Render.Hardware;

public class OpenGL : WLI_Render.Hardware{
    public  GL API => __API;
    private GL        __API;
    
    public RenderView CurrentRenderView{ get; private set; }
    
    public bool IsStarted{ get; private set; }

    private Func<string, IntPtr> __ProcLoader;
    public OpenGL(Func<string, IntPtr> ProcLoader){
        __ProcLoader = ProcLoader;
    }
    
    public void Start(){
        if(IsStarted){ throw new ExceptionWL("todo"); }

        __API = GL.GetApi(__ProcLoader);
        
        CurrentRenderView = new GLRenderView(this);
        
        IsStarted = true;
    }
    
    public void Stop(){
        if(!IsStarted){ throw new ExceptionWL("todo"); }

        IsStarted = false;
    }

    public void FrameStart(RenderView? Target = null){
        RenderView View = Target ?? CurrentRenderView;
        __API.Viewport(0, 0, (uint)View.Viewport.X, (uint)View.Viewport.Y);
    }
    
    public void FrameStop(){
        
    }
    
    public Buffer CreateBuffer(uint Usage, uint Size) => new GLBuffer(this, BufferTargetARB.ArrayBuffer, Size);

    public Shader CreateShader(string VertexSource, string FragmentSource) => new GLShader(this, VertexSource, FragmentSource);
    
    public unsafe Mesh CreateMesh<T>(T[] Vertices, uint[]? Indices = null) where T : unmanaged{
        GLBuffer VBO = new GLBuffer(this, BufferTargetARB.ArrayBuffer, (uint)(Vertices.Length * sizeof(T)));
        VBO.Update(Vertices);

        GLBuffer EBO = null!;
        if(Indices != null){
            EBO = new GLBuffer(this, BufferTargetARB.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)));
            EBO.Update(Indices);
        }

        return new GLMesh(this, VBO, EBO, (uint)Vertices.Length, (uint)(Indices?.Length ?? 0));
    }
    
    public Texture CreateTexture(Vector2I Size, uint Format){
        throw new NotImplementedException();
    }
}