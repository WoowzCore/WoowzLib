using System.Globalization;
using WLGenerator;
using WLGO;
using WLO;

namespace WLG.Math;

public static class Gen_Vector{
    private static string Namespace;
    
    public static List<CSResult> Generate(string Namespace__){
        Namespace = Namespace__;
        
        List<CSResult> Result = [];

        foreach(Type Type in Enum.GetValues<Type>()){
            for(int Count = 2; Count <= 4; Count++){
                Result.Add(GenerateVector(Type, Count));
            }
        }
        
        return Result;
    }
    
    // ----------------------------------------------------------------------
    
    public static CSResult GenerateVector(Type TypeRaw, int Count){
        string Name = VectorName(TypeRaw, Count);

        try{
            WL.Logger.Info($"Генерация Vector: {Name}");

            string Type = GetType(TypeRaw);
            string[] AvailableAxes = WL.String.RangeMap(1, Count, GetAxis);
            string Spread = WL.String.Concat(AvailableAxes);
            string Vector128 = $"Vector128<{Type}>";
            
            CSResult Result = new CSResult{ FileName = Name };

            CSGenerator Gen = new CSGenerator();

            #region Генерация
            
                Gen.Space();
                Gen.Comment(Bootstrap.GenerateComment(Name));
                Gen.Space();

                Gen.AddUsing("System.Runtime.Intrinsics");
                
                Gen.Namespace(Namespace);

                Gen.Space();
                
                Gen.Struct(CSGenerator.E_AM_Declaration.Public, Name, $"IEquatable<{Name}>, WLI.Packable", () => {

                    void GenerateFields(){
                        for(int i = 1; i <= Count; i++){
                            Gen.AddField(CSGenerator.E_AM.Public, Type, GetAxis(i));
                        }

                        if(Count < 4){
                            Gen.Space(2);
                        
                            for(int i = 1; i <= Count; i++){
                                Gen.AddProperty(CSGenerator.E_AM.Public, Type, GetSize(i), $"{{ get => {GetAxis(i)}; set => {GetAxis(i)} = value; }}");
                            }
                        }
                        
                        Gen.Space(2);
                        
                        for(int i = 1; i <= Count; i++){
                            Gen.AddProperty(CSGenerator.E_AM.Public, Type, GetColor(i), $"{{ get => {GetAxis(i)}; set => {GetAxis(i)} = value; }}");
                        }
                        
                        Gen.Space(2);

                        void __Gen(int Count__, string[] __Axes){
                            if(Count__ > Count){ return; }
                            
                            string[] Axes__ = WL.String.Take(__Axes, Count__);
                        
                            for(int Start = 0; Start < Count; Start++){
                                string[] Combo = new string[Count__];
                                for(int i = 0; i < Count__; i++){
                                    Combo[i] = __Axes[(Start + i) % Count];
                                }

                                string Vector__ = VectorName(TypeRaw, Count__);
                                string Name__ = WL.String.Concat(Combo);

                                if(Count__ == Count && Combo.SequenceEqual(Axes__)){ continue; }
                            
                                Gen.AddProperty(CSGenerator.E_AM.Public, Vector__, Name__, $"=> new {Vector__}({WL.String.Join(Combo)})");
                            }
                        }

                        void __Gen2(string[] __Axes){
                            __Gen(2, __Axes);
                            __Gen(3, __Axes);
                            __Gen(4, __Axes);
                            
                            for(int AxisIndex = 1; AxisIndex <= Count; AxisIndex++){
                                string AxisChar = __Axes[AxisIndex - 1];

                                for(int Size = 2; Size <= Count; Size++){
                                    string Name__ = WL.String.Repeat(AxisChar, Size);
                                    string Vector__ = VectorName(TypeRaw, Size);
                                    string Args = WL.String.Repeat(AxisChar, Size, ", ");
                                        
                                    Gen.AddProperty(CSGenerator.E_AM.Public, Vector__, Name__, $"=> new {Vector__}({Args})");
                                }
                            }
                        }
                        __Gen2(Axes);
                        __Gen2(Colors);
                    }
                    GenerateFields();
                    
                    Gen.Space(2);

                    void GenerateConstructors(){
                        string[] CtorParams = WL.String.ZipPairwise(
                            Enumerable.Repeat(Type, Count),
                            AvailableAxes    
                        );
                        
                        Gen.AddConstructor(CSGenerator.E_AM.Public, CtorParams, $"{{ {WL.String.Concat(WL.String.FormatAll(AvailableAxes, "this.{0} = {0};"))} }}");
                        Gen.AddConstructor(CSGenerator.E_AM.Public, [Type, Spread], $"this({WL.String.Repeat(Spread, Count, ", ")})", "{}");
                        for(int SubCount = 2; SubCount < Count; SubCount++){
                            int TailCount = Count - SubCount;

                            if(TailCount == 1){
                                string SubVector = VectorName(TypeRaw, SubCount);
                                string[] SubAxes = WL.String.Take(Axes, SubCount);
                                
                                Gen.AddConstructor(CSGenerator.E_AM.Public, [SubVector, "Vector", Type, GetAxis(Count)], $"this({WL.String.Join(WL.String.ConcatArrays(WL.String.FormatAll(SubAxes, "Vector.{0}"),[GetAxis(Count)]))})", "{}");
                            }else if(TailCount == SubCount){
                                string SubVector = VectorName(TypeRaw, SubCount);
                                string[] SubAxes = WL.String.Take(Axes, SubCount);
                                string[] TailAxes = WL.String.Take(WL.String.Skip(Axes, SubCount), SubCount);
                                
                                Gen.AddConstructor(CSGenerator.E_AM.Public, [SubVector, "VectorA", SubVector, "VectorB"], $"this({WL.String.Join(WL.String.ConcatArrays(WL.String.FormatAll(SubAxes, "VectorA.{0}"), WL.String.FormatAll(SubAxes, "VectorB.{0}")))})", "{}");
                            }
                        }
                        if(Count == 4){
                            Gen.AddConstructor(CSGenerator.E_AM.Public, [Type, "X", Type, "Y", Type, "Z"], "this(X, Y, Z, 1)", "{}"); Gen.Comment("<- Типо цвет");
                        }
                    }
                    GenerateConstructors();

                    Gen.Space(2);
                    
                    void GenerateSameTypeConverts(){
                        void __Gen(int Count__){
                            if(Count__ == Count){ return; }
                            string __VectorName = VectorName(TypeRaw, Count__);
                            string[] Axes__ = WL.String.PadRight(WL.String.Take(Axes, WL.Math.MinI(Count, Count__)), Count__, "0");
                            Gen.AddFunction(CSGenerator.E_AM.Public, __VectorName, $"To{Count__}{TypeSymbol(TypeRaw)}", [], $"=> new {__VectorName}({(WL.String.Join(Axes__))})");
                        }
                        __Gen(2);
                        __Gen(3);
                        __Gen(4);
                        
                        Gen.Space();
                        
                        if(TypeRaw != Gen_Vector.Type.Double){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, $"{Vector128}", "ToSIMD", [], $"=> Vector128.Create({WL.String.Join(WL.String.PadRight(AvailableAxes, 4, "0"))})");
                        }
                    }
                    GenerateSameTypeConverts();

                    Gen.Space(2);
                    
                    void GenerateConstants(){
                        Dictionary<string, (int MinCount, float X, float Y, float Z, float W)> Constants = new Dictionary<string, (int, float, float, float, float)>{
                            {"Zero", (0, 0, 0, 0, 0)},
                            {"One", (0, 1, 1, 1, 1)},
                            {"MOne", (0, -1, -1, -1, -1)},
                            {"Half", (0, 0.5f, 0.5f, 0.5f, 0.5f)},
                            {"MHalf", (0, -0.5f, -0.5f, -0.5f, -0.5f)},
                            {"Right", (2, 1, 0, 0, 0)},
                            {"Left", (2, -1, 0, 0, 0)},
                            {"AxisX", (2, 1, 0, 0, 0)},
                            {"AxisMX", (2, -1, 0, 0, 0)},
                            {"Up", (2, 0, 1, 0, 0)},
                            {"Down", (2, 0, -1, 0, 0)},
                            {"AxisY", (2, 0, 1, 0, 0)},
                            {"AxisMY", (2, 0, -1, 0, 0)},
                            {"Front", (3, 0, 0, 1, 0)},
                            {"Back", (3, 0, 0, -1, 0)},
                            {"FrontGL", (3, 0, 0, -1, 0)},
                            {"AxisZ", (3, 0, 0, 1, 0)},
                            {"AxisMZ", (3, 0, 0, -1, 0)},
                            {"Ana", (4, 0, 0, 0, 1)},
                            {"Kata", (4, 0, 0, 0, -1)},
                            {"AxisW", (4, 0, 0, 0, 1)},
                            {"AxisMW", (4, 0, 0, 0, -1)},
                            {"MaxValue", (0, float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue)},
                            {"MinValue", (0, float.MinValue, float.MinValue, float.MinValue, float.MinValue)},
                            {"NAN", (0, float.NaN, float.NaN, float.NaN, float.NaN)}
                        };

                        bool IsFractal = TypeRaw is Gen_Vector.Type.Float or Gen_Vector.Type.Double;

                        string FormatConst(float Value){
                            if(float.IsNaN(Value)){ return $"WL.Math.NAN{TypeSymbol(TypeRaw)}"; }
                            if(Value == float.MaxValue){ return $"WL.Math.MaxValue{TypeSymbol(TypeRaw)}"; }
                            if(Value == float.MinValue){ return $"WL.Math.MinValue{TypeSymbol(TypeRaw)}"; }
                            
                            string Num = Value.ToString(CultureInfo.InvariantCulture);

                            return Num + (TypeRaw != Gen_Vector.Type.Float || MathF.Round(Value) == Value ? "" : "f");
                        }
                        
                        foreach((string ConstName, (int MinCount, float X, float Y, float Z, float W) Const) in Constants){
                            if(Const.MinCount > Count){ continue; }
                            
                            if(!IsFractal && ConstName is "Half" or "MHalf" or "NAN"){ continue; }

                            float[] Values = [Const.X, Const.Y, Const.Z, Const.W];
                            
                            Gen.AddProperty(CSGenerator.E_AM.PublicStatic, Name, ConstName, $"=> new {Name}({WL.String.Join(WL.String.Take(Values, Count).Select(FormatConst))})");
                        }
                    }
                    GenerateConstants();
                    
                    Gen.Separator();
                    
                    void GenerateOperators(){
                        void __Gen(string Operator, string Func, string FuncShort){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, Name, FuncShort, [Name, "B"], $"{{{{ this = this {Operator} B; return this; }}}}");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, Name, FuncShort, [Type, "B"], $"{{{{ this = this {Operator} B; return this; }}}}");
                            
                            if(Count == 4 && TypeRaw != Gen_Vector.Type.Double){
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, $"operator {Operator}", [Name, "A", Name, "B"], $"{{ {Vector128} Result = Vector128.{Func}(A.ToSIMD(), B.ToSIMD()); return new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, "Result.GetElement({1})"))}); }}");
                            
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, $"operator {Operator}", [Name, "A", Type, "B"], $"{{ {Vector128} Result = Vector128.{Func}(A.ToSIMD(), Vector128.Create(B)); return new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, "Result.GetElement({1})"))}); }}");
                            }else{
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, $"operator {Operator}", [Name, "A", Name, "B"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"A.{{0}} {Operator} B.{{0}}"))})");
                            
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, $"operator {Operator}", [Name, "A", Type, "B"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"A.{{0}} {Operator} B"))})");
                            }
                        }
                        __Gen("+", "Add", "Add");
                        __Gen("-", "Subtract", "Sub");
                        __Gen("*", "Multiply", "Mul");
                        __Gen("/", "Divide", "Div");
                        
                        Gen.Space(2);
                        
                        Gen.AddProperty(CSGenerator.E_AM.Public, Type, "this[int Index]", $"{{ get => Index switch{{ {WL.String.Join(WL.String.FormatAll(AvailableAxes, "{1} => {0}"), ", ")}, var _ => throw new IndexOutOfRangeException() }}; set{{ switch(Index){{ {WL.String.Concat(WL.String.FormatAll(AvailableAxes, "case {1}: {0} = value; break;"))} default: throw new IndexOutOfRangeException(); }} }} }}");
                        
                        Gen.Space(2);
                        
                        void __GenDec(int Count__){
                            if(Count__ > Count){ return; }
                            Gen.AddAttributeAggressiveInlining();
                            string[] Axes__ = WL.String.Take(Axes, Count__);
                            Gen.AddFunction(CSGenerator.E_AM.Public, "Deconstruct", WL.String.ZipPairwise(WL.String.RepeatArray($"out {Type}", Count__), Axes__), $"{{ {WL.String.Concat(WL.String.FormatAll(Axes__, "{0} = this.{0};"))} }}");
                        }
                        __GenDec(2);
                        __GenDec(3);
                        __GenDec(4);
                    }
                    GenerateOperators();
                    
                    Gen.Separator();

                    void GenerateFunctions(){
                        void __GenLength(){
                            Gen.AddProperty(CSGenerator.E_AM.Public, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "LengthSquared", $"{{ {Gen.GetAttributeAggressiveInlining()} get => WL.Math.LengthSquared{Count}{TypeSymbol(TypeRaw)}({WL.String.Join(AvailableAxes)}); }}");
                            Gen.AddProperty(CSGenerator.E_AM.Public, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "Length", $"{{ {Gen.GetAttributeAggressiveInlining()} get => WL.Math.Length{Count}{TypeSymbol(TypeRaw)}({WL.String.Join(AvailableAxes)}); }}");
                        }
                        __GenLength();

                        Gen.Space(2);
                        
                        void __GenNormal(){
                            if(TypeRaw == Gen_Vector.Type.Int){ return; }
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, Name, "Normalize", [], "{{ this = Normalized; return this; }}");
                            Gen.AddProperty(CSGenerator.E_AM.Public, Name, "Normalized", $"{{ {Gen.GetAttributeAggressiveInlining()} get{{ {(TypeRaw == Gen_Vector.Type.Double ? "double" : "float")} L = Length; return L > WL.Math.Epsilon{TypeSymbol(TypeRaw)} ? this / L : {Name}.Zero; }} }}");
                        }
                        __GenNormal();

                        Gen.Space(2);

                        void __GenDistance(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "DistanceSquared", [Name, "B"], $"=> WL.Math.DistanceSquared{Count}{TypeSymbol(TypeRaw)}({WL.String.Join(AvailableAxes)}, {WL.String.Join(WL.String.FormatAll(AvailableAxes, "B.{0}"))})");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "Distance", [Name, "B"], $"=> WL.Math.Distance{Count}{TypeSymbol(TypeRaw)}({WL.String.Join(AvailableAxes)}, {WL.String.Join(WL.String.FormatAll(AvailableAxes, "B.{0}"))})");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "DistanceSquared", [Name, "A", Name, "B"], $"=> A.DistanceSquared(B)");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "Distance", [Name, "A", Name, "B"], $"=> A.Distance(B)");
                        }
                        __GenDistance();
                        
                        Gen.Space(2);
                        
                        void __GenLerp(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, Name, "Lerp", [Name, "B", TypeRaw == Gen_Vector.Type.Double ? "double" : "float", "T"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"WL.Math.Lerp{TypeSymbol(TypeRaw)}({{0}}, B.{{0}}, T)"))})");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, Name, "LerpSafe", [Name, "B", TypeRaw == Gen_Vector.Type.Double ? "double" : "float", "T"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"WL.Math.LerpSafe{TypeSymbol(TypeRaw)}({{0}}, B.{{0}}, T)"))})");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, "Lerp", [Name, "A", Name, "B", TypeRaw == Gen_Vector.Type.Double ? "double" : "float", "T"], "=> A.Lerp(B, T)");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, "LerpSafe", [Name, "A", Name, "B", TypeRaw == Gen_Vector.Type.Double ? "double" : "float", "T"], "=> A.LerpSafe(B, T)");
                        }
                        __GenLerp();
                    }
                    GenerateFunctions();
                    
                    Gen.Separator();

                    void GenerateOther(){
                        void GeneratePackUnpack(){
                            Gen.AddFunction(CSGenerator.E_AM.Public, "Dictionary<string, object?>", "__Pack", [], $"=> new Dictionary<string, object?>{{ [\"{Spread}\"] = $\"{{{WL.String.Join(AvailableAxes, "}|{")}}}\" }}");
                            Gen.Space();
                            Gen.AddFunction(CSGenerator.E_AM.Public, "__Unpack", ["Dictionary<string, object?>", "Data"], $"{{ string {Spread} = WL.Packer.Get<string>(Data, \"{Spread}\", \"{WL.String.Join(WL.String.RepeatArray("0", Count), "|")}\")!; {Gen.GetSpace()} string[] Parts = {Spread}.Split('|'); if(Parts.Length >= {Count}){{ {WL.String.Concat(WL.String.FormatAll(AvailableAxes, $"{Type}.TryParse(Parts[{{1}}], out {{0}});"))} }} }}");
                        }
                        GeneratePackUnpack();

                        Gen.Space(2);
                        
                        void GenerateEquals(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, "bool", "Equals", [Name, "Other"], $"=> {WL.String.Join(WL.String.FormatAll(AvailableAxes, "{0} == Other.{0}"), " && ")}");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicOverride, "bool", "Equals", ["object?", "Object"], $"=> Object is {Name} Other && Equals(Other)");
                            Gen.Space();
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, "bool", "operator ==", [Name, "Left", Name, "Right"], "=> Left.Equals(Right)");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, "bool", "operator !=", [Name, "Left", Name, "Right"], "=> !(Left == Right)");
                        }
                        GenerateEquals();

                        Gen.Space(2);

                        void GenerateToString(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, "string", "ToShortString", [], $"=> $\"{WL.String.Join(WL.String.FormatAll(AvailableAxes, "{{{0}}}"))}\"");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicOverride, "string", "ToString", [], $"=> $\"{Name}({{ToShortString()}})\"");
                        }
                        GenerateToString();
                        
                        Gen.Space(2);
                        
                        void GenerateOtherOther(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicOverride, "int", "GetHashCode", [], $"=> HashCode.Combine({WL.String.Join(AvailableAxes)})");
                        }
                        GenerateOtherOther();
                    }
                    GenerateOther();
                    
                });
                
            #endregion

            Result.RawContent = Gen.Build();
            Result.Content = CSGenerator.Refract(Result.RawContent);
            return Result;
        }catch(Exception e){
            throw new ExceptionWL($"Произошла ошибка при генерации {Name}!", e);
        }
    }
    
    // ----------------------------------------------------------------------
    
    public enum Type{ Int, Float, Double /* todo, мб и больше в будущем */ }
    public static string GetType(Type Type) => Type switch{
        Type.Int => "int",
        Type.Float => "float",
        Type.Double => "double"
    }; 
    
    public static string TypeSymbol(Type Type) => Type switch{
        Type.Int    => "I",
        Type.Float  => "F",
        Type.Double => "D",
    };

    public static string VectorName(Type Type, int Count) => $"Vector{Count}{TypeSymbol(Type)}";

    public static readonly string[] Axes  = [ "X", "Y", "Z", "W" ];
    public static string GetAxis(int Index) => Axes[Index - 1];
    public static readonly string[] Sizes = [ "W", "H", "D" ];
    public static string GetSize(int Index) => Sizes[Index - 1];
    public static readonly string[] Colors = [ "R", "G", "B", "A" ];
    public static string GetColor(int Index) => Colors[Index - 1];
}