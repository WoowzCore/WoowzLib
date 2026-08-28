using WLI.Format;
using WLO;

namespace WL;

public partial class Loader{
    private static readonly Dictionary<string, ImageFormat   > __ImageFormats    = [];
    private static readonly Dictionary<string, GeometryFormat> __GeometryFormats = [];

    public static void Register(string ID, ImageFormat Format){
        if(!__ImageFormats.TryAdd(ID, Format)){ return; }
    }
    
    public static void Register(string ID, GeometryFormat Format){
        if(!__GeometryFormats.TryAdd(ID, Format)){ return; }
    }
    
    // ----------------------------------------------------------------------

    public static Image LoadImage(string ID, byte[] Data){
        if(__ImageFormats.TryGetValue(ID, out ImageFormat? Format)){
            return Format.__Load(Data);
        }

        throw new ExceptionWL("Неизвестный формат изображения!");
    }
    
    public static Image LoadImage(byte[] Data){
        foreach((string Key, ImageFormat Format) in __ImageFormats){
            if(Format.__Is(Data)){ return Format.__Load(Data); }
        }

        throw new ExceptionWL("Неизвестный формат изображения!");
    }

    public static bool IsImage(string ID, byte[] Data){
        if(__ImageFormats.TryGetValue(ID, out ImageFormat? Format)){
            return Format.__Is(Data);
        }

        return false;
    }
    
    public static string? IsImage(byte[] Data){
        foreach((string Key, ImageFormat Format) in __ImageFormats){
            if(Format.__Is(Data)){ return Key; }
        }

        return null;
    }
    
    // ----------------------------------------------------------------------
    
    public static WLO.Geometry LoadGeometry(string ID, byte[] Data){
        if(__GeometryFormats.TryGetValue(ID, out GeometryFormat? Format)){
            return Format.__Load(Data);
        }

        throw new ExceptionWL("Неизвестный формат изображения!");
    }

    public static WLO.Geometry LoadGeometry(byte[] Data){
        foreach((string Key, GeometryFormat Format) in __GeometryFormats){
            if(Format.__Is(Data)){ return Format.__Load(Data); }
        }

        throw new ExceptionWL("Неизвестный формат геометрии!");
    }

    public static bool IsGeometry(string ID, byte[] Data){
        if(__GeometryFormats.TryGetValue(ID, out GeometryFormat? Format)){
            return Format.__Is(Data);
        }

        return false;
    }
    
    public static string? IsGeometry(byte[] Data){
        foreach((string Key, GeometryFormat Format) in __GeometryFormats){
            if(Format.__Is(Data)){ return Key; }
        }

        return null;
    }
}