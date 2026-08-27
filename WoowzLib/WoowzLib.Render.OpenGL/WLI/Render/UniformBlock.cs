namespace WLI.Render;

public interface UniformBlock{
    WLO.GPU.GLBuffer Buffer{ get; }

    uint ID{ get; }
}