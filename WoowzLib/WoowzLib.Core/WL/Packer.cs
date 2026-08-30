using System.Collections;
using System.Reflection;
using WLI;

namespace WL;

public struct Packer{
    public static string PackType = "PackType";

    private static readonly Dictionary<Type, Type> __Fallbacks = [];

    public static void SetFallback(Type Type, Type FallbackType){ __Fallbacks[Type] = FallbackType; }
    
    public static object? Pack(object? Object){
        if(Object == null){ return null; }

        if(Object is WLI.Packable Packable){
            Dictionary<string, object?> RawData = Packable.__Pack();

            Dictionary<string, object?> Result = new Dictionary<string, object?>();
            foreach(KeyValuePair<string, object?> KVP in RawData){
                Result[KVP.Key] = Pack(KVP.Value);
            }
            
            Result[PackType] = ToCustomType(Object.GetType());
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
            foreach(object? i in List){ Result.Add(Pack(i)); }
            return Result;
        }

        return Object;
    }

    public static object? Unpack(object? Data, Type? TargetType = null){
        if(Data == null){ return null; }

        if(Data is Dictionary<string, object?> Dictionary){
            Type? Type = null;
            
            if(Dictionary.TryGetValue(PackType, out object? TypeName)){
                Type = FromCustomType(TypeName!.ToString()!);
            }
            
            if(Type == null && TargetType != null){
                foreach(KeyValuePair<Type, Type> KVP in __Fallbacks){
                    if(KVP.Key.IsAssignableFrom(TargetType)){
                        Type = KVP.Value;
                        break;
                    }
                }
            }

            if(Type == null && TargetType != null && typeof(WLI.Packable).IsAssignableFrom(TargetType) && ! TargetType.IsAbstract && !TargetType.IsInterface){
                Type = Nullable.GetUnderlyingType(TargetType) ?? TargetType;
            }
            
            if(Type != null && typeof(WLI.Packable).IsAssignableFrom(Type)){
                try{
                    Packable Instance = (WLI.Packable)Activator.CreateInstance(Type, true)!;
                    Instance.__Unpack(Dictionary);
                    return Instance;
                }catch (Exception e){
                    Console.WriteLine($"todo, Ошибка создания экземпляра {Type.Name}: {e.Message}");
                }
            }

            if(TargetType != null && typeof(WLI.Packable).IsAssignableFrom(TargetType)){ return null; }
        }

        if(Data is IList List && TargetType != null && TargetType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(TargetType)){
            Type ElementType = TargetType.IsArray ? TargetType.GetElementType()! : TargetType.GetGenericArguments()[0];
            IList Result = TargetType.IsArray ? Array.CreateInstance(ElementType, List.Count) : (IList)Activator.CreateInstance(TargetType)!;

            for(int i = 0; i < List.Count; i++){
                object? Value = Unpack(List[i], ElementType);

                if(Value != null && ElementType.IsInstanceOfType(Value!)){
                    if(TargetType.IsArray){ Result[i] = Value; }else{ Result.Add(Value); }
                }else{
                    Console.WriteLine($"todo, Пропущен элемент списка: не удалось привести {Value?.GetType().Name ?? "null"} к {ElementType.Name}");
                }
            }
            return Result;
        }

        if(TargetType != null && TargetType != Data.GetType()){ try { return Convert.ChangeType(Data, Nullable.GetUnderlyingType(TargetType) ?? TargetType); } catch { return Data; } }

        return Data;
    }

    public static void Unpack(WLI.Packable Target, Dictionary<string, object?> Data) => Target.__Unpack(Data);

    public static T? Get<T>(Dictionary<string, object?> Data, string Key, T DefaultValue = default!, bool Raw = false){
        if(!Data.TryGetValue(Key, out object? Value) || Value == null!){ return DefaultValue; }
        if(Raw){
            return (T)Value;
        }

        object? Result = Unpack(Value, typeof(T));

        if(Result != null && !(Result is T)){
            Console.WriteLine($"todo, Ошибка Get<{typeof(T).Name}>: получено {Result.GetType().Name}");
            return DefaultValue;
        }

        return (T)Result!;
    }

