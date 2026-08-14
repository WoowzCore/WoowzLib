namespace WLO;

public readonly struct DeltaTimeInfo{
    public readonly double DT;
    public readonly double FPS;
    public readonly long   LastTicks;

    public DeltaTimeInfo(long LastTicks){
        this.LastTicks = LastTicks;
    }

    public DeltaTimeInfo(long LastTicks, double DT){
        this.LastTicks = LastTicks;
        this.DT = DT;
        this.FPS = DTToFPS(DT);
    }
    
    public static double FPSToDT(double FPS) => 1.0 / FPS;
    public static double DTToFPS(double DT ) => 1.0 / DT ;
}