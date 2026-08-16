using Silk.NET.OpenGL;
using WLO.Render.Hardware;
using Shader = WLI.GPU.Shader;

namespace WLO.GPU;

public class GLShader : GLResource, WLI.GPU.Shader{
    public Shader.Type Stage{ get; }
    
    public GLShader(OpenGL Render, Shader.Type Stage, string Source) : base(Render){
        this.Stage = Stage;
        ID = WL.OpenGL.CompileGLSL(__Owner.API, Stage, Source);
    }

    public override void Dispose() => __Owner.API.DeleteShader(ID);
}