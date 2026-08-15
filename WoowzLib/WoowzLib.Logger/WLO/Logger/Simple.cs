namespace WLO.Logger;

public class Simple : WLI.Logger{
    public string[] GetActivePrefixes(){
        if(__Prefixes.Count == 0){ return []; }

        return __Prefixes.Reverse().ToArray();
    }
    
    public string GeneratePrefix(uint Type){
        string Time = DateTime.Now.ToString("HH:mm:ss:fff");
        string TypePrefix = Type switch{
            (uint)WLI.Logger.Type.Debug   => "D",
            (uint)WLI.Logger.Type.Info    => "I",
            (uint)WLI.Logger.Type.Warning => "W",
            (uint)WLI.Logger.Type.Error   => "E",
            (uint)WLI.Logger.Type.Fatal   => "F",
            (uint)WLI.Logger.Type.Trace   => "T",
            var _ => Type.ToString()
        };

        string[] ActivePrefixes = GetActivePrefixes();

        string ActivePrefixes__ = ActivePrefixes.Length > 0 ? string.Join("|", ActivePrefixes) + "|" : string.Empty;
        
        return $"{TypePrefix}|{Time}|{ActivePrefixes__}";
    }

    private void __ChangeConsoleBackground(uint Type){
        Console.ForegroundColor = Type switch{
            (uint)WLI.Logger.Type.Debug   => ConsoleColor.Green,
            (uint)WLI.Logger.Type.Info    => ConsoleColor.White,
            (uint)WLI.Logger.Type.Warning => ConsoleColor.Yellow,
            (uint)WLI.Logger.Type.Error   => ConsoleColor.Red,
            (uint)WLI.Logger.Type.Fatal   => ConsoleColor.Magenta,
            (uint)WLI.Logger.Type.Trace   => ConsoleColor.Blue,
            var _ => ConsoleColor.DarkGray
        };
    }
    
    public void Log(uint Type, object Message){
        string Content = Message?.ToString() ?? "null";
        
        __ChangeConsoleBackground(Type);
        
        Console.WriteLine($"{GeneratePrefix(Type)}: {Content}");
    }
    
    // ----------------------------------------------------------------------

    private readonly Stack<string> __Prefixes = new Stack<string>();
    
    public void PrefixPush(object Prefix){
        string Content = Prefix?.ToString() ?? "null";
        __Prefixes.Push(Content);
    }
    
    public void PrefixPop(){
        if(__Prefixes.Count > 0){
            __Prefixes.Pop();
        }
    }
}