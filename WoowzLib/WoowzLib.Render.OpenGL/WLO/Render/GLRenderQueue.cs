using WLO.GPU;
using WLO.Render.Hardware;

namespace WLO.Render;

// TODO, добавить возможность делать свои render

public class GLRenderQueue{
    public readonly OpenGL Owner;
    public GLRenderQueue(OpenGL Render){ Owner = Render; }

    private readonly List<Command> __OpaqueCommands      = [];
    private readonly List<Command> __TransparentCommands = [];

    public void Submit(Command CMD){
        if(CMD.Mesh == null){ return; }

        if(CMD.IsTransparent){
            __TransparentCommands.Add(CMD);
        }else{
            __OpaqueCommands.Add(CMD);
        }
    }

    public void Render(Action<Command, GLProgram>? OnBeforeDraw = null){
        Sort();

        foreach(Command CMD in __OpaqueCommands){ RenderObject(CMD, null, OnBeforeDraw); }
        
        foreach(Command CMD in __TransparentCommands){ RenderObject(CMD, null, OnBeforeDraw); }
        
        Clear();
    }

    public void RenderWithProgram(GLProgram Program, Action<Command, GLProgram>? OnBeforeDraw = null){
        Sort();
        
        foreach(Command CMD in __OpaqueCommands){ RenderObject(CMD, Program, OnBeforeDraw); }
        
        foreach(Command CMD in __TransparentCommands){ RenderObject(CMD, Program, OnBeforeDraw); }
        
        Clear();
    }

    private void RenderObject(Command CMD, GLProgram? OverrideProgram, Action<Command, GLProgram>? Callback){
        GLProgram? ActiveProgram = OverrideProgram ?? CMD.Program;
        if(ActiveProgram == null){ return; }

        Owner.Pool.SetTexture2D(CMD.Texture2D);
        
        if(CMD.Uniforms != null){
            foreach(UniformValue Uniform in CMD.Uniforms){ ActiveProgram.SetUniform(Uniform); }
        }
        
        Callback?.Invoke(CMD, ActiveProgram);
        
        Owner.Draw(CMD.Mesh!, ActiveProgram);
    }
    
    private void Sort(){
        __OpaqueCommands.Sort((A, B) => {
            uint AID = A.Program?.ID ?? 0;
            uint BID = B.Program?.ID ?? 0;
            if(AID != BID){ return AID.CompareTo(BID); }

            AID = A.Texture2D?.ID ?? 0;
            BID = B.Texture2D?.ID ?? 0;
            if(AID != BID){ return AID.CompareTo(BID); }
            
            return A.Mesh!.ID.CompareTo(B.Mesh!.ID);
        });
        
        __TransparentCommands.Sort((A, B) => B.DistanceToCamera.CompareTo(A.DistanceToCamera));
    }

    private void Clear(){
        __OpaqueCommands.Clear();
        __TransparentCommands.Clear();
    }
    
    public struct Command{
        public GLProgram?   Program;
        public GLMesh?      Mesh;
        public GLTexture2D? Texture2D;

        public List<UniformValue>? Uniforms;

        public float DistanceToCamera;
        public bool  IsTransparent;

        public uint ObjectID;
    }
}