    // todo, remove that
    [Obsolete]
    private static Type? FindType(string Name){
        Type? Type = Type.GetType(Name);
        if(Type != null){ return Type; }

        string SimpleName = Name.Contains(',') ? Name.Split(',')[0].Trim() : Name;
        
        foreach(Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies()){
            Type = Assembly.GetType(SimpleName);
            if(Type != null){ return Type; }

            Type = Assembly.GetTypes().FirstOrDefault(T => T.FullName?.Replace('+', '.') == SimpleName);
            if(Type != null){ return Type; }
        }
        return null;
    }
    
    // ----------------------------------------------------------------------

    public static string ToCustomType(Type Type){
        string AssemblyName = Type.Assembly.GetName().Name!;

        string BaseName = Type.FullName!.Replace('+', '.');
        
        if(!Type.IsGenericType){
            return $"{AssemblyName}|{BaseName}";
        }

        if(BaseName.Contains('`')){ BaseName = BaseName[..BaseName.IndexOf('`')]; }
        
        return $"{AssemblyName}|{BaseName}({string.Join(", ", Type.GetGenericArguments().Select(ToCustomType))})";
    }

    public static Type? FromCustomType(string CustomType){
        if(string.IsNullOrEmpty(CustomType)){ return null; }

        int PipeIndex = CustomType.IndexOf('|');
        if(PipeIndex == -1){ return FindType(CustomType); }

        string AssemblyName = CustomType.Substring(0, PipeIndex);
        string TypePart = CustomType.Substring(PipeIndex + 1);

        if(!TypePart.Contains('(')){
            return FindTypeInAssembly(AssemblyName, TypePart);
        }

        int OpenAngle  = TypePart.IndexOf('(');
        int CloseAngle = TypePart.LastIndexOf(')');
        string BaseTypeName = TypePart.Substring(0, OpenAngle);
        string ArgsPart = TypePart.Substring(OpenAngle + 1, CloseAngle - OpenAngle - 1);

        List<string> ArgsStrings = __GenericSplit(ArgsPart, ", ");
        List<Type> GenericArgs = [];
        foreach(string ArgString in ArgsStrings){
            Type? ArgType = FromCustomType(ArgString);
            if(ArgType != null){ GenericArgs.Add(ArgType); }
        }

        string ClearBaseName = $"{BaseTypeName}`{GenericArgs.Count}";
        Type? BaseType = FindTypeInAssembly(AssemblyName, ClearBaseName);

        try{
            return BaseType?.MakeGenericType(GenericArgs.ToArray());
        }catch{
            return null;
        }
    }

    // todo, перенести в WL.String
    private static List<string> __GenericSplit(string Input, string Delimiter){
        List<string> Result = [];

        int Depth = 0;
        int Start = 0;
        int DelimiterL = Delimiter.Length;

        for(int i = 0; i < Input.Length; i++){
            if(Input[i] == '('){ Depth++; }
            else if(Input[i] == ')'){ Depth--; }
            else if(Depth == 0 && i <= Input.Length - DelimiterL){
                if(Input.Substring(i, DelimiterL) == Delimiter){
                    Result.Add(Input.Substring(Start, i - Start).Trim());
                    Start = i + DelimiterL;
                    i += DelimiterL - 1;
                }
            }
        }
        
        Result.Add(Input.Substring(Start).Trim());
        
        return Result;
    }
    
    private static Type? FindTypeInAssembly(string AssemblyName, string TypeFullName){
        foreach(Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies()){
            if(Assembly.GetName().Name == AssemblyName){
                Type? Type = Assembly.GetType(TypeFullName);
                if(Type != null){ return Type; }
            }
        }
        
        return FindType(TypeFullName);
    }
}