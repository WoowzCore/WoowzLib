using System.Numerics;
using ImGuiNET;
using WLI_Input;
using WLO.Math;

using API = ImGuiNET.ImGui;

namespace WLO.Interface;

public abstract class ImGUI : WLI.Engine{
    public ImGuiIOPtr IO{ get; private set; }
    
    public bool IsStarted{ get; protected set; }
    
    public virtual void Start(){
        try{
            if(IsStarted){ throw new ExceptionWL("ImGUI уже и так был запущен!"); }
            
            API.CreateContext();
            IO = API.GetIO();

            unsafe{
                IO.NativePtr -> IniFilename = null; // todo, отключение imgui.ini сохранения
            }
            
            //IO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            
            API.StyleColorsDark();
            
            IsStarted = true;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при создании ImGUI!", e);
        }
    }
    
    public bool Stop(){
        try{
            if(!IsStarted){ return false; }

            IsStarted = false;
            return true;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при остановке ImGUI!", e);
        }
    }
    
    // ----------------------------------------------------------------------

    public void MousePosition(Vector2I Position){
        IO.AddMousePosEvent(Position.X, Position.Y);
    }

    public void MouseButton(WLI_Input.Mouse.Button Button, bool Down){
        IO.AddMouseButtonEvent(Button switch{
            Mouse.Button.Button1 => (int)ImGuiMouseButton.Left,
            Mouse.Button.Button2 => (int)ImGuiMouseButton.Right,
            Mouse.Button.Button3 => (int)ImGuiMouseButton.Middle,
            Mouse.Button.Button4 => 3,
            Mouse.Button.Button5 => 4,
            
            var _ => -1
        }, Down);
    }

    public void MouseScroll(Vector2F Delta){
        IO.AddMouseWheelEvent(Delta.X, Delta.Y);
    }

