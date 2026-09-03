using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WL;

public struct String{
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