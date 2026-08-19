using WLI;

namespace WLI_Input;

public interface Keyboard : Input{
    bool IsKeyDown(Key Key);
    bool IsKeyPressed(Key Key);

    event Action<Key, bool>? OnKey;
    event Action<char>? OnChar;
    
    public enum Key{
        Unknown = -1,
    
        // todo, 0 in empty....
        
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
        F = 6,
        G = 7,
        H = 8,
        I = 9,
        J = 10,
        K = 11,
        L = 12,
        M = 13,
        N = 14,
        O = 15,
        P = 16,
        Q = 17,
        R = 18,
        S = 19,
        T = 20,
        U = 21,
        V = 22,
        W = 23,
        X = 24,
        Y = 25,
        Z = 26,
    
        D0 = 27,
        D1 = 28,
        D2 = 29,
        D3 = 30,
        D4 = 31,
        D5 = 32,
        D6 = 33,
        D7 = 34,
        D8 = 35,
        D9 = 36,
    
        F1  = 37,
        F2  = 38,
        F3  = 39,
        F4  = 40,
        F5  = 41,
        F6  = 42,
        F7  = 43,
        F8  = 44,
        F9  = 45,
        F10 = 46,
        F11 = 47,
        F12 = 48,
        F13 = 49,
        F14 = 50,
        F15 = 51,
        F16 = 52,
        F17 = 53,
        F18 = 54,
        F19 = 55,
        F20 = 56,
        F21 = 57,
        F22 = 58,
        F23 = 59,
        F24 = 60,
    
        Escape      = 61,
        Enter       = 62,
        Space       = 63,
        Tab         = 64,
        Backspace   = 65,
        Insert      = 66,
        Delete      = 67,
        PageUp      = 68,
        PageDown    = 69,
        Home        = 70,
        End         = 71,
        CapsLock    = 72,
        ScrollLock  = 73,
        NumLock     = 74,
        PrintScreen = 75,
        Pause       = 76,
    
        Left  = 77,
        Right = 78,
        Up    = 79,
        Down  = 80,
        
        ShiftL   = 81,
        ShiftR   = 82,
        ControlL = 83,
        ControlR = 84,
        AltL     = 85,
        AltR     = 86,
        SuperL   = 87,
        SuperR   = 88,
        Menu     = 89,
        
        Grave      = 90,  // `
        Minus      = 91,  // -
        Equal      = 92,  // =
        BracketL   = 93,  // [
        BracketR   = 94,  // ]
        Backslash  = 95,  // \
        Semicolon  = 96,  // ;
        Apostrophe = 97,  // '
        Comma      = 98,  // ,
        Period     = 99,  // .
        Slash      = 100, // /
        
        Num0        = 101,
        Num1        = 102,
        Num2        = 103,
        Num3        = 104,
        Num4        = 105,
        Num5        = 106,
        Num6        = 107,
        Num7        = 108,
        Num8        = 109,
        Num9        = 110,
        NumDivide   = 111,
        NumMultiply = 112,
        NumSubtract = 113,
        NumAdd      = 114,
        NumEnter    = 115,
        NumDecimal  = 116
    }
}