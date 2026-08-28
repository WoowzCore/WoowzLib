namespace WLI.Format;

public interface Format<T> where T : class{
    // todo, 😬 его можно изменить, ИСПРАВЬТЕ
    public string LinkedID{ get; set; }

    bool __Is(byte[] Data);
    T __Load(byte[] Data);
}