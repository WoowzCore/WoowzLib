using System.Text;
using Jint;
using Jint.Native;
using Jint.Runtime;
using WLI;

namespace WLOLanguageContext;

public class JS : LanguageContext{
    public Language __Owner{ get; }
    public WLOLanguage.JS Owner => (WLOLanguage.JS)__Owner;

    public event Action<string, Exception>? OnError;

    private void LogError(string ActionType, string Target, JavaScriptException e){
        if(OnError == null){ return; }
        
        StringBuilder SB = new StringBuilder();
        string Head = $" JS ERROR ({ActionType}) ";
        SB.AppendLine($"[{Head}]");
        SB.AppendLine($"Message: {e.Message}");
        SB.AppendLine($"Location: {e.Location}");
        SB.AppendLine($"Function: {Target}");

        if(!string.IsNullOrEmpty(e.JavaScriptStackTrace)){
            SB.AppendLine("JS StackTrace:");
            SB.AppendLine(e.JavaScriptStackTrace);
        }

        SB.AppendLine($"[{WL.String.Repeat("-", Head.Length)}]");
        
        OnError?.Invoke(SB.ToString(), e);
    }
    
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
        }catch(JavaScriptException e){
            LogError("Execute", $"RawCode ({Code.Length})", e);
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
        }catch(JavaScriptException e){
            LogError("Call", Name, e);
            return null;
        }catch(Exception e){
            OnError?.Invoke($"[JS System (Call)]: {Name}", e);
            return null;
        }
    }

    // ----------------------------------------------------------------------

    public readonly Jint.Engine __Engine;
    
    public void Dispose() => __Engine.Dispose();
}