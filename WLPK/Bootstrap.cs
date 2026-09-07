using System.Diagnostics;

namespace WLPK;

public static class Bootstrap{
    public static int Main(string[] Args){
        try{
            if(Args.Length < 2){
                WL.Logger.Info("Использование: WLPK <Папка_цель> <Выходной_файл>");
                return 1;
            }

            string TargetFolder = Args[0];
            string OutputFile   = Args[1];

            Stopwatch SW = Stopwatch.StartNew();

            WL.Archive.Pack(TargetFolder, OutputFile);
            
            SW.Stop();

            FileInfo FileInfo = new FileInfo(OutputFile);
            WL.Logger.Info("Упаковка завершена успешно!");
            WL.Logger.Info($"Время выполнения: {SW.ElapsedMilliseconds}ms");
            WL.Logger.Info($"Размер файла: {FileInfo.Length / 1024:F2} КБ");
            
            return 0;
        }catch(Exception e){
            WL.Logger.Fatal("Произошла ошибка при работе WLPK!", e);
            return 1;
        }
    }
}