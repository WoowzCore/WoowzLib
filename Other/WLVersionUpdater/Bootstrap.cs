using WLO;

namespace WLVU{
    public class Bootstrap{
        public static readonly Project ProjectInfo = Project
            .Create()
            .Name("WoowzLib Version Updater")
            .Version(0)
            .Author("Woowz11")
            .Description("Обновляет версии у проектов WoowzLib при компиляции")
            .URL("")
            .License("см. README.md")
            .Build();
        
        public static int Main(string[] Args){
            try{
                Console.WriteLine("привет");
                
                return 0;
            }catch(Exception e){
                throw new WLException("MEGA FAIL", e);
            }
        }
    }
}