using System.Text;
using WLO;

namespace WL;

public struct IO{
    public const uint Magic = 'W' | ('L' << 8) | ('P' << 16) | ('K' << 24);
    
    // todo, пояснить числа из воздуха позже
    public static byte[] Pack(string[] Keys, byte[][] Contents){
        if(Keys.Length != Contents.Length){ throw new ExceptionWL("todo, uncorect lengths"); }

        int TotalContentsL = 0;
        long[] InternalOffsets = new long[Contents.Length];
        for(int i = 0; i < Contents.Length; i++){
            InternalOffsets[i] = TotalContentsL;
            TotalContentsL += Contents[i].Length;
        }

        byte[] Blob = new byte[TotalContentsL];
        int BlobPosition = 0;
        for(int i = 0; i < Contents.Length; i++){
            WL.Byte.WBytes(Blob, ref BlobPosition, Contents[i]);
        }

        byte[] Compressed = WL.Byte.Compress(Blob);
        
        int HeaderL = 8;
        foreach(string Key in Keys){ HeaderL += WL.Byte.SizeOfStringUTF8(Key) + 8 + 8; }

        // todo, сделать динамический массив??????
        byte[] Result = new byte[HeaderL + Compressed.Length];
        int Position = 0;
        
        WL.Byte.WUInt(Result, ref Position, Magic);
        WL.Byte.WInt(Result, ref Position, Keys.Length);

        for(int i = 0; i < Keys.Length; i++){
            Byte.WStringUTF8(Result, ref Position, Keys[i]);
            Byte.WLong(Result, ref Position, InternalOffsets[i]);
            Byte.WLong(Result, ref Position, (long)Contents[i].Length);
        }

        WL.Byte.WBytes(Result, ref Position, Compressed);
        
        return Result;
    }

    public static byte[] Pack(string TargetFolder){
        if(!Directory.Exists(TargetFolder)){ throw new ExceptionWL("todo, target folder not found"); }

        string[] Files = Directory.GetFiles(TargetFolder, "*.*", SearchOption.AllDirectories);
        string[] RelativePaths = new string[Files.Length];
        byte[][] Contents = new byte[Files.Length][];

        for(int i = 0; i < Files.Length; i++){
            RelativePaths[i] = Path.GetRelativePath(TargetFolder, Files[i]).Replace("\\", "/");
            Contents     [i] = File.ReadAllBytes(Files[i]);
        }

        return Pack(RelativePaths, Contents);
    }

    public static void Pack(string TargetFolder, string OutputFile){
        File.WriteAllBytes(OutputFile, Pack(TargetFolder));
    }

    // todo, пояснить числа из воздуха позже
    public static (string[] Keys, byte[][] Contents) Unpack(byte[] WLPK){
        int Position = 0;

        if(WL.Byte.RUInt(WLPK, ref Position) != Magic){ throw new ExceptionWL("todo, THAT NOT WLPK!"); }

        int Count = WL.Byte.RInt(WLPK, ref Position);

        string[] Keys            = new string[Count];
        long  [] InternalOffsets = new long  [Count];
        long  [] ContentLengths  = new long  [Count];

        for(int i = 0; i < Count; i++){
            Keys           [i] = WL.Byte.RStringUTF8(WLPK, ref Position);
            InternalOffsets[i] = WL.Byte.RLong(WLPK, ref Position);
            ContentLengths [i] = WL.Byte.RLong(WLPK, ref Position);
        }

        byte[] Blob = WL.Byte.RBytesCompressed(WLPK, ref Position);

        byte[][] Contents = new byte[Count][];
        for(int i = 0; i < Count; i++){
            int L      = (int)ContentLengths [i];
            int Offset = (int)InternalOffsets[i];

            Contents[i] = new byte[L];
            System.Buffer.BlockCopy(Blob, Offset, Contents[i], 0, L); // todo, вынести в отдельную функцию
        }

        return (Keys, Contents);
    }

    public static (string[] Keys, byte[][] Contents) Unpack(string TargetWLPK){
        if(!File.Exists(TargetWLPK)){ throw new ExceptionWL("todo, ARCHIVE WLPK NOT FOUND"); }
        return Unpack(File.ReadAllBytes(TargetWLPK));
    }
}