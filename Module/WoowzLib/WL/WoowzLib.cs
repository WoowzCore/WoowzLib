using WLO;

/// <summary>
/// Основа WoowzLib
/// </summary>
public static partial class WL{
    
    /// <summary>
    /// Взаимодействие с WoowzLib
    /// </summary>
    public static partial class WoowzLib{
        public static readonly Project Info = Project
            .Create()
            .Name("WoowzLib")
            .Version(0)
            .Author("Woowz11")
            .Description("НАПОМНИТЕ ИЗМЕНИТЬ В БУДУЩЕМ")
            .URL("https://github.com/WoowzCore/WoowzLib")
            .License("см. README.md")
            .Build();
    }
}