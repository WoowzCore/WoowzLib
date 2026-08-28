using System.Text;
using WLI.Format;

namespace WLO.Loader;

public class FBX : Format_Assimp{
    public const string ID = "FBX";
    
    static FBX() => WL.Loader.Register(ID, new FBX());

    public FBX(){
        AssimpID = "fbx";
    }
    
    public override bool __Is(byte[] Data){
        if(Data.Length < 20){ return false; }
        string Head = Encoding.UTF8.GetString(Data, 0, 18);
        return Head.StartsWith("Kaydara FBX Binary");
    }
    
    // ----------------------------------------------------------------------

    public static Geometry Load(byte[] Data) => WL.Loader.Load<Geometry>(ID, Data);
    public static bool Is(byte[] Data) => WL.Loader.Is<Geometry>(ID, Data);
}