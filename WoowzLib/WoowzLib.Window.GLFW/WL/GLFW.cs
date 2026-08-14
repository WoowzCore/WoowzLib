using Silk.NET.GLFW;
using WLO;

namespace WL;

public static partial class GLFW{
    public static readonly Glfw API = Glfw.GetApi();
    private static         bool __IsInitialized;

    public static bool Ready => __IsInitialized;
    
    public static bool Start(){
        try{
            if(!__IsInitialized){
                if(!API.Init()){
                    throw new ExceptionWL("WL.GLFW.API.Init() вернуло false! Не инициализировало GLFW!");
                }

                __IsInitialized = true;

                return true;
            }

            return false;
        }
        catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при запуске GLFW!\nWL.GLFW.Start()!", e);
        }
    }

    public static bool Stop(){
        if(__IsInitialized){
            API.Terminate();
            __IsInitialized = false;

            return true;
        }
        return false;
    }

    /**
     * Вызывает Stop(), если все окна закрыты
     */
    public static void MaybeStop(){
        // todo
    }
}