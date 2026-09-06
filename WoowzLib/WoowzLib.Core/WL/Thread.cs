using System.Diagnostics;
using System.Runtime.CompilerServices;
using WLO;

namespace WL;

public struct Thread{
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

    public static double GetRawDT(ref long LastTicks){
        long CurrentTicks = System.Diagnostics.Stopwatch.GetTimestamp();
        double DT = (double)(CurrentTicks - LastTicks) / System.Diagnostics.Stopwatch.Frequency;
        LastTicks = CurrentTicks;
        return DT;
    }

    public static bool NeedFixedUpdate(ref double Accumulator, double Step){
        if(Accumulator >= Step){
            Accumulator -= Step;
            return true;
        }
        return false;
    }
}