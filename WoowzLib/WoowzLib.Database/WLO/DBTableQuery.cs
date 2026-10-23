using System.Linq.Expressions;
using WLI;

namespace WLO;

public sealed class DTTableQuery<T> where T : new(){
    private readonly Database __Owner;
    private readonly string   __TableName;

    private string?                      __WhereSql;
    private Dictionary<string, object?>? __WhereParameters;
    private string?                      __OrderColumn;
    private bool                         __OrderDescending;
    private int?                         __SkipCount;
    private int?                         __TakeCount;

    internal DTTableQuery(Database Owner, string TableName){
        __Owner = Owner;
        __TableName = TableName;
    }

    public DTTableQuery<T> Where(Expression<Func<T, bool>> Predicate){
        (__WhereSql, __WhereParameters) = WL.SQL.Translate(Predicate, __Owner);
        return this;
    }

    public DTTableQuery<T> OrderBy(Expression<Func<T, object>> Selector){
        __OrderColumn = WL.SQL.GetColumnName(Selector, __Owner);
        __OrderDescending = false;
        return this;
    }

    public DTTableQuery<T> OrderByDescending(Expression<Func<T, object>> Selector){
        __OrderColumn = WL.SQL.GetColumnName(Selector, __Owner);
        __OrderDescending = true;
        return this;
    }

    public DTTableQuery<T> Skip(int Count){ __SkipCount = Count; return this; }
    public DTTableQuery<T> Take(int Count){ __TakeCount = Count; return this; }

    public List<T> ToList() => __Owner.Query<T>(__BuildSql(), __WhereParameters);

    public T? FirstOrDefault(){
        __TakeCount = 1;
        return __Owner.QuerySingle<T>(__BuildSql(), __WhereParameters);
    }

    public int Count(){
        string Sql = $"SELECT COUNT(*) FROM {__Owner.QuoteIdentifier(__TableName)}"
                     + (__WhereSql != null ? $" WHERE {__WhereSql}" : "");

        object? Result = __Owner.QueryScalar(Sql, __WhereParameters);
        return Result == null ? 0 : Convert.ToInt32(Result);
    }

    private string __BuildSql(){
        string Sql = $"SELECT * FROM {__Owner.QuoteIdentifier(__TableName)}";

        if(__WhereSql != null){ Sql += $" WHERE {__WhereSql}"; }
        if(__OrderColumn != null){ Sql += $" ORDER BY {__OrderColumn}" + (__OrderDescending ? " DESC" : " ASC"); }

        if(__TakeCount.HasValue){ Sql += $" LIMIT {__TakeCount.Value}"; }
        else if(__SkipCount.HasValue){ Sql += " LIMIT -1"; }

        if(__SkipCount.HasValue){ Sql += $" OFFSET {__SkipCount.Value}"; }

        return Sql;
    }
}