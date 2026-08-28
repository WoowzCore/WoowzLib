using WLO;

namespace WLI.Format;

public interface GeometryFormat{
    bool __Is(byte[] Data);
    Geometry __Load(byte[] Data);
}