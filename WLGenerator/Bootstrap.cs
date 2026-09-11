using WLG;

namespace WLGenerator;

public static class Bootstrap{
    public static int Main(string[] Args){
        try{
            const string ResultPath = "W:\\Other\\WoowzLib\\WLGenerator\\Out";
            const string DebugPath  = "W:\\Other\\WoowzLib\\WLGenerator\\Out\\Debug";
            
            Gen_Math.Generate(ResultPath, DebugPath);
            
            return 0;
        }catch(Exception e){
            WL.Logger.Fatal("Произошла ошибка при работе WLGenerator!", e);
            return 1;
        }
    }
}