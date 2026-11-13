using System.Runtime.InteropServices;

namespace WLN;

public struct WinMM{
    public const string DLL = "winmm.dll";
    
    [DllImport(DLL, EntryPoint = "timeBeginPeriod")]
    public static extern uint TimeBeginPeriod(uint Milliseconds);
    
    [DllImport(DLL, EntryPoint = "timeEndPeriod")]
    public static extern uint TimeEndPeriod(uint Milliseconds);
}