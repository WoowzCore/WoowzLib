using WL;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLShader : WLI.GPU.GLResource, WLI.GPU.Shader{
    public WLI.GPU.Shader.Type Stage{ get; }

    public GLShader(OpenGL Render, WLI.GPU.Shader.Type Stage, string Source) : base(Render){
        this.Stage = Stage;
        ID = OpenGL.CompileGLSL(Owner.API, Stage, Source);
    }

    public GLShader(OpenGL Render, WLSL.Result WLSL) : this(Render, WLSL.Type, WLSL.GLSL){}

    public override void OnDestroy() => Owner.API.DeleteShader(ID);
}