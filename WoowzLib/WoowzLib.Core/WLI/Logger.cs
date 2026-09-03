namespace WLI;

public interface Logger{
    void Log(uint Type, object? Message, Exception? e = null);
    
    void Info (object? Message, Exception? e = null) => Log((uint)Type.Info   , Message, e);
    void Warn (object? Message, Exception? e = null) => Log((uint)Type.Warning, Message, e);
    void Error(object? Message, Exception? e = null) => Log((uint)Type.Error  , Message, e);
    void Debug(object? Message, Exception? e = null) => Log((uint)Type.Debug  , Message, e);
    void Fatal(object? Message, Exception? e = null) => Log((uint)Type.Fatal  , Message, e);
    void Trace(object? Message, Exception? e = null) => Log((uint)Type.Trace  , Message, e);

    void PrefixPush(object Prefix);
    void PrefixPop (             );
    
    event Action<uint, object?, Exception?>? OnRawLog;
    event Action<uint, string             >? OnLog;
    
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