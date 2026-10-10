using System.Text;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using WLI;

namespace WLOLanguageContext;

public class ClearScript : LanguageContext{
    public Language __Owner{ get; }
    public WLOLanguage.ClearScript Owner => (WLOLanguage.ClearScript)__Owner;

    public event Action<string, Exception>? OnError;

    private void LogError(string ActionType, string Target, ScriptEngineException e){
        if(OnError == null){ return; }
        
        StringBuilder SB = new StringBuilder();
        string Head = $" ClearScript ERROR ({ActionType}) ";
        SB.AppendLine($"[{Head}]");
        SB.AppendLine($"Message: {e.Message}");
        SB.AppendLine($"Function: {Target}");

        if(!string.IsNullOrEmpty(e.ErrorDetails)){
            SB.AppendLine($"Details: {e.ErrorDetails}");
        }

        if(e.ScriptException != null){
            SB.AppendLine($"Exception Data: {e.ScriptException.ToString()}");
        }
        
        SB.AppendLine($"[{WL.String.Repeat("-", Head.Length)}]");
        
        OnError?.Invoke(SB.ToString(), e);
    }
    
    public ClearScript(WLOLanguage.ClearScript Owner){
        __Owner = Owner;

        __Engine = new V8ScriptEngine(V8ScriptEngineFlags.DisableGlobalMembers | V8ScriptEngineFlags.AddPerformanceObject | V8ScriptEngineFlags.EnableTaskPromiseConversion);
    }

    public object? Execute(string Code){
        try{
            return __Engine.Evaluate(Code);
        }catch(ScriptEngineException e){
            LogError("Execute", $"RawCode ({Code.Length})", e);
            return null;
        }catch(Exception e){
            OnError?.Invoke($"[ClearScript System (Execute)]: {Code.Length}", e);
            throw;
        }
    }

    public void SetVariable(string Name, object? Value){
        try{
            if(Value == null){
                __Engine.Script[Name] = null;
            }else if(Value is ScriptObject){
                __Engine.Script[Name] = Value;
            }else{
                __Engine.AddHostObject(Name, Value);   
            }
        }catch(Exception e){
            OnError?.Invoke($"[ClearScript System (SetVariable)]: {Name}", e);
        }
    }
    public object? GetVariable(string Name){
        try{
            dynamic? Result = __Engine.Script[Name];
            return (Result is Undefined) ? null : Result;
        }catch{
            return null;
        }
    }

    public object? Call(string Name, params object?[] Args){
        try{
            ScriptObject? Function = __Engine.Evaluate(Name) as ScriptObject;

            if(Function == null){ return null; }

            return Function.Invoke(false, Args);
        }catch(ScriptEngineException e){
            LogError("Call", Name, e);
            return null;
        }catch(Exception e){
            OnError?.Invoke($"[JS System (Call)]: {Name}", e);
            return null;
        }
    }

    // ----------------------------------------------------------------------

    public readonly V8ScriptEngine __Engine;
    
    public void Dispose() => __Engine.Dispose();
}