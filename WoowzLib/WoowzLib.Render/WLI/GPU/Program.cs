using WLO.Math;

namespace WLI.GPU;

public interface Program : WLI.GPU.Resource{
    bool IsLinked{ get; }
}