using Silk.NET.Vulkan;
using WLO;

namespace WL;

public static class Vulkan{
    public static void CheckResult(Result Received, string ExtraMessage = "*Нет дополнительной информации*", Result Target = Result.Success){
        if(Received != Target){
            throw new ExceptionWL($"Значения Vulkan Result [{Received}] и [{Target}] не равны!\nДополнительно: {ExtraMessage}");
        }
    }
}