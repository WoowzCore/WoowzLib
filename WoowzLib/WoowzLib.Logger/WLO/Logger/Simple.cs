namespace WLO.Logger;

public class Simple : WLI.Logger{
    public string GeneratePrefix(uint Type){
        string Time = DateTime.Now.ToString("HH:mm:ss");
        string TypePrefix = Type switch{
            (uint)WLI.Logger.Type.Debug   => "D",
            (uint)WLI.Logger.Type.Info    => "I",
            (uint)WLI.Logger.Type.Warning => "W",
            (uint)WLI.Logger.Type.Error   => "E",
            (uint)WLI.Logger.Type.Fatal   => "F",
            (uint)WLI.Logger.Type.Trace   => "T",
            _ => Type.ToString()
        };

        return $"{TypePrefix}|{Time}|";
    }

    public void ChangeConsoleBackground(uint Type){
        Console.ForegroundColor = Type switch{
            (uint)WLI.Logger.Type.Debug   => ConsoleColor.Green,
            (uint)WLI.Logger.Type.Info    => ConsoleColor.White,
            (uint)WLI.Logger.Type.Warning => ConsoleColor.Yellow,
            (uint)WLI.Logger.Type.Error   => ConsoleColor.Red,
            (uint)WLI.Logger.Type.Fatal   => ConsoleColor.Magenta,
            (uint)WLI.Logger.Type.Trace   => ConsoleColor.Blue,
            _ => ConsoleColor.DarkGray
        };
    }
    
    public void Log(uint Type, object Message){
        string Content = Message?.ToString() ?? "null";
        
        ChangeConsoleBackground(Type);
        
        Console.WriteLine($"{GeneratePrefix(Type)}: {Content}");
    }
}