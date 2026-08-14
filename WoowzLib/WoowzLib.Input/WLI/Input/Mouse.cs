using WLO;
using WLI;

namespace WLI_Input;

public interface Mouse : Input{
    bool IsButtonDown(Button Button);
    Vector2I Position{ get; }
    Vector2I Delta{ get; }

    public enum Button{
        Unknown = -1,
    
        Left   = 0,
        Right  = 1,
        Middle = 2,
    
        Button1 = 0,
        Button2 = 1,
        Button3 = 2,
        Button4 = 3,
        Button5 = 4
    }
}