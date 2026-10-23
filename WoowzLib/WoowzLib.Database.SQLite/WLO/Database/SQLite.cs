using System.Reflection;
using WLI;

namespace WLO.Database;

public class SQLite : WLI.Database{
    private readonly Microsoft.Data.Sqlite.SqliteConnection  __Connection;
    private          Microsoft.Data.Sqlite.SqliteTransaction __CurrentTransaction;

    private SQLite(string ConnectionString){
        __Connection = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
        __Connection.Open();

        using(Microsoft.Data.Sqlite.SqliteCommand Prgma = __Connection.CreateCommand()){
            Prgma.CommandText = "PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;";
            Prgma.ExecuteNonQuery();
        }
    }

    public static WLI.Database Open(string Path) => new SQLite($"Data Source={Path}");
    public static WLI.Database OpenInMemory() => new SQLite("Data Source=:memory:");
    
    // ----------------------------------------------------------------------

    private Microsoft.Data.Sqlite.SqliteCommand __CreateCommand(string SQL, object? Parameters){
        Microsoft.Data.Sqlite.SqliteCommand Command = __Connection.CreateCommand();
        Command.CommandText = SQL;
        Command.Transaction = __CurrentTransaction;
        __BindParameters(Command, Parameters);
        return Command;
    }
    
    public void Execute(string SQL, object? Parameters = null){
        using Microsoft.Data.Sqlite.SqliteCommand Command = __CreateCommand(SQL, Parameters);
        Command.ExecuteNonQuery();
    }

    public List<T> Query<T>(string SQL, object? Parameters = null) where T : new(){
        using Microsoft.Data.Sqlite.SqliteCommand Command = __CreateCommand(SQL, Parameters);
        using Microsoft.Data.Sqlite.SqliteDataReader Reader = Command.ExecuteReader();

        List<T> Result = [];
        while(Reader.Read()){
            Result.Add(__MapRow<T>(Reader));
        }
        return Result;
    }

    public T? QuerySingle<T>(string SQL, object? Parameters = null) where T : new(){
        using Microsoft.Data.Sqlite.SqliteCommand Command = __CreateCommand(SQL, Parameters);
        using Microsoft.Data.Sqlite.SqliteDataReader Reader = Command.ExecuteReader();

        return Reader.Read() ? __MapRow<T>(Reader) : default;
    }

    public object? QueryScalar(string SQL, object? Parameters = null){
        using Microsoft.Data.Sqlite.SqliteCommand Command = __CreateCommand(SQL, Parameters);
        object? Result = Command.ExecuteScalar();
        return Result is DBNull ? null : Result;
    }

    public DatabaseTransaction BeginTransaction(){
        __CurrentTransaction = __Connection.BeginTransaction();
        return new SQLiteTransaction(__CurrentTransaction, () => __CurrentTransaction = null);
    }

    // ----------------------------------------------------------------------
    
    public string QuoteIdentifier(string Name) => $"\"{Name}\"";

    public string ParameterPrefix => "$";

    public string MapCLRTypeToColumnType(Type CLRType){
        Type ActualType = Nullable.GetUnderlyingType(CLRType) ?? CLRType;

        if(ActualType == typeof(string)){ return "TEXT"; }

        if(ActualType == typeof(long) || ActualType == typeof(int) || ActualType == typeof(short) || ActualType == typeof(byte) || ActualType == typeof(bool)){ return "INTEGER"; }

        if(ActualType == typeof(double) || ActualType == typeof(float) || ActualType == typeof(decimal)){ return "REAL"; }

        if(ActualType == typeof(byte[])){ return "BLOB"; }

        return "TEXT";
    }

    public void EnsureTableCreated<T>() where T : class, new(){
        string TableName = typeof(T).Name;
        PropertyInfo[] Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        List<string> ColumnDefinitions = [];

        foreach(PropertyInfo Property in Properties){
            string ColumnName = QuoteIdentifier(Property.Name);

            bool IsIDColumn = string.Equals(Property.Name, "Id", StringComparison.OrdinalIgnoreCase)
                              && (Property.PropertyType == typeof(int) || Property.PropertyType == typeof(long));

            if(IsIDColumn){
                ColumnDefinitions.Add($"{ColumnName} INTEGER PRIMARY KEY AUTOINCREMENT");
                continue;
            }

            string ColumnType = MapCLRTypeToColumnType(Property.PropertyType);
            bool IsNullable = Nullable.GetUnderlyingType(Property.PropertyType) != null || !Property.PropertyType.IsValueType;

            ColumnDefinitions.Add($"{ColumnName} {ColumnType}" + (IsNullable ? "" : " NOT NULL"));
        }

        string SQL = $"CREATE TABLE IF NOT EXISTS {QuoteIdentifier(TableName)} ({string.Join(", ", ColumnDefinitions)});";
        Execute(SQL);
    }
    
    // ----------------------------------------------------------------------

    private static T __MapRow<T>(Microsoft.Data.Sqlite.SqliteDataReader Reader) where T : new(){
        T Item = new T();
        PropertyInfo[] Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for(int i = 0; i < Reader.FieldCount; i++){
            string ColumnName = Reader.GetName(i);

            PropertyInfo? Property = Array.Find(Properties,
                P => string.Equals(P.Name, ColumnName, StringComparison.OrdinalIgnoreCase));

            if(Property == null || !Property.CanWrite){ continue; }

            object Value = Reader.GetValue(i);
            if(Value is DBNull){ continue; }

            Property.SetValue(Item, __ConvertValue(Value, Property.PropertyType));
        }

        return Item;
    }
    
    private static void __BindParameters(Microsoft.Data.Sqlite.SqliteCommand Command, object? Parameters){
        if(Parameters == null){ return; }

        if(Parameters is IDictionary<string, object?> Dictionary){
            foreach(KeyValuePair<string, object?> Pair in Dictionary){
                Command.Parameters.AddWithValue("$" + Pair.Key, Pair.Value ?? DBNull.Value);
            }
            return;
        }

        foreach(PropertyInfo Property in Parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)){
            object? Value = Property.GetValue(Parameters);
            Command.Parameters.AddWithValue("$" + Property.Name, Value ?? DBNull.Value);
        }
    }

    private static object? __ConvertValue(object Value, Type TargetType){
        Type ActualType = Nullable.GetUnderlyingType(TargetType) ?? TargetType;

        if(ActualType.IsEnum){ return Enum.Parse(ActualType, Value.ToString()!); }
        if(ActualType == typeof(bool)){ return Convert.ToInt64(Value) != 0; }

        return Convert.ChangeType(Value, ActualType);
    }

    public void Dispose() => __Connection.Dispose();
}

public sealed class SQLiteTransaction : WLI.DatabaseTransaction{
    private readonly Microsoft.Data.Sqlite.SqliteTransaction __Transaction;
    private readonly Action                                  __OnFinished;
    private          bool                                    __Committed = false;

    internal SQLiteTransaction(Microsoft.Data.Sqlite.SqliteTransaction Transaction, Action OnFinished){
        __Transaction = Transaction;
        __OnFinished  = OnFinished;
    }

    public void Commit(){
        __Transaction.Commit();
        __Committed = true;
    }

    public void Dispose(){
        if(!__Committed){ __Transaction.Rollback(); }
        __Transaction.Dispose();
        __OnFinished();
    }
}