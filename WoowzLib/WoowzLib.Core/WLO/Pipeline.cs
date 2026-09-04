namespace WEO;

public class Pipeline{
    private readonly Dictionary<string, object> __Stages = [];

    public Stage<T> GetOrCreate<T>(string Name) where T : Delegate{
        if(!__Stages.TryGetValue(Name, out object? Stage)){
            Stage = new Stage<T>();
            __Stages[Name] = Stage;
        }
        return (Stage<T>)Stage;
    }

    public void Run(string Name, params object?[] Args){
        if(__Stages.TryGetValue(Name, out object? Stage)){
            try{
                ((dynamic)Stage).Run(Args);
            }catch(Exception e){
                WL.Logger.Error($"todo, Ошибка при выполнении этапа \"{Name}\": {e.InnerException?.Message ?? e.Message}\n{e.InnerException?.StackTrace ?? e.StackTrace}", e);   
            }
        }
    }
    
    public void Clear(){
        foreach(KeyValuePair<string, object> KVP in __Stages){
            object Stage = KVP.Value;
            ((dynamic)Stage).Clear();
        }
        __Stages.Clear();
    }

    public void Clear(string Name){
        if(__Stages.TryGetValue(Name, out object? Stage)){
            ((dynamic)Stage).Clear();
            __Stages.Remove(Name);
        }
    }
}