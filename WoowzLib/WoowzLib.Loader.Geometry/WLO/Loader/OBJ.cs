using System.Text;
using WLI.Format;

namespace WLO.Loader;

public class OBJ : Format_Assimp{
    public const string ID = "OBJ";
    
    static OBJ() => WL.Loader.Register(ID, new OBJ());
    
    public OBJ(){
        AssimpID = "obj";
    }
    
    public override bool __Is(byte[] Data){
        if(Data.Length < 10){ return false; }
        string Head = Encoding.UTF8.GetString(Data, 0, System.Math.Min(Data.Length, 1024));
        return Head.Contains("v ") || Head.Contains("v  ") || Head.Contains("f ") || Head.Contains("vt ") || Head.Contains("mtllib ") || Head.Contains("usemtl ");
    }
    
    // ----------------------------------------------------------------------

    public static Geometry Load(byte[] Data) => WL.Loader.Load<Geometry>(ID, Data);
    public static bool Is(byte[] Data) => WL.Loader.Is<Geometry>(ID, Data);
}