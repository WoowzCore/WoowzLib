namespace WLI;

public interface LanguageContext : IDisposable{
    Language __Owner{ get; }
    
    object? Execute(string Code);

    void SetVariable(string Name, object? Value);
    object? GetVariable(string Name);

    object? Call(string Name, params object?[] Args);

    event Action<string, Exception> OnError;
}