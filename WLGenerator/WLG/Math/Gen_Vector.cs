using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
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
            string Vector128 = $"Vector{(TypeRaw == Gen_Vector.Type.Double ? "256" : "128")}";
            string Vector128T = $"{Vector128}<{Type}>";
            bool UseSIMD = Count == 4;
            string UnsafeAsTo128 = $"Unsafe.As<{Name}, {Vector128T}>";
            string UnsafeAsFrom128 = $"Unsafe.As<{Vector128T}, {Name}>";
            
            CSResult Result = new CSResult{ FileName = Name };

            CSGenerator Gen = new CSGenerator();

            #region Генерация
            
                Gen.Space();
                Gen.Comment(Bootstrap.GenerateComment(Name));
                Gen.Space();

                Gen.AddUsing("System.Runtime.Intrinsics");
                
                Gen.Namespace(Namespace);

                Gen.Space();
                
                Gen.AddAttribute("System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)");
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

                        if(Count == 3){
                            Gen.Space(2);
                        
                            for(int i = 1; i <= Count; i++){
                                Gen.AddProperty(CSGenerator.E_AM.Public, Type, GetRotate(i), $"{{ get => {GetAxis(i)}; set => {GetAxis(i)} = value; }}");
                            }
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
                            
                                Gen.AddProperty(CSGenerator.E_AM.Public, Vector__, Name__, $"{{ {Gen.GetAttributeAggressiveInlining()} get => new {Vector__}({WL.String.Join(Combo)}); }}");
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
                                        
                                    Gen.AddProperty(CSGenerator.E_AM.Public, Vector__, Name__, $"{{ {Gen.GetAttributeAggressiveInlining()} get => new {Vector__}({Args}); }}");
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
                        if(Count == 4 && TypeRaw != Gen_Vector.Type.Int){
                            Gen.AddConstructor(CSGenerator.E_AM.Public, [Type, "X", Type, "Y", Type, "Z"], "this(X, Y, Z, 1)", "{}"); Gen.Comment("<- Типо цвет");
                        }
                    }
                    GenerateConstructors();

                    Gen.Space(2);
                    
                    void GenerateTypeConverts(){
                        void __Gen(int Count__){
                            if(Count__ == Count){ return; }
                            string __VectorName = VectorName(TypeRaw, Count__);
                            string[] Axes__ = WL.String.PadRight(WL.String.Take(Axes, WL.Math.MinI(Count, Count__)), Count__, "0");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, __VectorName, $"To{Count__}{TypeSymbol(TypeRaw)}", [], $"=> new {__VectorName}({(WL.String.Join(Axes__))});");
                        }
                        __Gen(2);
                        __Gen(3);
                        __Gen(4);
                        
                        Gen.Space();
                        
                        Gen.AddAttributeAggressiveInlining();
                        Gen.AddFunction(CSGenerator.E_AM.Public, $"{Vector128T}", "ToSIMD", [], Count == 4 ? $"=> {UnsafeAsTo128}(ref Unsafe.AsRef(in this));" : $"=> {Vector128}.Create({WL.String.Join(WL.String.PadRight(AvailableAxes, 4, "0"))});");
                        
                        Gen.Space();
                        
                        Gen.AddAttributeAggressiveInlining();
                        Gen.AddFunction(CSGenerator.E_AM.PublicStatic, "implicit", $"operator {Name}", [$"System.Numerics.Vector{Count}", "A"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"{(TypeRaw == Gen_Vector.Type.Int ? "(int)" : "")}A.{{0}}"))});");
                        Gen.AddAttributeAggressiveInlining();
                        Gen.AddFunction(CSGenerator.E_AM.PublicStatic, "implicit", $"operator System.Numerics.Vector{Count}", [Name, "A"], $"=> new System.Numerics.Vector{Count}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"{(TypeRaw == Gen_Vector.Type.Double ? "(float)" : "")}A.{{0}}"))});");
                    }
                    GenerateTypeConverts();

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
                            
                            Gen.AddProperty(CSGenerator.E_AM.PublicStatic, Name, ConstName, $"{{ {Gen.GetAttributeAggressiveInlining()} get => new {Name}({WL.String.Join(WL.String.Take(Values, Count).Select(FormatConst))}); }}");
                        }
                    }
                    GenerateConstants();
                    
                    Gen.Separator();
                    
                    void GenerateOperators(){
                        void __Gen(string Operator, string Func){
                            if(UseSIMD){
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, $"operator {Operator}", [Name, "A", Name, "B"], $"{{ {Vector128T} Result = {Vector128}.{Func}(A.ToSIMD(), B.ToSIMD()); return {UnsafeAsFrom128}(ref Result); }}");
                            
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, $"operator {Operator}", [Name, "A", Type, "B"], $"{{ {Vector128T} Result = {Vector128}.{Func}(A.ToSIMD(), {Vector128}.Create(B)); return {UnsafeAsFrom128}(ref Result); }}");
                            }else{
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, $"operator {Operator}", [Name, "A", Name, "B"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"A.{{0}} {Operator} B.{{0}}"))});");
                            
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, $"operator {Operator}", [Name, "A", Type, "B"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"A.{{0}} {Operator} B"))});");
                            }
                        }
                        __Gen("+", "Add");
                        __Gen("-", "Subtract");
                        __Gen("*", "Multiply");
                        __Gen("/", "Divide");
                        
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
                            
                            Gen.Space(2);
                        }
                        __GenLength();
                        
                        void __GenNormal(){
                            if(TypeRaw == Gen_Vector.Type.Int){ return; }
                            Gen.AddProperty(CSGenerator.E_AM.Public, Name, "Normalize", $"{{ {Gen.GetAttributeAggressiveInlining()} get{{ {(TypeRaw == Gen_Vector.Type.Double ? "double" : "float")} L = Length; return L > WL.Math.Epsilon{TypeSymbol(TypeRaw)} ? this / L : {Name}.Zero; }} }}");
                            
                            Gen.Space(2);
                        }
                        __GenNormal();

                        void __GenNegative(){
                            if(Count == 4 && TypeRaw != Gen_Vector.Type.Double){
                                Gen.AddProperty(CSGenerator.E_AM.Public, Name, "Negative", $"{{ {Gen.GetAttributeAggressiveInlining()} get{{ {Vector128T} Result = {Vector128}.Negate(this.ToSIMD()); return {UnsafeAsFrom128}(ref Result); }} }}");
                            }else{
                                Gen.AddProperty(CSGenerator.E_AM.Public, Name, "Negative", $"{{ {Gen.GetAttributeAggressiveInlining()} get => this * -1; }}");
                            }
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, "operator -", [Name, "A"], "=> A.Negative;");
                            
                            Gen.Space(2);
                        }
                        __GenNegative();

                        void __GenDistance(){
                            if(UseSIMD){
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.Public, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "DistanceSquared", [Name, "B"], $"{{ {Vector128T} Diff = {Vector128}.Subtract(this.ToSIMD(), B.ToSIMD()); return {Vector128}.Dot(Diff, Diff); }}");
                            }else{
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.Public, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "DistanceSquared", [Name, "B"], $"=> WL.Math.DistanceSquared{Count}{TypeSymbol(TypeRaw)}({WL.String.Join(AvailableAxes)}, {WL.String.Join(WL.String.FormatAll(AvailableAxes, "B.{0}"))});");   
                            }
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "Distance", [Name, "B"], $"=> WL.Math.Distance{Count}{TypeSymbol(TypeRaw)}({WL.String.Join(AvailableAxes)}, {WL.String.Join(WL.String.FormatAll(AvailableAxes, "B.{0}"))});");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "DistanceSquared", [Name, "A", Name, "B"], $"=> A.DistanceSquared(B);");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "Distance", [Name, "A", Name, "B"], $"=> A.Distance(B);");
                            
                            Gen.Space(2);
                        }
                        __GenDistance();
                        
                        void __GenDot(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, Type, "Dot", [Name, "B"], UseSIMD ? $"=> {Vector128}.Dot(this.ToSIMD(), B.ToSIMD());" : $"=> WL.Math.Dot{Count}{TypeSymbol(TypeRaw)}({WL.String.Join(AvailableAxes)}, {WL.String.Join(WL.String.FormatAll(AvailableAxes, "B.{0}"))});");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Type, "Dot", [Name, "A", Name, "B"], "=> A.Dot(B);");
                            
                            Gen.Space(2);
                        }
                        __GenDot();
                        
                        void __GenCross(){
                            if(Count == 3){
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.Public, Name, "Cross", [Name, "B"], $"=> new {Name}((Y*B.Z) - (Z*B.Y), (Z*B.X) - (X*B.Z), (X*B.Y) - (Y*B.X));");
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, "Cross", [Name, "A", Name, "B"], "=> A.Cross(B);");   
                            }else if(Count == 2){
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.Public, Type, "Cross", [Name, "B"], $"=> WL.Math.Cross{TypeSymbol(TypeRaw)}({WL.String.Join(AvailableAxes)}, {WL.String.Join(WL.String.FormatAll(AvailableAxes, "B.{0}"))});");
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Type, "Cross", [Name, "A", Name, "B"], "=> A.Cross(B);");
                            }else{
                                return;
                            }

                            Gen.Space(2);
                        }
                        __GenCross();
                        
                        void __GenLerp(){
                            if(UseSIMD && TypeRaw != Gen_Vector.Type.Int){
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.Public, Name, "Lerp", [Name, "B", TypeRaw == Gen_Vector.Type.Double ? "double" : "float", "T"], $"{{ {Vector128T} Result = {Vector128}.Add(this.ToSIMD(), {Vector128}.Multiply({Vector128}.Subtract(B.ToSIMD(), this.ToSIMD()), {Vector128}.Create(T))); return {UnsafeAsFrom128}(ref Result); }}");
                            }else{
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.Public, Name, "Lerp", [Name, "B", TypeRaw == Gen_Vector.Type.Double ? "double" : "float", "T"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"WL.Math.Lerp{TypeSymbol(TypeRaw)}({{0}}, B.{{0}}, T)"))});");
                            }
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, Name, "LerpSafe", [Name, "B", TypeRaw == Gen_Vector.Type.Double ? "double" : "float", "T"], $"=> Lerp(B, WL.Math.Clamp01{TypeSymbol(TypeRaw == Gen_Vector.Type.Int ? Gen_Vector.Type.Float : TypeRaw)}(T));");   
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, "Lerp", [Name, "A", Name, "B", TypeRaw == Gen_Vector.Type.Double ? "double" : "float", "T"], "=> A.Lerp(B, T);");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, "LerpSafe", [Name, "A", Name, "B", TypeRaw == Gen_Vector.Type.Double ? "double" : "float", "T"], "=> A.LerpSafe(B, T);");
                            
                            Gen.Space(2);
                        }
                        __GenLerp();

                        void __GenMinMax(){
                            void __Gen(string Func){
                                if(UseSIMD){
                                    Gen.AddAttributeAggressiveInlining();
                                    Gen.AddFunction(CSGenerator.E_AM.Public, Name, Func, [Name, "B"], $"{{ {Vector128T} Result = {Vector128}.{Func}(this.ToSIMD(), B.ToSIMD()); return {UnsafeAsFrom128}(ref Result); }}");
                                    Gen.AddAttributeAggressiveInlining();
                                    Gen.AddFunction(CSGenerator.E_AM.Public, Name, Func, [Type, "B"], $"{{ {Vector128T} Result = {Vector128}.{Func}(this.ToSIMD(), {Vector128}.Create(B)); return {UnsafeAsFrom128}(ref Result); }}");
                                }else{
                                    Gen.AddAttributeAggressiveInlining();
                                    Gen.AddFunction(CSGenerator.E_AM.Public, Name, Func, [Name, "B"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"WL.Math.{Func}{TypeSymbol(TypeRaw)}({{0}}, B.{{0}})"))});");
                                    Gen.AddAttributeAggressiveInlining();
                                    Gen.AddFunction(CSGenerator.E_AM.Public, Name, Func, [Type, "B"], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"WL.Math.{Func}{TypeSymbol(TypeRaw)}({{0}}, B)"))});");
                                }

                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, Func, [Name, "A", Name, "B"], $"=> A.{Func}(B);");
                                Gen.AddAttributeAggressiveInlining();
                                Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, Func, [Name, "A", Type, "B"], $"=> A.{Func}(B);");
                            }
                            __Gen("Min");
                            Gen.Space(2);
                            __Gen("Max");
                            
                            Gen.Space(2);
                        }
                        __GenMinMax();

                        void __GenClamp(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, Name, "Clamp", [Name, "Min", Name, "Max"], "=> this.Min(Max).Max(Min);");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, Name, "Clamp", [Type, "Min", Type, "Max"], "=> this.Min(Max).Max(Min);");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, "Clamp", [Name, "A", Name, "Min", Name, "Max"], "=> A.Clamp(Min, Max);");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, Name, "Clamp", [Name, "A", Type, "Min", Type, "Max"], "=> A.Clamp(Min, Max);");
                            
                            Gen.Space(2);
                        }
                        __GenClamp();

                        void __GenOther(){
                            if(Count == 2){
                                Gen.AddProperty(CSGenerator.E_AM.Public, TypeRaw == Gen_Vector.Type.Int ? "float" : Type, "Aspect", $"{{ {Gen.GetAttributeAggressiveInlining()} get => WL.Math.Aspect{TypeSymbol(TypeRaw)}(W, H); }}");
                                Gen.Space(2);
                            }
                        }
                        __GenOther();
                        
                        void __GenFloorCeilRound(){
                            if(TypeRaw == Gen_Vector.Type.Int){ return; }

                            void __Gen(string Func, string Func2, bool DontSIMD = false){
                                if(UseSIMD && !DontSIMD){
                                    Gen.AddAttributeAggressiveInlining();   
                                    Gen.AddFunction(CSGenerator.E_AM.Public, Name, Func, [], $"{{ {Vector128T} Result = {Vector128}.{Func2}(this.ToSIMD()); return {UnsafeAsFrom128}(ref Result); }}");
                                }else{
                                    Gen.AddAttributeAggressiveInlining();   
                                    Gen.AddFunction(CSGenerator.E_AM.Public, Name, Func, [], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"WL.Math.{Func}{TypeSymbol(TypeRaw)}({{0}})"))});");
                                }
                            }
                            __Gen("Floor", "Floor");
                            __Gen("Round", "Round", true);
                            __Gen("Ceil", "Ceiling");
                            
                            Gen.Space(2);
                        }
                        __GenFloorCeilRound();
                        
                        void __GenAbs(){
                            if(UseSIMD){
                                Gen.AddAttributeAggressiveInlining();   
                                Gen.AddFunction(CSGenerator.E_AM.Public, Name, "Abs", [], $"{{ {Vector128T} Result = {Vector128}.Abs(this.ToSIMD()); return {UnsafeAsFrom128}(ref Result); }}");
                            }else{
                                Gen.AddAttributeAggressiveInlining();   
                                Gen.AddFunction(CSGenerator.E_AM.Public, Name, "Abs", [], $"=> new {Name}({WL.String.Join(WL.String.FormatAll(AvailableAxes, $"WL.Math.Abs{TypeSymbol(TypeRaw)}({{0}})"))});");
                            }
                        }
                        __GenAbs();
                    }
                    GenerateFunctions();
                    
                    Gen.Separator();

                    void GenerateOther(){
                        void GeneratePackUnpack(){
                            Gen.AddFunction(CSGenerator.E_AM.Public, "Dictionary<string, object?>", "__Pack", [], $"=> new Dictionary<string, object?>{{ [\"{Spread}\"] = $\"{{{WL.String.Join(AvailableAxes, "}|{")}}}\" }};");
                            Gen.Space();
                            Gen.AddFunction(CSGenerator.E_AM.Public, "__Unpack", ["Dictionary<string, object?>", "Data"], $"{{ string {Spread} = WL.Packer.Get<string>(Data, \"{Spread}\", \"{WL.String.Join(WL.String.RepeatArray("0", Count), "|")}\")!; {Gen.GetSpace()} string[] Parts = {Spread}.Split('|'); if(Parts.Length >= {Count}){{ {WL.String.Concat(WL.String.FormatAll(AvailableAxes, $"{Type}.TryParse(Parts[{{1}}], out {{0}});"))} }} }}");
                        }
                        GeneratePackUnpack();

                        Gen.Space(2);
                        
                        void GenerateEquals(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, "bool", "Equals", [Name, "Other"], $"=> {WL.String.Join(WL.String.FormatAll(AvailableAxes, "{0} == Other.{0}"), " && ")};");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicOverride, "bool", "Equals", ["object?", "Object"], $"=> Object is {Name} Other && Equals(Other);");
                            Gen.Space();
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, "bool", "operator ==", [Name, "Left", Name, "Right"], "=> Left.Equals(Right);");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicStatic, "bool", "operator !=", [Name, "Left", Name, "Right"], "=> !(Left == Right);");
                        }
                        GenerateEquals();

                        Gen.Space(2);

                        void GenerateToString(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.Public, "string", "ToShortString", [], $"=> $\"{WL.String.Join(WL.String.FormatAll(AvailableAxes, "{{{0}}}"))}\";");
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicOverride, "string", "ToString", [], $"=> $\"{Name}({{ToShortString()}})\";");
                        }
                        GenerateToString();
                        
                        Gen.Space(2);
                        
                        void GenerateOtherOther(){
                            Gen.AddAttributeAggressiveInlining();
                            Gen.AddFunction(CSGenerator.E_AM.PublicOverride, "int", "GetHashCode", [], $"=> HashCode.Combine({WL.String.Join(AvailableAxes)});");
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
    public static readonly string[] Rotations = ["Pitch", "Yaw", "Roll"];
    public static string GetRotate(int Index) => Rotations[Index - 1];
}