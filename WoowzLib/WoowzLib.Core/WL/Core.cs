using System.Globalization;
using System.Runtime.InteropServices;
using WLO;

namespace WL;

public struct Core{
    public static bool Started{ get; private set; }
    
    public static string[]? Arguments{ get; private set; }
    
    public static ProjectInfo ProjectInfo = new ProjectInfo(null);
    public static ProjectInfo EngineInfo  = new ProjectInfo(null);
    
    public static string PathToEXE           => Environment.ProcessPath!;
    public static string PathToFolderEXE     => Path.GetDirectoryName(PathToEXE)!;
    public static string PathToTempFolderEXE => AppContext.BaseDirectory;
    
    // ----------------------------------------------------------------------
    
    public static void Start(string[] Arguments){
        if(Started){ throw new ExceptionWL("WoowzLib.Core уже был запущен!"); }

        WL.Core.Arguments = Arguments;
        
        CultureInfo Culture = CultureInfo.InvariantCulture;
        
        CultureInfo.DefaultThreadCurrentCulture   = Culture;
        CultureInfo.DefaultThreadCurrentUICulture = Culture;
        CultureInfo.CurrentCulture                = Culture;
        CultureInfo.CurrentUICulture              = Culture;

        WLN.WinMM.TimeBeginPeriod(1);
        
        Started = true;
    }

    public static int Stop(int ExitCode = 0){
        if(!Started){ throw new ExceptionWL("WoowzLib.Core уже был остановлен!"); }

        WLN.WinMM.TimeEndPeriod(1);
        
        Started = false;

        return ExitCode;
    }
}