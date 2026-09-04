using WLO;

namespace WEO;

public class Stage<T> where T : Delegate{
    private readonly List<(string Key, T Action)> __Actions = [];

    public void Add(string Key, T Action) => __Actions.Add((Key, Action));

    public void AddFirst(string Key, T Action) => __Actions.Insert(0, (Key, Action));

    public void AddBefore(string TargetKey, string Key, T Action){
        int Index = __Actions.FindIndex(A => A.Key == TargetKey);
        if(Index == -1){ __Actions.Add((Key, Action)); }else{ __Actions.Insert(Index, (Key, Action)); }
    }
    
    public void AddAfter(string TargetKey, string Key, T Action){
        int Index = __Actions.FindIndex(A => A.Key == TargetKey);
        if(Index == -1){ __Actions.Add((Key, Action)); }else{ __Actions.Insert(Index + 1, (Key, Action)); }
    }

    public void Remove(string Key) => __Actions.RemoveAll(A => A.Key == Key);

    public void Clear() => __Actions.Clear();

    public void Run(Action<T> Invoker){
        foreach((string Key, T Action) Action in __Actions){
            try{
                Invoker(Action.Action);
            }catch(Exception e){
                WL.Logger.Error($"todo, error on stage [\"{Action.Key}\"] INVOKER: {e.InnerException?.Message ?? e.Message}\n{e.InnerException?.StackTrace ?? e.StackTrace}", e);
            }
        }
    }

    public void Run(params object?[] Args){
        foreach((string Key, T Action) Action in __Actions){
            try{
                Action.Action.DynamicInvoke(Args);
            }catch(Exception e){
                WL.Logger.Error($"todo, error on stage [\"{Action.Key}\"]: {e.InnerException?.Message ?? e.Message}\n{e.InnerException?.StackTrace ?? e.StackTrace}", e);
            }
        }
    }
}