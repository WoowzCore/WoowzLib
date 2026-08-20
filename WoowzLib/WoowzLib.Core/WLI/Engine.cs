namespace WLI;

public interface Engine{
    bool IsStarted{ get; }
    
    void Start();
    bool Stop ();
}