using System.Reflection;
using Dapper;
using Dommel.Bulk.RowMap;
using Dommel.Bulk.TypeMap;

namespace Dommel.Bulk.DatabaseAdapters;

/// <inheritdoc/>
public abstract class DatabaseAdapterBase : IDatabaseAdapter
{
    private readonly IDictionary<Type, ITypeMapper> _typeMappers;

    protected DatabaseAdapterBase(IDictionary<Type, ITypeMapper> typeMappers)
    {
        _typeMappers = typeMappers;
    }

    /// <inheritdoc/>
    public ITypeMapper GetTypeMapper(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (_typeMappers.TryGetValue(type, out ITypeMapper? typeMapper))
        {
            return typeMapper;
        }
        else if (type.IsEnum && _typeMappers.TryGetValue(Enum.GetUnderlyingType(type), out ITypeMapper? enumTypeMapper))
        {
            return enumTypeMapper;
        }

        throw new NotSupportedException($"Not found type mapper for type {type}");
    }

    /// <inheritdoc/>
    public void AddTypeMapper<T>(GenericTypeMapper<T> genericTypeMapper)
    {
        _typeMappers[typeof(T)] = genericTypeMapper;
    }

    /// <summary>
    /// Returns NULL text
    /// </summary>
    /// <returns>NULL text</returns>
    public virtual string GetNullStr() => "NULL";

    public virtual SqlQuery BuildBulkInsertQuery<T>(
        ISqlBuilder sqlBuilder,
        IRowMapper rowMapper,
        IEnumerable<T> entities,
        ExecutionFlags flags,
        string[] propertiesToUpdate,
        string constraintName)
    {
        Type type = typeof(T);

        string tableName = Resolvers.Table(type, sqlBuilder);

        bool insertDatabaseGeneratedKeys = (flags & ExecutionFlags.InsertDatabaseGeneratedKeys) == ExecutionFlags.InsertDatabaseGeneratedKeys;

        ColumnPropertyInfo[] keyProperties = Resolvers.KeyProperties(type);
        PropertyInfo[] propertiesToUse = Resolvers.Properties(type)
            .Where(x => !x.IsGenerated || keyProperties.Contains(x))
            .Select(x => x.Property)
            .ToArray();

        var propertiesToInsert = insertDatabaseGeneratedKeys
            ? propertiesToUse.ToArray()
            : propertiesToUse
                .Except(keyProperties.Where(p => p.IsGenerated).Select(p => p.Property))
                .ToArray();

        TextWriter tw = new StringWriter();
        DynamicParameters parameters = new DynamicParameters();

        BuildInsertHeader<T>(tw, sqlBuilder, flags, propertiesToUpdate, constraintName);

        tw.Write(" INTO ");
        tw.Write(tableName);
        tw.Write(" (");

        var columnNamesToInsert = propertiesToInsert.Select(p => Resolvers.Column(p, sqlBuilder, false));

        bool isFirst = true;
        foreach (string columnName in columnNamesToInsert)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                tw.Write(", ");
            }

            tw.Write(columnName);
        }
        tw.WriteLine(") VALUES");

        rowMapper.MapRows(
            entities,
            sqlBuilder,
            this,
            tw,
            parameters,
            propertiesToInsert,
            ", ",
            $",{Environment.NewLine}",
            "(",
            ")");

        bool hasUpdateFlag = (flags & ExecutionFlags.UpdateIfExists) == ExecutionFlags.UpdateIfExists;

        IEnumerable<PropertyInfo> propertyInfosToUpdate;

        if (propertiesToUpdate?.Length > 0)
        {
            propertyInfosToUpdate = propertiesToUse
                .Where(x => propertiesToUpdate.Contains(x.Name));
        }
        else if(hasUpdateFlag)
        {
            propertyInfosToUpdate = propertiesToUse
                .Where(x => keyProperties.All(y => y.Property != x));
        }
        else
        {
            propertyInfosToUpdate = Array.Empty<PropertyInfo>();
        }

        BuildInsertFooter<T>(tw, sqlBuilder, flags, propertyInfosToUpdate, constraintName);

        tw.Write(";");

        return new SqlQuery(tw.ToString(), parameters);
    }

    protected virtual void BuildInsertHeader<T>(TextWriter textWriter, ISqlBuilder sqlBuilder, ExecutionFlags flags, string[] propertiesToUpdate, string constraintName)
    {
        textWriter.Write("INSERT");
    }

    protected virtual void BuildInsertFooter<T>(TextWriter textWriter, ISqlBuilder sqlBuilder, ExecutionFlags flags, IEnumerable<PropertyInfo> propertiesToUpdate, string constraintName)
    {
    }
}