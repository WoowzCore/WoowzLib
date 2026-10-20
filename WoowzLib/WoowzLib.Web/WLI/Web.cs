namespace WLI;

public interface Web : WLI.WindowElement{
    void Navigate(string URL);
    void NavigateCustom(string HTML);

    void PostMessage(string JSON);

    event Action<string>? OnMessage;
}