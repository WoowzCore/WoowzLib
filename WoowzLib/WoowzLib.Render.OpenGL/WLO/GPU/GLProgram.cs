using Silk.NET.OpenGL;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLProgram : WLI.GPU.GLResource, WLI.GPU.Program{
    public bool IsLinked{ get; } = false;
    
    public GLProgram(OpenGL Render, WLI.GPU.Shader[] Shaders) : base(Render){
        ID = __Owner.API.CreateProgram();

        foreach(WLI.GPU.Shader Shader in Shaders){
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



    public override void OnDestroy() => __Owner.API.DeleteProgram(ID);
    
    public void SetUniformF(int Uniform, float Value){
        throw new NotImplementedException();
    }
    public void SetUniformI(int Uniform, int Value){
        throw new NotImplementedException();
    }
    public void SetUniformB(int Uniform, bool Value){
        throw new NotImplementedException();
    }
    public void SetUniformV2F(int Uniform, Vector2F Value){
        throw new NotImplementedException();
    }
    public void SetUniformV2I(int Uniform, Vector2I Value){
        throw new NotImplementedException();
    }
}