using Jint;
using Jint.Native;
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
            OnError?.Invoke($"[JS ({e.Location}) (Execute)]: {Code.Length}", e);
            return null;
        }catch(Exception e){
            OnError?.Invoke($"[JS System (Execute)]: {Code.Length}", e);
            throw;
        }
    }

    public void SetVariable(string Name, object? Value) => __Engine.SetValue(Name, Value);
    public object? GetVariable(string Name) => __Engine.GetValue(Name).ToObject();

    public object? Call(string Name, params object?[] Args){
        try{
            JsValue Value = __Engine.Evaluate(Name);
            if(Value.IsUndefined() || Value.IsNull()){ return Value; }
            
            return __Engine.Invoke(Value, Args).ToObject();
        }catch(Jint.Runtime.JavaScriptException e){
            OnError?.Invoke($"[JS ({e.Location}) (Call)]: {Name}({WL.String.Join(Args)})", e);
            return null;
        }
    }

    // ----------------------------------------------------------------------

    public readonly Jint.Engine __Engine;
    
    public void Dispose() => __Engine.Dispose();
}