namespace WL;

public static partial class Logger{
    public static WLI.Logger CurrentLogger = new WLO.Logger.Simple();
    
    public static void Log(uint Type, object Message) => CurrentLogger.Log(Type, Message);
    
    public static void Info (object Message) => CurrentLogger.Info(Message);
    public static void Warn (object Message) => CurrentLogger.Warn(Message);
    public static void Error(object Message) => CurrentLogger.Error(Message);
    public static void Debug(object Message) => CurrentLogger.Debug(Message);
    public static void Fatal(object Message) => CurrentLogger.Fatal(Message);
    public static void Trace(object Message) => CurrentLogger.Trace(Message);
}