    public void KeyboardKey(WLI_Input.Keyboard.Key Key, bool Down){
        IO.AddKeyEvent(Key switch{
            Keyboard.Key.Unknown => ImGuiKey.None,
            Keyboard.Key.A => ImGuiKey.A,
            Keyboard.Key.B => ImGuiKey.B,
            Keyboard.Key.C => ImGuiKey.C,
            Keyboard.Key.D => ImGuiKey.D,
            Keyboard.Key.E => ImGuiKey.E,
            Keyboard.Key.F => ImGuiKey.F,
            Keyboard.Key.G => ImGuiKey.G,
            Keyboard.Key.H => ImGuiKey.H,
            Keyboard.Key.I => ImGuiKey.I,
            Keyboard.Key.J => ImGuiKey.J,
            Keyboard.Key.K => ImGuiKey.K,
            Keyboard.Key.L => ImGuiKey.L,
            Keyboard.Key.M => ImGuiKey.M,
            Keyboard.Key.N => ImGuiKey.N,
            Keyboard.Key.O => ImGuiKey.O,
            Keyboard.Key.P => ImGuiKey.P,
            Keyboard.Key.Q => ImGuiKey.Q,
            Keyboard.Key.R => ImGuiKey.R,
            Keyboard.Key.S => ImGuiKey.S,
            Keyboard.Key.T => ImGuiKey.T,
            Keyboard.Key.U => ImGuiKey.U,
            Keyboard.Key.V => ImGuiKey.V,
            Keyboard.Key.W => ImGuiKey.W,
            Keyboard.Key.X => ImGuiKey.X,
            Keyboard.Key.Y => ImGuiKey.Y,
            Keyboard.Key.Z => ImGuiKey.Z,
            Keyboard.Key.D0 => ImGuiKey._0,
            Keyboard.Key.D1 => ImGuiKey._1,
            Keyboard.Key.D2 => ImGuiKey._2,
            Keyboard.Key.D3 => ImGuiKey._3,
            Keyboard.Key.D4 => ImGuiKey._4,
            Keyboard.Key.D5 => ImGuiKey._5,
            Keyboard.Key.D6 => ImGuiKey._6,
            Keyboard.Key.D7 => ImGuiKey._7,
            Keyboard.Key.D8 => ImGuiKey._8,
            Keyboard.Key.D9 => ImGuiKey._9,
            Keyboard.Key.F1 => ImGuiKey.F1,
            Keyboard.Key.F2 => ImGuiKey.F2,
            Keyboard.Key.F3 => ImGuiKey.F3,
            Keyboard.Key.F4 => ImGuiKey.F4,
            Keyboard.Key.F5 => ImGuiKey.F5,
            Keyboard.Key.F6 => ImGuiKey.F6,
            Keyboard.Key.F7 => ImGuiKey.F7,
            Keyboard.Key.F8 => ImGuiKey.F8,
            Keyboard.Key.F9 => ImGuiKey.F9,
            Keyboard.Key.F10 => ImGuiKey.F10,
            Keyboard.Key.F11 => ImGuiKey.F11,
            Keyboard.Key.F12 => ImGuiKey.F12,
            Keyboard.Key.F13 => ImGuiKey.F13,
            Keyboard.Key.F14 => ImGuiKey.F14,
            Keyboard.Key.F15 => ImGuiKey.F15,
            Keyboard.Key.F16 => ImGuiKey.F16,
            Keyboard.Key.F17 => ImGuiKey.F17,
            Keyboard.Key.F18 => ImGuiKey.F18,
            Keyboard.Key.F19 => ImGuiKey.F19,
            Keyboard.Key.F20 => ImGuiKey.F20,
            Keyboard.Key.F21 => ImGuiKey.F21,
            Keyboard.Key.F22 => ImGuiKey.F22,
            Keyboard.Key.F23 => ImGuiKey.F23,
            Keyboard.Key.F24 => ImGuiKey.F24,
            Keyboard.Key.Escape => ImGuiKey.Escape,
            Keyboard.Key.Enter => ImGuiKey.Enter,
            Keyboard.Key.Space => ImGuiKey.Space,
            Keyboard.Key.Tab => ImGuiKey.Tab,
            Keyboard.Key.Backspace => ImGuiKey.Backspace,
            Keyboard.Key.Insert => ImGuiKey.Insert,
            Keyboard.Key.Delete => ImGuiKey.Delete,
            Keyboard.Key.PageUp => ImGuiKey.PageUp,
            Keyboard.Key.PageDown => ImGuiKey.PageDown,
            Keyboard.Key.Home => ImGuiKey.Home,
            Keyboard.Key.End => ImGuiKey.End,
            Keyboard.Key.CapsLock => ImGuiKey.CapsLock,
            Keyboard.Key.ScrollLock => ImGuiKey.ScrollLock,
            Keyboard.Key.NumLock => ImGuiKey.NumLock,
            Keyboard.Key.PrintScreen => ImGuiKey.PrintScreen,
            Keyboard.Key.Pause => ImGuiKey.Pause,
            Keyboard.Key.Left => ImGuiKey.LeftArrow,
            Keyboard.Key.Right => ImGuiKey.RightArrow,
            Keyboard.Key.Up => ImGuiKey.UpArrow,
            Keyboard.Key.Down => ImGuiKey.DownArrow,
            Keyboard.Key.ShiftL => ImGuiKey.LeftShift,
            Keyboard.Key.ShiftR => ImGuiKey.RightShift,
            Keyboard.Key.ControlL => ImGuiKey.LeftCtrl,
            Keyboard.Key.ControlR => ImGuiKey.RightCtrl,
            Keyboard.Key.AltL => ImGuiKey.LeftAlt,
            Keyboard.Key.AltR => ImGuiKey.RightAlt,
            Keyboard.Key.SuperL => ImGuiKey.LeftSuper,
            Keyboard.Key.SuperR => ImGuiKey.RightSuper,
            Keyboard.Key.Menu => ImGuiKey.Menu,
            Keyboard.Key.Grave => ImGuiKey.GraveAccent,
            Keyboard.Key.Minus => ImGuiKey.Minus,
            Keyboard.Key.Equal => ImGuiKey.Equal,
            Keyboard.Key.BracketL => ImGuiKey.LeftBracket,
            Keyboard.Key.BracketR => ImGuiKey.RightBracket,
            Keyboard.Key.Backslash => ImGuiKey.Backslash,
            Keyboard.Key.Semicolon => ImGuiKey.Semicolon,
            Keyboard.Key.Apostrophe => ImGuiKey.Apostrophe,
            Keyboard.Key.Comma => ImGuiKey.Comma,
            Keyboard.Key.Period => ImGuiKey.Period,
            Keyboard.Key.Slash => ImGuiKey.Slash,
            Keyboard.Key.Num0 => ImGuiKey.Keypad0,
            Keyboard.Key.Num1 => ImGuiKey.Keypad1,
            Keyboard.Key.Num2 => ImGuiKey.Keypad2,
            Keyboard.Key.Num3 => ImGuiKey.Keypad3,
            Keyboard.Key.Num4 => ImGuiKey.Keypad4,
            Keyboard.Key.Num5 => ImGuiKey.Keypad5,
            Keyboard.Key.Num6 => ImGuiKey.Keypad6,
            Keyboard.Key.Num7 => ImGuiKey.Keypad7,
            Keyboard.Key.Num8 => ImGuiKey.Keypad8,
            Keyboard.Key.Num9 => ImGuiKey.Keypad9,
            Keyboard.Key.NumDivide => ImGuiKey.KeypadDivide,
            Keyboard.Key.NumMultiply => ImGuiKey.KeypadMultiply,
            Keyboard.Key.NumSubtract => ImGuiKey.KeypadSubtract,
            Keyboard.Key.NumAdd => ImGuiKey.KeypadAdd,
            Keyboard.Key.NumEnter => ImGuiKey.KeypadEnter,
            Keyboard.Key.NumDecimal => ImGuiKey.KeypadDecimal,
            
            var _ => (int)ImGuiKey.None
        }, Down);
    }

    public void KeyboardKeys(WLI_Input.Keyboard.Key[] Keys, bool Down){
        foreach(WLI_Input.Keyboard.Key Key in Keys){
            KeyboardKey(Key, Down);
        }
    }

    public void KeyboardChar(char Char){
        IO.AddInputCharacter(Char);
    }
    
    // ----------------------------------------------------------------------
    
    public void FrameStart(float DT, Vector2I Viewport){
        IO.DeltaTime = DT;
        IO.DisplaySize = new Vector2(Viewport.X, Viewport.Y);
        
        API.NewFrame();
    }

    public void FrameEnd(){
        API.Render();
    }

    public void Render(){
        OnRender(API.GetDrawData());
    }

    protected abstract void OnRender(ImDrawDataPtr DrawData);
}