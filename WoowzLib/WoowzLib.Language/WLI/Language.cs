namespace WLI;

public interface Language : IDisposable{
    string ID{ get; }

    LanguageContext CreateContext();
}

public interface Language<out C> : Language where C : LanguageContext{
    new C CreateContext();
}