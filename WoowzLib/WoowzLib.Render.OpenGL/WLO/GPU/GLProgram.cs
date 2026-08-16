using Silk.NET.OpenGL;
using WLO.Math;
using WLO.Render.Hardware;
using Shader = WLI.GPU.Shader;

namespace WLO.GPU;

public class GLProgram : GLResource, WLI.GPU.Program{
    public bool IsLinked{ get; } = false;
    
    public GLProgram(OpenGL Render, Shader[] Shaders) : base(Render){
        ID = __Owner.API.CreateProgram();

        foreach(Shader Shader in Shaders){
            __Owner.API.AttachShader(ID, Shader.ID);
        }
        
        __Owner.API.LinkProgram(ID);

        __Owner.API.GetProgram(ID, ProgramPropertyARB.LinkStatus, out int Status);
        if(Status == 0){
            string InfoLog = __Owner.API.GetProgramInfoLog(ID);
            throw new ExceptionWL($"Ошибка линковки программы: {InfoLog}");
        }

        IsLinked = true;
    }
    
    

    public override void Dispose() => __Owner.API.DeleteProgram(ID);
    
    public void SetUniformF(int Uniform, float Value){
        throw new NotImplementedException();
    }
    public void SetUniformV2F(int Uniform, Vector2F Value){
        throw new NotImplementedException();
    }
}