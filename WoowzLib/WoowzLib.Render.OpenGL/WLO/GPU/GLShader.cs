using Silk.NET.OpenGL;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLShader : GLResource, WLI.GPU.Shader{
    public GLShader(OpenGL Render, string VertexSource, string FragmentSource) : base(Render){
        uint Vertex   = Compile(ShaderType.VertexShader  , VertexSource  );
        uint Fragment = Compile(ShaderType.FragmentShader, FragmentSource);

        ID = __Owner.API.CreateProgram();
        __Owner.API.AttachShader(ID, Vertex  );
        __Owner.API.AttachShader(ID, Fragment);
        __Owner.API.LinkProgram(ID);
        
        __Owner.API.DeleteShader(Vertex  );
        __Owner.API.DeleteShader(Fragment);
    }

    private uint Compile(ShaderType Type, string Source){
        uint Shader = __Owner.API.CreateShader(Type);
        __Owner.API.ShaderSource(Shader, Source);
        __Owner.API.CompileShader(Shader);
        // todo error check
        return Shader;
    }

    public override void Dispose() => __Owner.API.DeleteProgram(ID);
}