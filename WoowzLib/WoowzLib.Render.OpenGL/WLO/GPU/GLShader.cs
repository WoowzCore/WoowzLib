using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLShader : WLI.GPU.GLResource, WLI.GPU.Shader{
    public WLI.GPU.Shader.Type Stage{ get; }
    
    public GLShader(OpenGL Render, WLI.GPU.Shader.Type Stage, string Source) : base(Render){
        this.Stage = Stage;
        ID = WL.OpenGL.CompileGLSL(__Owner.API, Stage, Source);
    }

    public override void Dispose() => __Owner.API.DeleteShader(ID);
}