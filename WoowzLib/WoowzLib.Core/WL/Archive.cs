using System.Text;
using WLO;

namespace WL;

// TODO, КАК ЭТО НАЗВАТЬ?
public struct Archive{
    public const uint Magic = 'W' | ('L' << 8) | ('P' << 16) | ('K' << 24);
    
    public static byte[] Pack(string[] Keys, byte[][] Contents){
        if(Keys.Length != Contents.Length){ throw new ExceptionWL("todo, uncorect lengths"); }

        using MemoryStream MS     = new MemoryStream();
        using BinaryWriter Writer = new BinaryWriter(MS, Encoding.UTF8);
        
        Writer.Write(Magic);
        Writer.Write(Keys.Length);

        long HeaderSize = 8;
        foreach(string Key in Keys){
            HeaderSize += GetStringBinaryStize(Key) + 8 + 8;
        }

        long CurrentOffset = HeaderSize;
        for(int i = 0; i < Keys.Length; i++){
            Writer.Write(Keys[i]);
            Writer.Write(CurrentOffset);
            Writer.Write((long)Contents[i].Length);

            CurrentOffset += Contents[i].Length;
        }

        for(int i = 0; i < Contents.Length; i++){
            Writer.Write(Contents[i]);
        }

        return MS.ToArray();
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

    public static (string[] Keys, byte[][] Contents) Unpack(byte[] WLPK){
        using MemoryStream MS     = new MemoryStream(WLPK);
        using BinaryReader Reader = new BinaryReader(MS, Encoding.UTF8);

        if(Reader.ReadUInt32() != Magic){ throw new ExceptionWL("todo, THAT NOT WLPK!"); }

        int Count = Reader.ReadInt32();

        string[] Keys    = new string[Count];
        long  [] Offsets = new long  [Count];
        long  [] Sizes   = new long  [Count];

        for(int i = 0; i < Count; i++){
            Keys   [i] = Reader.ReadString();
            Offsets[i] = Reader.ReadInt64();
            Sizes  [i] = Reader.ReadInt64();
        }

        byte[][] Contents = new byte[Count][];
        for(int i = 0; i < Count; i++){
            MS.Seek(Offsets[i], SeekOrigin.Begin);
            Contents[i] = Reader.ReadBytes((int)Sizes[i]);
        }

        return (Keys, Contents);
    }

    public static (string[] Keys, byte[][] Contents) Unpack(string TargetWLPK){
        if(!File.Exists(TargetWLPK)){ throw new ExceptionWL("todo, ARCHIVE WLPK NOT FOUND"); }
        return Unpack(File.ReadAllBytes(TargetWLPK));
    }




    private static int GetStringBinaryStize(string S){
        int ByteCount = Encoding.UTF8.GetByteCount(S);
        int LP = ByteCount < 128 ? 1 : (ByteCount < 16384 ? 2 : 3);
        return ByteCount + LP;
    }
}