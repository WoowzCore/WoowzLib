using System.Runtime.CompilerServices;
using WLI.Format;
using WLO;

namespace WL;

public partial class Loader{
    private static readonly Dictionary<Type, object> __Registries = [];

    private static Dictionary<string, Format<T>> GetRegistry<T>() where T : class{
        Type Type = typeof(T);
        if(!__Registries.TryGetValue(Type, out object? Registry)){
            Registry = new Dictionary<string, Format<T>>();
            __Registries[Type] = Registry;
        }

        return (Dictionary<string, Format<T>>)Registry;
    }

    // todo, сука какое же говно
    public static void UpdateRegister(params Type[] FormatTypes){
        foreach(Type Type in FormatTypes){
            RuntimeHelpers.RunClassConstructor(Type.TypeHandle);
        }
    }
    
    public static void Register<T>(string ID, Format<T> Format) where T : class{
        Format.LinkedID = ID;
        Dictionary<string, Format<T>> Registry = GetRegistry<T>();
        if(!Registry.TryAdd(ID, Format)){
            // todo. 🤷‍♂️
        }
    }

    public static T Load<T>(string ID, byte[] Data) where T : class{
        if(GetRegistry<T>().TryGetValue(ID, out Format<T>? Format)){
            return Format.__Load(Data);
        }
        throw new ExceptionWL($"Неизвестный формат {ID} для типа {typeof(T).Name}!");
    }

    public static T Load<T>(byte[] Data) where T : class{
        foreach(Format<T> Format in GetRegistry<T>().Values){
            if(Format.__Is(Data)){ return Format.__Load(Data); }
        }
        throw new ExceptionWL($"Не удалось определить формат данных для типа {typeof(T).Name}!");
    }
    
    public static bool Is<T>(string ID, byte[] Data) where T : class => GetRegistry<T>().TryGetValue(ID, out Format<T>? format) && format.__Is(Data);

    public static string? GetFormatID<T>(byte[] Data) where T : class{
        foreach(KeyValuePair<string, Format<T>> KVP in GetRegistry<T>()) {
            if(KVP.Value.__Is(Data)){ return KVP.Key; }
        }
        
        return null;
    }
}