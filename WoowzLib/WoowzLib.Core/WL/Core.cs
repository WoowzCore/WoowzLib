using WLO;

namespace WL;

public struct Core{
    public static ProjectInfo ProjectInfo = new ProjectInfo(null);
    public static ProjectInfo EngineInfo  = new ProjectInfo(null);

    public static string[]? Arguments;
    
    public static string PathToEXE => Environment.ProcessPath!;
    public static string PathToFolderEXE => Path.GetDirectoryName(PathToEXE)!;
    public static string PathToTempFolderEXE => AppContext.BaseDirectory;
}