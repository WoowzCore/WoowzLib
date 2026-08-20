using WLO.Render.Hardware;

namespace WLI.GPU;

public abstract class GLResource : WLI.GPU.Resource, IEquatable<GLResource>{
    public uint ID{ get; protected set; }
    public bool FromID{ get; protected set; }
    
    protected readonly OpenGL __Owner;

    protected GLResource(OpenGL Render) => __Owner = Render;

    public bool Destroyed{ get; set; }

    public void Destroy(){
        if(Destroyed){ return; }

        Destroyed = true;
        OnDestroy();
        ID = 0;
    }
    public void Dispose() => Destroy();

    public abstract void OnDestroy();
    
    // ----------------------------------------------------------------------

    public string ToString_GLResource() => $"{ID}, {(Destroyed ? "DESTROYED" : $"{FromID}, {__Owner}")}";
    public override string ToString() => $"{GetType().Name}({ToString_GLResource()})";

    public bool Equals(GLResource? Other){
        if(Other is null){ return false; }
        if(ReferenceEquals(this, Other)){ return true; }

        return ID == Other.ID && __Owner == Other.__Owner && GetType() == Other.GetType();
    }

    public override bool Equals(object? Object) => Object is GLResource Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(ID, __Owner, GetType());
}