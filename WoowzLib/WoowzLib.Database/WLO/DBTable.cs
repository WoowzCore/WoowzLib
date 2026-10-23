using System.Linq.Expressions;
using System.Reflection;
using WLI;

namespace WLO;

public sealed class DBTable<T> where T : class, new(){
    private readonly Database       __Owner;
    private readonly PropertyInfo[] __Properties;
    private readonly string         __TableName;

    internal DBTable(Database Owner){
        __Owner = Owner;
        __TableName  = typeof(T).Name;
        __Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        __Owner.EnsureTableCreated<T>();
    }

    public void Insert(T Item){
        PropertyInfo[] InsertProperties = __Properties.Where(P => !(string.Equals(P.Name, "Id", StringComparison.OrdinalIgnoreCase) && Convert.ToInt64(P.GetValue(Item) ?? 0L) == 0)).ToArray();

        string Columns    = string.Join(", ", InsertProperties.Select(P => __Owner.QuoteIdentifier(P.Name)));
        string ParamNames = string.Join(", ", InsertProperties.Select(P => __Owner.ParameterPrefix + P.Name));

        string SQL = $"INSERT INTO {__Owner.QuoteIdentifier(__TableName)} ({Columns}) VALUES ({ParamNames})";

        Dictionary<string, object?> Parameters = InsertProperties.ToDictionary(P => P.Name, P => P.GetValue(Item));

        __Owner.Execute(SQL, Parameters);
    }

    public void InsertMany(IEnumerable<T> Items){
        using DatabaseTransaction Transaction = __Owner.BeginTransaction();

        foreach(T Item in Items){
            Insert(Item);
        }

        Transaction.Commit();
    }

    public DTTableQuery<T> Where(Expression<Func<T, bool>> Predicate) => new DTTableQuery<T>(__Owner, __TableName).Where(Predicate);
    public DTTableQuery<T> All() => new DTTableQuery<T>(__Owner, __TableName);

    public void DeleteWhere(Expression<Func<T, bool>> Predicate){
        (string WhereSql, Dictionary<string, object?> Parameters) = WL.SQL.Translate(Predicate, __Owner);
        __Owner.Execute($"DELETE FROM {__Owner.QuoteIdentifier(__TableName)} WHERE {WhereSql}", Parameters);
    }

    public void DeleteAll(){
        __Owner.Execute($"DELETE FROM {__Owner.QuoteIdentifier(__TableName)}");
    }

    public void Update(Expression<Func<T, bool>> Predicate, object PatchValues){
        (string WhereSql, Dictionary<string, object?> Parameters) = WL.SQL.Translate(Predicate, __Owner);

        List<string> SetClauses = [];
        foreach(PropertyInfo Property in PatchValues.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)){
            string ParamName = "set_" + Property.Name;
            SetClauses.Add($"{__Owner.QuoteIdentifier(Property.Name)} = {__Owner.ParameterPrefix}{ParamName}");
            Parameters[ParamName] = Property.GetValue(PatchValues);
        }

        string Sql = $"UPDATE {__Owner.QuoteIdentifier(__TableName)} SET {string.Join(", ", SetClauses)} WHERE {WhereSql}";
        __Owner.Execute(Sql, Parameters);
    }
}