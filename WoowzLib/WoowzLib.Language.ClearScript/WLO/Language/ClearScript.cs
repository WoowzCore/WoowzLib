using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using WLI;

namespace WLOLanguage;

public class ClearScript : Language<WLOLanguageContext.ClearScript>{
    public string ID => "clearscript";
    
    public WLOLanguageContext.ClearScript CreateContext() => new WLOLanguageContext.ClearScript(this);

    LanguageContext Language.CreateContext() => CreateContext();

    public void Dispose(){}
}