using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WL;

// TODO, насрал кода, нужно норм сделать тут всё
// ХУЛИ РАБОТА С МАССИВАМИ В СТРОКАХ, НЕ ДЕЛО!

public struct String{
    /// todo, ОБЪЕДЕНЯЕТ ЗНАЧЕНИЯ В СТРОКУ, РАЗДЕЛЯЯ ИХ МЕЖДУ СОБОЙ SEPARATOR, (["a","b","c"], ", ") => "a, b, c"
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join<T>(IEnumerable<T> Items, string Separator = ", ") => string.Join(Separator, Items);

    /// todo, ОБЪЕДЕНЯЕТ ЗНАЧЕНИЯ В СТРОКУ, (["a","b","c"]) => "abc"
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Concat<T>(IEnumerable<T> Items) => string.Concat(Items);

    /// todo, ПОВТОРЯЕТ СТРОКУ, ("a", 5, "+") => "a+a+a+a+a"
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Repeat(string Value, int Count, string Separator = "") => Join(Enumerable.Repeat(Value, Count), Separator);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] FormatAll<T>(IEnumerable<T> Items, string Format) => Items.Select((X, I) => string.Format(Format, X, I)).ToArray();

    /// todo, ДОБАВЛЯЕТ SEPARATOR МЕЖДУ ЭЛЕМЕНТАМИ
    public static T[] InterLeave<T>(IEnumerable<T> Items, T Separator){
        List<T> Result = [];
        foreach(T Item in Items){
            if(Result.Count > 0){ Result.Add(Separator); }
            Result.Add(Item);
        }
        return Result.ToArray();
    }

    /// todo, ОБЪЕДЕНЯЕТ 2 МАССИВА (["a","b"], ["1","2"]) => ["a","1","b","2"]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] ZipPairwise(IEnumerable<string> First, IEnumerable<string> Second) => First.Zip(Second, (A, B) => new[]{ A, B }).SelectMany(X => X).ToArray();

    /// todo, ДИАПАЗОН ОТ From ДО To
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<int> Range(int From, int To) => Enumerable.Range(From, To - From + 1);

    /// todo, ПРОЕКЦИЯ ДИАПАЗОНА (1, 3, i => $"P{i}") => ["P1","P2","P3"]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] RangeMap(int From, int To, Func<int, string> Selector) => Range(From, To).Select(Selector).ToArray();

    /// todo, ДОПОЛНЯЕТ МАССИВ ДО ДЛИНЫ TOTAL (["a","b"], 4, "0") => ["a","b","0","0"]
    public static string[] PadRight(string[] Items, int Total, string Fill){
        if(Items.Length >= Total){ return Items; }

        string[] Result = new string[Total];
        Array.Copy(Items, Result, Items.Length);
        for(int i = Items.Length; i < Total; i++){
            Result[i] = Fill;
        }
        return Result;
    }

    /// TODO, Обрезает массив (["a","b","c","d","f"], 3) => ["a","b","c"]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Take<T>(T[] Items, int Count) => Items.Take(Count).ToArray();

    // todo, хз что за функция
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] Skip(string[] Items, int Count) => Items.Skip(Count).ToArray();
    
    /// todo, СОЗДАЁТ МАССИВ С ОДНИМ ЗНАЧЕНИЕМ ОПРЕДЕЛЁННОГО КОЛ-ВО
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] RepeatArray<T>(T Value, int Count) => Enumerable.Repeat(Value, Count).ToArray();

    /// todo, ОБЪЕДЕНЯЕТ МАССИВЫ (["a","b"], ["1","2"]) => ["a","b","1","2"]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] ConcatArrays(params string[][] Arrays) => Arrays.SelectMany(X => X).ToArray();
    
    // ----------------------------------------------------------------------
    
    public static string ToJSON(object? Object){
        string JSON = JsonSerializer.Serialize(Tag(Packer.Pack(Object)), new JsonSerializerOptions{ WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping  });
        
        return Regex.Replace(JSON, @"(?m)^(  )+", M => new string('\t', M.Length / 2));
    }

    // ВОЗВРАЩАЕТ СЫРЫЕ ДАННЫЕ, ДАЛЕЕ НУЖНО WL.Packabilizer.Unpack()
    public static object? FromJSON(string JSON){
        if(string.IsNullOrEmpty(JSON)){ return null; }

        object? MapJSON(JsonElement JE) => JE.ValueKind switch{
            JsonValueKind.Object => JE.EnumerateObject().ToDictionary(P => P.Name, P => MapJSON(P.Value)),
            JsonValueKind.Array  => JE.EnumerateArray().Select(MapJSON).ToList(),
            JsonValueKind.String => JE.GetString(),
            JsonValueKind.True   => true,
            JsonValueKind.False  => false,
            JsonValueKind.Null   => null,
            var _ => JE.ToString()
        };
        
        return Untag(MapJSON(JsonSerializer.Deserialize<JsonElement>(JSON)));
    }

    public static object? Tag(object? Value){
        if(Value == null){ return null; }

        if(Value is Dictionary<string, object> Dictionary){
            Dictionary<string, object> Result = new Dictionary<string, object>();
            foreach(KeyValuePair<string, object> KVP in Dictionary){
                Result[KVP.Key] = Tag(KVP.Value)!;
            }

            return Result;
        }

        if(Value is List<object> List){
            return List.Select(Tag).ToList();
        }

        if(Value.GetType().IsEnum){
            return "X" + WL.Packer.ToCustomType(Value.GetType()) + ":" + Value.ToString();
        }

        return Value switch{
            int  I => "I" + I,
            uint A => "A" + A,
            
            byte  B => "B" + B,
            sbyte E => "E" + E,
            
            short  S => "S" + S,
            ushort G => "G" + G,
            
            long  L => "L" + L,
            ulong H => "H" + H,
            
            float   F => "F" + F.ToString(CultureInfo.InvariantCulture),
            double  D => "D" + D.ToString(CultureInfo.InvariantCulture),
            decimal P => "P" + P.ToString(CultureInfo.InvariantCulture),
            
            bool Z => "Z" + (Z ? 1 : 0),
            
            string T => "T" + T,
            
            char C => "C" + C,
            
            nint  N => "N" + N,
            nuint M => "M" + M,
            var   _ => "?" + Value.ToString()
        };
    }

    public static object? Untag(object? Value){
        if(Value == null){ return null; }

        if(Value is Dictionary<string, object> Dictionary){
            Dictionary<string, object> Result = new Dictionary<string, object>();
            foreach(KeyValuePair<string, object> KVP in Dictionary){
                Result[KVP.Key] = Untag(KVP.Value)!;
            }
            return Result;
        }

        if(Value is List<object> List){
            return List.Select(Untag).ToList();
        }

        if(Value is string S && S.Length >= 1){
            char Prefix = S[0];
            string Body = S.Substring(1);

            object ParseEnum(string Body){
                try{
                    int ColonIndex = Body.LastIndexOf(':');
                    if(ColonIndex == -1){ return Body; }

                    string TypePart = Body.Substring(0, ColonIndex);
                    string ValuePart = Body.Substring(ColonIndex + 1);

                    Type? EnumType = WL.Packer.FromCustomType(TypePart);

                    if(EnumType == null && TypePart.Contains('.')){
                        int LastDot = TypePart.LastIndexOf('.');
                        string CorrectName = TypePart.Remove(LastDot, 1).Insert(LastDot, "+");
                        EnumType = WL.Packer.FromCustomType(CorrectName);
                    }
                    
                    if(EnumType != null && EnumType.IsEnum){
                        return Enum.Parse(EnumType, ValuePart);
                    }

                    return ValuePart;
                }catch{
                    return Body;
                }
            }
            
            return Prefix switch{
                'I' => int.Parse(Body),
                'A' => uint.Parse(Body),
                
                'B' => byte.Parse(Body),
                'E' => sbyte.Parse(Body),
                
                'S' => short.Parse(Body),
                'G' => ushort.Parse(Body),
                
                'L' => long.Parse(Body),
                'H' => ulong.Parse(Body),
                
                'F' => float.Parse(Body, CultureInfo.InvariantCulture),
                'D' => double.Parse(Body, CultureInfo.InvariantCulture),
                'P' => decimal.Parse(Body, CultureInfo.InvariantCulture),
                
                'Z' => Body == "1",
                
                'T' => Body,
                
                'C' => Body.Length > 0 ? Body[0] : '\0',
                
                'N' => nint.Parse(Body),
                'M' => nuint.Parse(Body),
                
                'X' => ParseEnum(Body),
                
                '?' => Body,
                var _ => S
            };
        }

        return Value;
    }
}