using System.Diagnostics;
using System.Runtime.CompilerServices;
using WLO;

namespace WL;

public static partial class Thread{
    public static bool LimitByDeltaTime(double TargetDeltaTime, ref DeltaTimeInfo? DTI){
        if(!DTI.HasValue){
            DTI = new DeltaTimeInfo(Stopwatch.GetTimestamp());
            return false;
        }

        long CurrentTicks = Stopwatch.GetTimestamp();
        double ElapsedSeconds = (double)(CurrentTicks - DTI.Value.LastTicks) / Stopwatch.Frequency;

        if(ElapsedSeconds < TargetDeltaTime){
            return false;
        }

        DTI = new DeltaTimeInfo(CurrentTicks, ElapsedSeconds);
        
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LimitByFPS(double TargetFPS, ref DeltaTimeInfo? DTI) => LimitByDeltaTime(DeltaTimeInfo.FPSToDT(TargetFPS), ref DTI);
}