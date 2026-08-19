using Silk.NET.OpenGL;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLProgram : WLI.GPU.GLResource, WLI.GPU.Program{
    public bool IsLinked{ get; } = false;
    
    private GLProgram(OpenGL Render, WLI.GPU.Shader[] Shaders) : base(Render){
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
    
    public static GLProgram Create(OpenGL Render, WLI.GPU.Shader[] Shaders){
        GLProgram Result = new GLProgram(Render, Shaders);
        
        Render.Registry_Program[Result.ID] = Result;

        return Result;
    }
    
    private GLProgram(OpenGL Render, uint TargetID) : base(Render){
        FromID = true;
        ID = TargetID;

        __Owner.API.GetProgram(ID, ProgramPropertyARB.LinkStatus, out int Status);
        IsLinked = Status != 0;
    }

    public static GLProgram GetExists(OpenGL Render, uint TargetID){
        if(TargetID == 0){ return null!; }
        
        if(Render.Registry_Program.TryGetValue(TargetID, out GLProgram Result)){ return Result; }

        if(!Render.API.IsProgram(TargetID)){ throw new ExceptionWL("Указан несуществующий ID!"); }

        Result = new GLProgram(Render, TargetID);
        Render.Registry_Program[TargetID] = Result;

        return Result;
    }

    public override void OnDestroy(){
        __Owner.Registry_Program.Remove(ID);
        __Owner.API.DeleteProgram(ID);
    }

    // ----------------------------------------------------------------------
    
    private readonly Dictionary<string, int> __Uniforms = [];
    public int GetUniform(string Name){
        if(__Uniforms.TryGetValue(Name, out int Location)){ return Location; }

        Location = __Owner.API.GetUniformLocation(ID, Name);
        __Uniforms[Name] = Location;

        if(Location == -1){ __Owner.Log(__Owner.LogType_Uniform, $"Uniform \"{Name}\" не найден в программе {this}!"); }
        
        return Location;
    }
    
    public bool UniformCorrect(int Uniform) => Uniform != -1;
    
    // ----------------------------------------------------------------------

    private readonly Dictionary<int, float   > __FValues   = [];
    private readonly Dictionary<int, int     > __IValues   = [];
    private readonly Dictionary<int, Vector2F> __V2FValues = [];
    private readonly Dictionary<int, Vector2I> __V2IValues = [];
    private readonly Dictionary<int, Vector3F> __V3FValues = [];
    private readonly Dictionary<int, Matrix4F> __M4FValues = [];
    
    // TODO, добавить get uniform
    
    public void SetUniformF(int Uniform, float Value){
        if(!UniformCorrect(Uniform)){ return; }
        if(__FValues.TryGetValue(Uniform, out float Old) && Old == Value){ return; }
        
        __Owner.API.ProgramUniform1(ID, Uniform, Value);
        __FValues[Uniform] = Value;

    }
    
    public void SetUniformI(int Uniform, int Value){
        if(!UniformCorrect(Uniform)){ return; }
        if(__IValues.TryGetValue(Uniform, out int Old) && Old == Value){ return; }
        
        __Owner.API.ProgramUniform1(ID, Uniform, Value);
        __IValues[Uniform] = Value;
        
    }
    
    public void SetUniformB(int Uniform, bool Value){
        if(!UniformCorrect(Uniform)){ return; }
        int Value__ = Value ? 1 : 0;
        if(__IValues.TryGetValue(Uniform, out int Old) && Old == Value__){ return; }
        
        __Owner.API.ProgramUniform1(ID, Uniform, Value__);
        __IValues[Uniform] = Value__;

    }
    
    public void SetUniformV2F(int Uniform, Vector2F Value){
        if(!UniformCorrect(Uniform)){ return; }
        if(__V2FValues.TryGetValue(Uniform, out Vector2F Old) && Old == Value){ return; }
        
        __Owner.API.ProgramUniform2(ID, Uniform, Value.X, Value.Y);
        __V2FValues[Uniform] = Value;
    }
    
    public void SetUniformV2I(int Uniform, Vector2I Value){
        if(!UniformCorrect(Uniform)){ return; }
        if(__V2IValues.TryGetValue(Uniform, out Vector2I Old) && Old == Value){ return; }
        
        __Owner.API.ProgramUniform2(ID, Uniform, Value.X, Value.Y);
        __V2IValues[Uniform] = Value;
        
    }
    
    public void SetUniformV3F(int Uniform, Vector3F Value){
        if(!UniformCorrect(Uniform)){ return; }
        if(__V3FValues.TryGetValue(Uniform, out Vector3F Old) && Old == Value){ return; }
        
        __Owner.API.ProgramUniform3(ID, Uniform, Value.X, Value.Y, Value.Z);
        __V3FValues[Uniform] = Value;
    }
    
    public void SetUniformM4F(int Uniform, Matrix4F Value){
        if(!UniformCorrect(Uniform)){ return; }
        if(__M4FValues.TryGetValue(Uniform, out Matrix4F Old) && Old == Value){ return; }

        unsafe{
            __Owner.API.ProgramUniformMatrix4(ID, Uniform, 1, false, (float*)&Value);
        }
        __M4FValues[Uniform] = Value;
    }
}