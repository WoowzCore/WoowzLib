using Jint;
using WLI;

namespace WLOLanguageContext;

public class JS : LanguageContext{
    public Language __Owner{ get; }
    public WLOLanguage.JS Owner => (WLOLanguage.JS)__Owner;

    public event Action<string, Exception>? OnError;

    public JS(WLOLanguage.JS Owner){
        __Owner = Owner;

        __Engine = new Jint.Engine(Options => {
            Options.LimitRecursion(64);
            Options.Strict = true;
        });
    }

    public object? Execute(string Code){
        try{
            return __Engine.Evaluate(Code).ToObject();
        }catch(Jint.Runtime.JavaScriptException e){
            OnError?.Invoke($"[JS ({e.Location}) (Execute)]", e);
            return null;
        }catch(Exception e){
            OnError?.Invoke("[JS System (Execute)]", e);
            throw;
        }
    }

    public void SetVariable(string Name, object? Value) => __Engine.SetValue(Name, Value);
    public object? GetVariable(string Name) => __Engine.GetValue(Name).ToObject();

    public object? Call(string Name, params object?[] Args){
        try{
            return __Engine.Invoke(Name, Args).ToObject();
        }catch(Jint.Runtime.JavaScriptException e){
            OnError?.Invoke($"[JS ({e.Location}) (Call)]", e);
            return null;
        }
    }

    // ----------------------------------------------------------------------

    public readonly Jint.Engine __Engine;
    
    public void Dispose() => __Engine.Dispose();
}