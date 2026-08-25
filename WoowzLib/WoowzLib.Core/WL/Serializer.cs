using System.Collections;
using System.Reflection;
using System.Text.Json;
using WLI;

namespace WL;

// нужен рефакторинг... очень сильно, насрано вонояет

public static class Serializer{
    public const string __Type  = "__Type";

    public static Type? FindType(string? TypeName){
        if(string.IsNullOrEmpty(TypeName)){ return null; }

        Type? Result = Type.GetType(TypeName);
        if(Result != null){ return Result; }

        foreach(Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies()){
            Result = Assembly.GetType(TypeName);
            if(Result != null){ return Result; }
        }
        
        return null;
    }
    
    public static Dictionary<string, object> Serialize(object? Object){
        if(Object == null){ return []; }

        Dictionary<string, object> Data;
        
        if(Object is Serializable Serializable){
            Data = Serializable.Serialize();
        }else if(Object is System.Collections.IEnumerable Enumerable and not string){
            List<object> List = [];
            foreach(object? Item in Enumerable){ List.Add(Serialize(Item)); }
            
            Data = new Dictionary<string, object>{
                ["Items"] = List,
                [__Type] = "List"
            };
        }else if(IsPrimitive(Object.GetType())){
            Data = new Dictionary<string, object>{
                ["Value"] = Object,
                [__Type] = Object.GetType().FullName!
            };
        }else{
            Data = SerializeObject(Object);
        }

        if(!Data.ContainsKey(__Type)){
            Data[__Type] = Object.GetType().AssemblyQualifiedName!;
        }
        
        return Data;
    }

    public static object? Deserialize(Dictionary<string, object> Data, Type? TargetType = null){
        if(Data.Count == 0){ return null; }
        
        if(Data.TryGetValue(__Type, out object? TypeName)){
            Type? Type = FindType(TypeName.ToString());
            
            if(Type != null && typeof(Serializable).IsAssignableFrom(Type)){
                Serializable? Instance = Activator.CreateInstance(Type) as Serializable;
                Instance?.Deserialize(Data);
                return Instance;
            }
        }
        
        if(Data.TryGetValue("Value", value: out object? Value) && Data.TryGetValue(__Type, out TypeName)){
            Type? Type = FindType(TypeName.ToString());
            if(Type != null && IsPrimitive(Type)){
                return Convert.ChangeType(Value, Type);
            }
            return Value;
        }
        
        if(Data.TryGetValue("Items", out object? Items) && Data.TryGetValue(__Type, out TypeName) && TypeName.ToString() == "List"){
            List<object?> List = [];
            foreach(object Item in (List<object>)Items){
                if(Item is Dictionary<string, object> Dictionary){
                    List.Add(Deserialize(Dictionary));
                }else{
                    List.Add(Item);
                }
            }
            return List;
        }

        if(Data.TryGetValue(__Type, out TypeName)){
            Type? Type = FindType(TypeName.ToString());
            if(Type != null){ return DeserializeObject(Data, Type); }
        }
        
        if(TargetType != null){
            return DeserializeObject(Data, TargetType);
        }
        
        return null;
    }

    public static Dictionary<string, object> SInt(int Value) => new Dictionary<string, object>{ ["Value"] = Value, [__Type] = typeof(int).FullName! };
    public static Dictionary<string, object> SFloat(float Value) => new Dictionary<string, object>{ ["Value"] = Value, [__Type] = typeof(float).FullName! };
    public static Dictionary<string, object> SDouble(double Value) => new Dictionary<string, object>{ ["Value"] = Value, [__Type] = typeof(double).FullName! };
    public static Dictionary<string, object> SBool(bool Value) => new Dictionary<string, object>{ ["Value"] = Value, [__Type] = typeof(bool).FullName! };
    public static Dictionary<string, object> SString(string Value) => new Dictionary<string, object>{ ["Value"] = Value, [__Type] = typeof(string).FullName! };

    public static int DInt(Dictionary<string, object> Data) => Convert.ToInt32(Data["Value"]);
    public static float DFloat(Dictionary<string, object> Data) => Convert.ToSingle(Data["Value"]);
    public static double DDouble(Dictionary<string, object> Data) => Convert.ToDouble(Data["Value"]);
    public static bool DBool(Dictionary<string, object> Data) => Convert.ToBoolean(Data["Value"]);
    public static string DString(Dictionary<string, object> Data) => Data["Value"]?.ToString() ?? "";

    private static bool IsPrimitive(Type Type) => Type.IsPrimitive || Type == typeof(string) || Type == typeof(decimal) || Type == typeof(DateTime);

    private static Dictionary<string, object> SerializeObject(object Object){
        Dictionary<string, object> Data = new Dictionary<string, object>();
        Type Type = Object.GetType();
        
        foreach(FieldInfo Field in Type.GetFields(BindingFlags.Public | BindingFlags.Instance)){
            object? Value = Field.GetValue(Object);
            if(Value != null){
                Data[Field.Name] = SerializeValue(Value);
            }
        }
        
        foreach(PropertyInfo Prop in Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)){
            if(Prop.CanRead && Prop.CanWrite){
                object? Value = Prop.GetValue(Object);
                if(Value != null){
                    Data[Prop.Name] = SerializeValue(Value);
                }
            }
        }
        
