using WLO.Math;

namespace WLI.GPU;

public interface Program : WLI.GPU.Resource{
    bool IsLinked{ get; }

    void SetUniformF(int Uniform, float Value);
    void SetUniformI(int Uniform, int Value);
    void SetUniformB(int Uniform, bool Value);
    void SetUniformV2F(int Uniform, Vector2F Value);
    void SetUniformV2I(int Uniform, Vector2I Value);
    void SetUniformM4F(int Uniform, Matrix4F Value);
}