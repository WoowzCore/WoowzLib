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
        for(int i = 0; i < __Actions.Count; i++){
            Invoker(__Actions[i].Action);
        }
    }

    public void Run(params object?[] Args){
        for(int i = 0; i < __Actions.Count; i++){
            __Actions[i].Action.DynamicInvoke(Args);
        }
    }
}