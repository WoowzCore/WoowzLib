using System.Text.RegularExpressions;
using WLO;

public static class Bootstrap{
    private static readonly Regex VersionRegex = new Regex(@"<Version>(.*?)</Version>", RegexOptions.IgnoreCase);
    
    public static int Main(string[] Args){
        try{
            WL.Core.ProjectInfo = new ProjectInfo("CSVersionUpdater",  Author: "Woowz11");
            
            void Work(){
                if(Args.Length == 0){ WL.Logger.Info("Использование: CSVersionUpdater <Путь до *.csproj>"); return; }

                string ProjectPath = Path.GetFullPath(Args[0]);

                if(!File.Exists(ProjectPath)){ throw new Exception($"Проект не найден по пути: {ProjectPath}"); }

                string Content = File.ReadAllText(ProjectPath);
                Match Match = VersionRegex.Match(Content);
                
                string New;
                string Old;

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
            Work();
            
            return 0;
        }catch(Exception e){
            WL.Logger.Fatal($"Произошла ошибка при работе CSVersionUpdater!\n{e.Message}\n{e.StackTrace}");
            return 1;
        }
    }
}