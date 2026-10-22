using System.Globalization;
using System.Runtime.InteropServices;
using WLO;

namespace WL;

public struct Core{
    public static ProjectInfo ProjectInfo = new ProjectInfo(null);
    public static ProjectInfo EngineInfo  = new ProjectInfo(null);

    public static string[]? Arguments;
    
    public static string PathToEXE => Environment.ProcessPath!;
    public static string PathToFolderEXE => Path.GetDirectoryName(PathToEXE)!;
    public static string PathToTempFolderEXE => AppContext.BaseDirectory;

    // ---------------------------------------------------------------------- 

    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    private static extern uint TimeBeginPeriod(uint Milliseconds);
    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    private static extern uint TimeEndPeriod(uint Milliseconds);

    private static bool Started = false;
    
    public static void Start(string[] Arguments){
        if(Started){ throw new ExceptionWL("WoowzLib Core уже был запущен!"); } Started = true;

        WL.Core.Arguments = Arguments;
        
        CultureInfo.DefaultThreadCurrentCulture   = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        TimeBeginPeriod(1);
    }

    public static int Stop(int ExitCode = 0){
        if(!Started){ throw new ExceptionWL("WoowzLib Core уже был остановлен!"); }

        TimeEndPeriod(1);
        
        Started = false;

        return ExitCode;
    }
}