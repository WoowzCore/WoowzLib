using Silk.NET.OpenGL;
using WLO.Math;
using WLO.Render;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLProgram : WLI.GPU.GLResource, WLI.GPU.Program{
    public bool IsLinked{ get; } = false;
    
    private GLProgram(OpenGL Render, WLI.GPU.Shader[] Shaders) : base(Render){
        ID = Owner.API.CreateProgram();
        if(ID == 0){ throw new ExceptionWL("todo, failed create glprogram"); }
        
        foreach(WLI.GPU.Shader Shader in Shaders){
            Owner.API.AttachShader(ID, Shader.ID);
        }
        
        Owner.API.LinkProgram(ID);

        Owner.API.GetProgram(ID, ProgramPropertyARB.LinkStatus, out int Status);
        if(Status == 0){
            string InfoLog = Owner.API.GetProgramInfoLog(ID);
            throw new ExceptionWL($"Ошибка линковки программы: {InfoLog}");
        }

        Owner.API.GetProgramInterface(ID, ProgramInterface.Uniform, ProgramInterfacePName.ActiveResources, out int UniformsCount);
        if(UniformsCount > 0){

            ReadOnlySpan<ProgramResourceProperty> Properties =[
                ProgramResourceProperty.Location,
                ProgramResourceProperty.NameLength
            ];

            uint PropsCount = (uint)Properties.Length;
            int[] Results = new int[Properties.Length];

            unsafe{
                fixed(ProgramResourceProperty* PtrProperties = Properties){
                    for(uint i = 0; i < (uint)UniformsCount; i++){
                        uint Written;

                        fixed(int* PtrResults = Results){
                            Owner.API.GetProgramResource(ID, ProgramInterface.Uniform, i, PropsCount, PtrProperties, PropsCount, &Written, PtrResults);
                        }

                        int Location   = Results[0];
                        int NameLength = Results[1];

                        if(Location != -1){
                            __Uniforms.Add(Location);

                            if(NameLength > 0){
                                Owner.API.GetProgramResourceName(ID, ProgramInterface.Uniform, i, (uint)NameLength, out uint _, out string Name);

                                if(!string.IsNullOrEmpty(Name)){ __UniformNames[Name] = Location; }
                            }
                        }
                    }
                }
            }
            
        }

        IsLinked = true;
    }
    
    public static GLProgram Create(OpenGL Render, WLI.GPU.Shader[] Shaders){
        GLProgram Result = new GLProgram(Render, Shaders);
        
        Render.Pool.RegistryProgram[Result.ID] = Result;

        return Result;
    }
    
    private GLProgram(OpenGL Render, uint TargetID) : base(Render){
        FromID = true;
        ID = TargetID;

        Owner.API.GetProgram(ID, ProgramPropertyARB.LinkStatus, out int Status);
        IsLinked = Status != 0;
    }

    public static GLProgram GetExists(OpenGL Render, uint TargetID){
        if(TargetID == 0){ return null!; }
        
        if(Render.Pool.RegistryProgram.TryGetValue(TargetID, out GLProgram? Result)){ return Result; }

        if(!Render.API.IsProgram(TargetID)){ throw new ExceptionWL("Указан несуществующий ID!"); }

        Result = new GLProgram(Render, TargetID);
        Render.Pool.RegistryProgram[TargetID] = Result;

        return Result;
    }

    public override void OnDestroy(){
        Owner.Pool.RegistryProgram.Remove(ID);
        if(object.Equals(Owner.Pool.GetProgram(), this)){ Owner.Pool.SetProgram(null, true); }
        Owner.API.DeleteProgram(ID);
    }

    // ----------------------------------------------------------------------

    private readonly HashSet   <int              > __Uniforms      = [];
    private readonly Dictionary<int, UniformValue> __UniformValues = [];
    private readonly Dictionary<string, int      > __UniformNames  = [];
    
    public int GetLocationFromName(string Name){
        if(__UniformNames.TryGetValue(Name, out int Location)){ return Location; }

        Location = Owner.API.GetUniformLocation(ID, Name);
        __UniformNames[Name] = Location;

        if(Location == -1){ Owner.Log(Owner.LogType_Uniform, $"Uniform ID \"{Name}\" не найден в программе {this}!"); }
        
        return Location;
    }
    
    public void SetUniform(UniformValue NewValue){
        if(NewValue.Location <= -1){ return; }

        if(!__Uniforms.Contains(NewValue.Location)){ return; }

        if(__UniformValues.TryGetValue(NewValue.Location, out UniformValue OldValue) && OldValue.Equals(NewValue)){ return; }

        unsafe{
            switch(NewValue.Type){
                case UniformValue.DataType.Float   : Owner.API.ProgramUniform1(ID, NewValue.Location, NewValue.F1); break;
                case UniformValue.DataType.Int     : Owner.API.ProgramUniform1(ID, NewValue.Location, (int)NewValue.F1); break;
                case UniformValue.DataType.Vector2F: Owner.API.ProgramUniform2(ID, NewValue.Location, NewValue.F1, NewValue.F2); break;
                case UniformValue.DataType.Vector3F: Owner.API.ProgramUniform3(ID, NewValue.Location, NewValue.F1, NewValue.F2, NewValue.F3); break;
                case UniformValue.DataType.Matrix4F: Owner.API.ProgramUniformMatrix4(ID, NewValue.Location, 1, false, (float*)&NewValue.Matrix); break;
            }
        }

        __UniformValues[NewValue.Location] = NewValue;
    }

    public UniformValue? GetUniform(int Location) => __UniformValues.TryGetValue(Location, out UniformValue Value) ? Value : null;
}