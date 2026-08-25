using System.Collections;
using System.Reflection;
using WLI;

namespace WL;

public static class Packer{
    public static string PackType = "PackType";
    
    public static object? Pack(object? Object){
        if(Object == null){ return null; }

        if(Object is WLI.Packable Packable){
            Dictionary<string, object?> RawData = Packable.__Pack();

            Dictionary<string, object?> Result = new Dictionary<string, object?>();
            foreach(KeyValuePair<string, object?> KVP in RawData){
                Result[KVP.Key] = Pack(KVP.Value);
            }
            
            Result[PackType] = Object.GetType().AssemblyQualifiedName!;
            return Result;
        }

        if(Object is IDictionary Dictionary){
            Dictionary<string, object> Result = new Dictionary<string, object>();
            foreach(DictionaryEntry e in Dictionary){
                Result[e.Key.ToString()!] = Pack(e.Value)!;
            }

            return Result;
        }

        if(Object is IEnumerable List && Object is not string){
            List<object?> Result = [];
            foreach (object? i in List){ Result.Add(Pack(i)); }
            return Result;
        }

        return Object;
    }

    public static object? Unpack(object? Data, Type? TargetType = null){
        if(Data == null){ return null; }

        if (Data is Dictionary<string, object?> Dictionary && Dictionary.TryGetValue(PackType, out object? TypeName)) {
            Type? Type = FindType(TypeName!.ToString()!);
            if(Type != null){
                Packable Instance = (WLI.Packable)Activator.CreateInstance(Type, true)!;
                Instance.__Unpack(Dictionary);
                return Instance;
            }
        }

        if(Data is IList List && TargetType != null && TargetType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(TargetType)){
            Type ElementType = TargetType.IsArray ? TargetType.GetElementType()! : TargetType.GetGenericArguments()[0];
            IList Result = TargetType.IsArray ? Array.CreateInstance(ElementType, List.Count) : (IList)Activator.CreateInstance(TargetType)!;

            for(int i = 0; i < List.Count; i++){
                object? Value = Unpack(List[i], ElementType);
                if (TargetType.IsArray) Result[i] = Value; else Result.Add(Value);
            }
            return Result;
        }

        if (TargetType != null && TargetType != Data.GetType()){ try { return Convert.ChangeType(Data, Nullable.GetUnderlyingType(TargetType) ?? TargetType); } catch { return Data; } }

        return Data;
    }

    public static void Unpack(WLI.Packable Target, Dictionary<string, object?> Data) => Target.__Unpack(Data);

    public static T? Get<T>(Dictionary<string, object?> data, string key, T defaultValue = default!, bool Raw = false){
        if(!data.TryGetValue(key, out object? Value) || Value == null!){ return defaultValue; }
        if(Raw){
            return (T)Value;
        }
        return (T)Unpack(Value, typeof(T))!;
    }

    private static Type? FindType(string name){
        Type? Type = Type.GetType(name);
        if(Type != null){ return Type; }
        foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()){
            Type = a.GetType(name); if(Type != null){ return Type; }
        }
        return null;
    }
}