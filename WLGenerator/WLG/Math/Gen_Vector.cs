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
            
            CSResult Result = new CSResult{ FileName = Name };

            CSGenerator Gen = new CSGenerator();

            #region Генерация

                Gen.Namespace(Namespace);

                Gen.Space();
                
                Gen.Class(CSGenerator.E_AM_Declaration.Public, Name, $"IEquatable<{Name}>, WLI.Packable", () => {

                    void GenerateFields(){
                        for(int i = 1; i <= Count; i++){
                            Gen.AddField(CSGenerator.E_AM.Public, Type, GetAxis(i));
                        }

                        if(Count < 4){
                            Gen.Space();
                        
                            for(int i = 1; i <= Count; i++){
                                Gen.AddProperty(CSGenerator.E_AM.Public, Type, GetSize(i), $"{{ get => {GetAxis(i)}; set => {GetAxis(i)} = value; }}");
                            }
                        }
                    }
                    GenerateFields();
                    
                    Gen.Space();

                    void GenerateConstructors(){
                        string[] AxisNames = WL.String.RangeMap(1, Count, GetAxis);
                        string[] CtorParams = WL.String.ZipPairwise(
                            Enumerable.Repeat(Type, Count),
                            AxisNames    
                        );

                        string Spread = WL.String.Concat(AxisNames);
                        
                        Gen.AddConstructor(CSGenerator.E_AM.Public, CtorParams, $"{{ {WL.String.Concat(WL.String.FormatAll(AxisNames, "this.{0} = {0};"))} }}");
                        Gen.AddConstructor(CSGenerator.E_AM.Public, [Type, Spread], $"this({WL.String.Repeat(Spread, Count, ", ")})", "{}");
                    }
                    GenerateConstructors();
                    
                });
                
            #endregion

            Result.RawContent = Gen.ToString();
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
}