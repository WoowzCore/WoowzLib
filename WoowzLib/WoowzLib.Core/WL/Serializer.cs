using System.Collections;
using System.Reflection;
using System.Text.Json;
using WLI;

namespace WL;

// TODO, сделать параметр Deserialize включить параметр (bool) типо изменять значения на дефолтные если не найдено, и сделать удобнее
// а то писать каждый раз (data.TryGetValue("Transform", out var t) && t is Dictionary<string, object> td) не удобно!!!!

public static class Serializer{
    public const string __Type  = "__Type";
    public const string __Items = "__Items";
    public const string __Value = "__Value";
    
    public static Dictionary<string, object> Serialize(object? Object){
        if(Object == null){ return []; }
        
        if(Object is Serializable Serializable){ return Serializable.Serialize(); }
        
        if(Object is System.Collections.IEnumerable Enumerable and not string){
            List<object> List = [];
            foreach(object? Item in Enumerable){
                List.Add(Serialize(Item));
            }
            return new Dictionary<string, object>{
                [__Items] = List,
                [__Type] = "List"
            };
        }
        
        if(IsPrimitive(Object.GetType())){
            return new Dictionary<string, object>{
                [__Value] = Object,
                [__Type] = Object.GetType().FullName!
            };
        }
        
        return SerializeObject(Object);
    }

    public static object? Deserialize(Dictionary<string, object> Data, Type? TargetType = null){
        if(Data.Count == 0){ return null; }

        if(Data.TryGetValue(__Value, value: out object? Value) && Data.TryGetValue(__Type, out object? TypeName)){
            Type? Type = System.Type.GetType(TypeName.ToString()!);
            if(Type != null && IsPrimitive(Type)){
                return Convert.ChangeType(Value, Type);
            }
            return Value;
        }
        
        if(Data.TryGetValue(__Items, out object? Items) && Data.TryGetValue(__Type, out TypeName) && TypeName.ToString() == "List"){
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
        
        if(Data.TryGetValue("__Type", out TypeName)){
            Type? Type = System.Type.GetType(TypeName.ToString()!);
            if(Type != null && typeof(Serializable).IsAssignableFrom(Type)){
                Serializable? Instance = Activator.CreateInstance(Type) as Serializable;
                Instance?.Deserialize(Data);
                return Instance;
            }
            if(Type != null){
                return DeserializeObject(Data, Type);
            }
        }
        
        if(TargetType != null){
            return DeserializeObject(Data, TargetType);
        }
        
        return null;
    }

    public static Dictionary<string, object> SInt(int Value) => new Dictionary<string, object>{ [__Value] = Value, [__Type] = typeof(int).FullName! };
    public static Dictionary<string, object> SFloat(float Value) => new Dictionary<string, object>{ [__Value] = Value, [__Type] = typeof(float).FullName! };
    public static Dictionary<string, object> SDouble(double Value) => new Dictionary<string, object>{ [__Value] = Value, [__Type] = typeof(double).FullName! };
    public static Dictionary<string, object> SBool(bool Value) => new Dictionary<string, object>{ [__Value] = Value, [__Type] = typeof(bool).FullName! };
    public static Dictionary<string, object> SString(string Value) => new Dictionary<string, object>{ [__Value] = Value, [__Type] = typeof(string).FullName! };

    public static int DInt(Dictionary<string, object> Data) => Convert.ToInt32(Data[__Value]);
    public static float DFloat(Dictionary<string, object> Data) => Convert.ToSingle(Data[__Value]);
    public static double DDouble(Dictionary<string, object> Data) => Convert.ToDouble(Data[__Value]);
    public static bool DBool(Dictionary<string, object> Data) => Convert.ToBoolean(Data[__Value]);
    public static string DString(Dictionary<string, object> Data) => Data[__Value]?.ToString() ?? "";

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
}