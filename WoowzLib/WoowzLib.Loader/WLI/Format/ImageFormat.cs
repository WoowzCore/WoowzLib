using WLO;

namespace WLI.Format;

public interface ImageFormat{
    bool __Is(byte[] Data);
    Image __Load(byte[] Data);
}