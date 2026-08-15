namespace WLO;

public readonly struct ProjectInfo{
    public ProjectInfo(string? Name, Version? Version = null, string? Author = null, string? License = null){
        this.Name    = Name    ?? "Неизвестный проект";
        this.Version = Version ?? new Version();
        this.Author  = Author  ?? "Аноним";
        this.License = License ?? "MIT";
    }
    
    public readonly string  Name;
    public readonly Version Version;
    public readonly string  Author;
    public readonly string  License;
}