        return Data;
    }

    private static object DeserializeObject(Dictionary<string, object> Data, Type Type){
        object? Instance = Activator.CreateInstance(Type);
        if(Instance == null){ return null!; }
        
        foreach(FieldInfo Field in Type.GetFields(BindingFlags.Public | BindingFlags.Instance)){
            if(Data.TryGetValue(Field.Name, out object? Value) && Value != null!){
                Field.SetValue(Instance, DeserializeValue(Value, Field.FieldType));
            }
        }
        
        foreach(PropertyInfo Prop in Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)){
            if(Prop.CanRead && Prop.CanWrite && Data.TryGetValue(Prop.Name, out object? Value) && Value != null!){
                Prop.SetValue(Instance, DeserializeValue(Value, Prop.PropertyType));
            }
        }
        
        return Instance;
    }

    private static object SerializeValue(object Value){
        if(Value is Serializable S){ return S.Serialize(); }
        if(Value is System.Collections.IEnumerable Enumerable and not string){ return Enumerable.Cast<object>().Select(SerializeValue).ToList(); }
        if(IsPrimitive(Value.GetType())){ return Value; }
        return SerializeObject(Value);
    }

    private static object? DeserializeValue(object Value, Type TargetType){
        if(Value == null!){ return null; }
        
        if(TargetType.IsInstanceOfType(Value)){ return Value; }
        
        if(IsPrimitive(TargetType)){ return Convert.ChangeType(Value, TargetType); }
        
        if(Value is Dictionary<string, object> Dictionary){
            if(typeof(Serializable).IsAssignableFrom(TargetType)){
                Serializable? Instance = Activator.CreateInstance(TargetType) as Serializable;
                Instance?.Deserialize(Dictionary);
                return Instance;
            }
            
            return DeserializeObject(Dictionary, TargetType);
        }
        
        if(Value is List<object> List && TargetType.IsGenericType && TargetType.GetGenericTypeDefinition() == typeof(List<>)){
            Type ElementType = TargetType.GetGenericArguments()[0];
            IList? Result = Activator.CreateInstance(TargetType) as System.Collections.IList;
            if(Result != null){
                foreach(object Item in List){
                    object? Deserialized = DeserializeValue(Item, ElementType);
                    if(Deserialized != null) Result.Add(Deserialized);
                }
            }
            return Result;
        }
        
        return Value;
    }

    public static T? Get<T>(Dictionary<string, object> Data, string Key, T? DefaultValue = default){
        if(Data.TryGetValue(Key, out object? Value)){
            if(Value is T TypedValue){ return TypedValue; }

            if(Value is Dictionary<string, object> Dictionary){
                return (T?)Deserialize(Dictionary, typeof(T));
            }

            try{ return (T?)Convert.ChangeType(Value, typeof(T)); }catch{}
        }
        
        return DefaultValue;
    }
    
    // todo, remove that later
    public static string ToJson(Dictionary<string, object> Data) => JsonSerializer.Serialize(Data, new JsonSerializerOptions{ WriteIndented = true });
    public static Dictionary<string, object> FromJson(string JSON){
        JsonElement element = JsonSerializer.Deserialize<JsonElement>(JSON);
        return (Dictionary<string, object>)MapJsonElement(element)!;
    }
    
    private static object? MapJsonElement(JsonElement element) {
        switch (element.ValueKind) {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object>();
                foreach (var prop in element.EnumerateObject()) {
                    dict[prop.Name] = MapJsonElement(prop.Value)!;
                }
                return dict;
            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in element.EnumerateArray()) {
                    list.Add(MapJsonElement(item)!);
                }
                return list;
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                // Возвращаем double, Convert.ToSingle/ToInt32 позже справятся
                if (element.TryGetInt64(out long l)) return l;
                return element.GetDouble();
            case JsonValueKind.True: return true;
            case JsonValueKind.False: return false;
            case JsonValueKind.Null: return null;
            default: return null;
        }
    }
    
    // ----------------------------------------------------------------------

    public static Dictionary<string, object> Pack(object Anonymous){
        if(Anonymous is Dictionary<string, object?> Dictionary){ return Pack(Dictionary); }

        Dictionary<string, object> Result = new Dictionary<string, object>();
        Type Type = Anonymous.GetType();
        
        foreach(PropertyInfo Property in Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)){
            Result[Property.Name] = Serialize(Property.GetValue(Anonymous));
        }
        
        foreach(FieldInfo Field in Type.GetFields(BindingFlags.Public | BindingFlags.Instance)){
            Result[Field.Name] = Serialize(Field.GetValue(Anonymous));
        }
        return Result;
    }
    
    public static Dictionary<string, object> Pack(Dictionary<string, object?> RawData){
        Dictionary<string, object> Result = new Dictionary<string, object>();
        foreach(KeyValuePair<string, object?> KVP in RawData){
            Result[KVP.Key] = Serialize(KVP.Value);
        }
        return Result;
    }
}