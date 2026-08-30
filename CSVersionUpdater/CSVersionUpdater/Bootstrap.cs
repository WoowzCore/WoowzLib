using System.Diagnostics;
using System.Text.RegularExpressions;
using WLO;

public static class Bootstrap{
    private static readonly Regex VersionRegex = new Regex(@"<Version>(.*?)</Version>", RegexOptions.IgnoreCase);
    
    public static int Main(string[] Args){
        try{
            WL.Core.ProjectInfo = new ProjectInfo("CSVersionUpdater",  Author: "Woowz11");
            
            void Work(){
                if(Args.Length == 0){ WL.Logger.Info("Использование: CSVersionUpdater <Путь до *.csproj>\n(Требует что-бы файл был в GIT репозитории, для детекта изменений)"); return; }

                string ProjectPath     = Path.GetFullPath(Args[0]);
                
                if(!File.Exists(ProjectPath)){ throw new Exception($"Проект не найден по пути: {ProjectPath}"); }

                if(!HasGITChanges(Path.GetDirectoryName(ProjectPath) ?? "", Path.GetFileName(ProjectPath))){
                    WL.Logger.Info("Изменений не обнаружено");
                    return;
                }
                
                UpdateVersion(ProjectPath);
            }
            Work();
            
            return 0;
        }catch(Exception e){
            WL.Logger.Fatal($"Произошла ошибка при работе CSVersionUpdater!\n{e.Message}\n{e.StackTrace}");
            return 1;
        }
    }

    private static bool HasGITChanges(string WorkDirectory, string CSPROJName){
        try{
            ProcessStartInfo PSI = new ProcessStartInfo{
                FileName = "git",
                Arguments = "status --porcelain .",
                WorkingDirectory = WorkDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? Process = System.Diagnostics.Process.Start(PSI);
            if(Process == null){ throw new Exception("Не удалось запустить GIT, проверьте, что добавлен ли он в PATH"); }

            string Output = Process.StandardOutput.ReadToEnd();
            string Error  = Process.StandardError .ReadToEnd();
            Process.WaitForExit();

            if(Process.ExitCode != 0){ throw new Exception($"GIT вернул ошибку, возможно папка не является репозиторием\n{Error.Trim()}"); }

            if(string.IsNullOrWhiteSpace(Output)){ return false; }

            foreach(string Line in Output.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)){
                if(Line.Length < 4){ continue; }

                string FilePath = Line.Substring(3).Trim();
                if(!FilePath.Equals(CSPROJName, StringComparison.OrdinalIgnoreCase)){
                    return true;
                }
            }
            
            return false;
        }catch(Exception e){
            WL.Logger.Error($"Ошибка при проверке GIT: {e.Message}\n{e.StackTrace}");
            return true;
        }
    }

    private static void UpdateVersion(string ProjectPath){
        string Content = File.ReadAllText(ProjectPath);
        Match Match = VersionRegex.Match(Content);
                
        string Old;
        string New;

        if(!Match.Success){
            Old = "Не найдено";
            New = "0.0.0";

            if(Content.Contains("<PropertyGroup>")){
                Content = Content.Replace("<PropertyGroup>", $"<PropertyGroup>\n\t\t<Version>{New}</Version>");
            }else{
                throw new Exception("Не удалось найти версию, и найти <PropertyGroup>, что-бы создать её!");
            }
        }else{
            Old = Match.Groups[1].Value.Trim();
            string[] Parts = Old.Split('.');

            int LastIndex = Parts.Length - 1;
            if(int.TryParse(Parts[LastIndex], out int LastPart)){
                Parts[LastIndex] = (LastPart + 1).ToString();
            }else{
                throw new Exception($"Неверный формат версии: {Old}");
            }

            New = string.Join('.', Parts);
            Content = VersionRegex.Replace(Content, $"<Version>{New}</Version>");
        }
                
        File.WriteAllText(ProjectPath, Content);
        WL.Logger.Info($"Версия успешно обновлена! [{Old}] -> [{New}]");
    }
}