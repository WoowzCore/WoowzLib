namespace WLI;

public interface Logger{
    void Log(uint Type, object Message);
    
    void Info (object Message) => Log((uint)Type.Info   , Message);
    void Warn (object Message) => Log((uint)Type.Warning, Message);
    void Error(object Message) => Log((uint)Type.Error  , Message);
    void Debug(object Message) => Log((uint)Type.Debug  , Message);
    void Fatal(object Message) => Log((uint)Type.Fatal  , Message);
    void Trace(object Message) => Log((uint)Type.Trace  , Message);

    void PrefixPush(object Prefix);
    void PrefixPop (             );
    
    event Action<uint, object>? OnRawLog;
    event Action<uint, string>? OnLog;
    
    public enum Type : uint{
        Debug     = 100,
        Info      = 200,
        Warning   = 300,
        Error     = 400,
        Fatal     = 500,
        Trace     = 600,
        NoLog     = 700
    }
}