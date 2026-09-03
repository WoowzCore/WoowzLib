namespace WL;

// TODO, перехват сообщений из console нужно сделать

public struct Logger{
    public static WLI.Logger? CurrentLogger = new WLO.Logger.Simple();
    
    public static void Log(uint Type, object? Message, Exception? e = null) => CurrentLogger?.Log(Type, Message, e);
    
    public static void Info (object? Message, Exception? e = null) => CurrentLogger?.Info (Message, e);
    public static void Warn (object? Message, Exception? e = null) => CurrentLogger?.Warn (Message, e);
    public static void Error(object? Message, Exception? e = null) => CurrentLogger?.Error(Message, e);
    public static void Debug(object? Message, Exception? e = null) => CurrentLogger?.Debug(Message, e);
    public static void Fatal(object? Message, Exception? e = null) => CurrentLogger?.Fatal(Message, e);
    public static void Trace(object? Message, Exception? e = null) => CurrentLogger?.Trace(Message, e);

    public static void PrefixPush(object Prefix) => CurrentLogger?.PrefixPush(Prefix);
    public static void PrefixPop (             ) => CurrentLogger?.PrefixPop (      );
}