using System.Globalization;
using WLO;

namespace WL;

public struct Core{
    public static ProjectInfo ProjectInfo = new ProjectInfo(null);
    public static ProjectInfo EngineInfo  = new ProjectInfo(null);

    public static string[]? Arguments;
    
    public static string PathToEXE => Environment.ProcessPath!;
    public static string PathToFolderEXE => Path.GetDirectoryName(PathToEXE)!;
    public static string PathToTempFolderEXE => AppContext.BaseDirectory;

    public static void Start(string[] Arguments){
        WL.Core.Arguments = Arguments;
        
        CultureInfo.DefaultThreadCurrentCulture   = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }
}