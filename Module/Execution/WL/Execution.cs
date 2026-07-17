using System.Data.SqlTypes;

public static partial class WL{
    
    /// <summary>
    /// Взаимодействие с WoowzLib.Execution
    /// </summary>
    public static partial class Execution{
        /// <summary>
        /// Скомпилировано в DEBUG режиме?
        /// </summary>
        #if DEBUG
            public const bool CompiledInDebug = true;
        #else
            public const bool CompiledInDebug = false;
        #endif
        
        /// <summary>
        /// Скомпилировано в RELEASE режиме?
        /// </summary>
        #if RELEASE
            public const bool CompiledInRelease = true;
        #else
            public const bool CompiledInRelease = false;
        #endif
        
        // Короче надо сделать что-бы можно было изменять debug и все остальные дела
    }
}