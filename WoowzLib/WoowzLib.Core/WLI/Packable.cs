namespace WLI;

// НЕ ИСПОЛЬЗОВАТЬ WL.Packabilizer.Pack() в __Pack()!!!

public interface Packable{
    Dictionary<string, object?> __Pack();
    void __Unpack(Dictionary<string, object?> Data);
}