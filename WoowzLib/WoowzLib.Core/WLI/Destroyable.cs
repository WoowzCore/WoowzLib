namespace WLI;

public interface Destroyable : IDisposable{
    bool Destroyed{ get; protected set; }

    void OnDestroy();

    void Destroy(){
        if(Destroyed){ return; }
        OnDestroy();
        Destroyed = true;
    }

    void Dispose() => Destroy();
}