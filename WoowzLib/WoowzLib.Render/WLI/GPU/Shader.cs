namespace WLI.GPU;

public interface Shader : WLI.GPU.Resource{
    Type Stage{ get; }
    
    public enum Type{
        Vertex,
        Fragment,
        Geometry,
        Compute
    }
}