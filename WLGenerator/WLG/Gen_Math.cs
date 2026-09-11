using WLG.Math;
using WLGO;
using WLO;

namespace WLG;

public static class Gen_Math{
    public static void Generate(string ResultPath, string DebugPath){
        try{
            ResultPath = Path.Combine(ResultPath, "Math");
            DebugPath  = Path.Combine(DebugPath , "Math");
            WL.Logger.Info($"Генерация Math: {ResultPath}");
            
            Directory.CreateDirectory(ResultPath);
            Directory.CreateDirectory(DebugPath );

            List<CSResult> Result = [];
            
            Result.AddRange(Gen_Vector.Generate("WLO.Math"));
            
            foreach(CSResult CSR in Result){
                string FilePath = Path.Combine(DebugPath, $"{CSR.FileName}.cs");
                File.WriteAllText(FilePath, CSR.RawContent);
                
                FilePath = Path.Combine(ResultPath, $"{CSR.FileName}.cs");
                File.WriteAllText(FilePath, CSR.Content);
            }
        }catch(Exception e){
            throw new ExceptionWL("Ошибка при генерации Math!", e);
        }
    }
}