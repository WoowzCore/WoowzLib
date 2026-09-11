using WLG;

namespace WLGenerator;

public static class Bootstrap{
    public static int Main(string[] Args){
        try{
            WL.Core.Start(Args);
            
            const string ResultPath = "W:\\Other\\WoowzLib\\WLGenerator\\Out";
            const string DebugPath  = "W:\\Other\\WoowzLib\\WLGenerator\\Out\\Debug";
            
            Gen_Math.Generate(ResultPath, DebugPath);
            
            return 0;
        }catch(Exception e){
            WL.Logger.Fatal("Произошла ошибка при работе WLGenerator!", e);
            return 1;
        }
    }

    public static string GenerateComment(string Name) => $"\n\tКласс {Name} сгенерирован с помощью WLGenerator\n\tСгенерирован: {DateTime.Now:yyyy.MM.dd HH:mm:ss}\n";
}