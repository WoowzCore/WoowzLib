using WLI;

namespace WLOLanguage;

public class JS : Language<WLOLanguageContext.JS>{
    public string ID => "js";

    public WLOLanguageContext.JS CreateContext() => new WLOLanguageContext.JS(this);

    LanguageContext Language.CreateContext() => CreateContext();

    public void Dispose(){}
}