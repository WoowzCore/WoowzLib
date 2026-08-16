using WLO.Math;

namespace WLI.GPU;

public interface Program : WLI.GPU.Resource{
    bool IsLinked{ get; }

    void SetUniformF(int Uniform, float Value);
    void SetUniformV2F(int Uniform, Vector2F Value);
}