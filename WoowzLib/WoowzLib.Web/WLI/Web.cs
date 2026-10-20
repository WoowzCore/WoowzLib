namespace WLI;

public interface Web : WLI.WindowElement{
    void Navigate(string URL);
    void NavigateToString(string HTML);

    void ExecuteScript(string Script);
    void PostMessage(string JSON);

    event Action<string>? OnMessage;
}