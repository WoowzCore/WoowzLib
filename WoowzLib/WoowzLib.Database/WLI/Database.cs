namespace WLI;

public interface Database : IDisposable{
    static abstract Database Open(string Path);
    static abstract Database OpenInMemory();

    void Execute(string SQL, object? Parameters = null);

    List<T> Query<T>(string SQL, object? Parameters = null) where T : new();
    T? QuerySingle<T>(string SQL, object? Parameters = null) where T : new();
    object? QueryScalar(string SQL, object? Parameters = null);

    DatabaseTransaction BeginTransaction();

    string QuoteIdentifier(string Name);
    string ParameterPrefix{ get; }
    string MapCLRTypeToColumnType(Type CLRType);

    void EnsureTableCreated<T>() where T : class, new();
    
    WLO.DBTable<T> Table<T>() where T : class, new() => new WLO.DBTable<T>(this);
}

public interface DatabaseTransaction : IDisposable{
    void Commit();
}