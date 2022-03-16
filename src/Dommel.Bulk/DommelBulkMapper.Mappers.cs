using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper;
using Dommel.Bulk.Extensions;
using Dommel.Bulk.TypeMap;

namespace Dommel.Bulk;

public static partial class DommelBulkMapper
{
    private static readonly ITypeMapper EnumTypeMapper = new FormatTypeMapper("d");
    private static readonly ITypeMapper NullTypeMapper = new NullTypeMapper();
    private static Dictionary<Type, ITypeMapper> TypeMappers { get; } = new Dictionary<Type, ITypeMapper>
    {
        [typeof(bool)] = new BoolTypeMapper(),
        [typeof(byte)] = new FormatTypeMapper("D"),
        [typeof(char)] = new FormatTypeMapper(null, quote:"'", escape: true),
        [typeof(double)] = new FormatTypeMapper("G17"),
        [typeof(float)] = new FormatTypeMapper("G9"),
        [typeof(int)] = new FormatTypeMapper("D"),
        [typeof(long)] = new FormatTypeMapper("D"),
        [typeof(sbyte)] = new FormatTypeMapper("D"),
        [typeof(short)] = new FormatTypeMapper("D"),
        [typeof(uint)] = new FormatTypeMapper("D"),
        [typeof(ulong)] = new FormatTypeMapper("D"),
        [typeof(ushort)] = new FormatTypeMapper("D"),
        [typeof(decimal)] = new FormatTypeMapper("G"),
        [typeof(decimal)] = new FormatTypeMapper("D"),
        [typeof(DateTime)] = new FormatTypeMapper("yyyy-MM-dd HH:mm:ss.ffffff", quote: "'"),
        [typeof(Guid)] = new FormatTypeMapper("D", quote: "'"),
        [typeof(string)] = new FormatTypeMapper(null, quote: "'", escape: true),
        [typeof(TimeSpan)] = new TimeSpanTimeMapper(),
        [typeof(ArraySegment<byte>)] = new ByteArraySegmentTypeMapper("0x{0}"),
        [typeof(byte[])] = new ByteArrayTypeMapper("0x{0}"),
#if NET6_0_OR_GREATER
        [typeof(DateOnly)] = new FormatTypeMapper("yyyy-MM-dd", quote:"'"),
        [typeof(TimeOnly)] = new FormatTypeMapper("HH:mm:ss.ffffff", quote:"'"),
#endif
    };

    /// <summary>
    /// Adds a custom implementation of <see cref="ITypeMapper"/>
    /// for the specified type
    /// </summary>
    /// <param name="type">type to the map</param>
    /// <param name="typeMapper">An implementation of the <see cref="ITypeMapper"/> interface.</param>
    public static void AddTypeMapper(Type type, ITypeMapper typeMapper)
    {
        TypeMappers[type] = typeMapper;
    }

    /// <summary>
    /// Bulk inserts the specified collection of entities into the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="connection">The connection to the database. This can either be open or closed.</param>
    /// <param name="entities">The entities to be inserted.</param>
    /// <param name="transaction">Optional transaction for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int BulkInsert<TEntity>(this IDbConnection connection, IEnumerable<TEntity> entities, IDbTransaction? transaction = null)
        where TEntity : class
    {
        var sql = BuildInsertQueryReflection(DommelMapper.GetSqlBuilder(connection), entities);
        LogQuery<TEntity>(sql);
        return connection.Execute(sql, transaction);
    }

    /// <summary>
    /// Bulk inserts the specified collection of entities into the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="connection">The connection to the database. This can either be open or closed.</param>
    /// <param name="entities">The entities to be inserted.</param>
    /// <param name="transaction">Optional transaction for the command.</param>
    /// <param name="cancellationToken">Optional cancellation token for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> BulkInsertAsync<TEntity>(this IDbConnection connection, IEnumerable<TEntity> entities, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var sql = BuildInsertQueryReflection(DommelMapper.GetSqlBuilder(connection), entities);
        LogQuery<TEntity>(sql);
        return connection.ExecuteAsync(new CommandDefinition(sql, transaction: transaction, cancellationToken: cancellationToken));
    }

    internal static string BuildInsertQueryReflection<T>(ISqlBuilder sqlBuilder, IEnumerable<T> entities)
    {
        Type type = typeof(T);

        var tableName = Resolvers.Table(type, sqlBuilder);

        // Use all non-key and non-generated properties for inserts
        var keyProperties = Resolvers.KeyProperties(type);
        var typeProperties = Resolvers.Properties(type)
            .Where(x => !x.IsGenerated)
            .Select(x => x.Property)
            .Except(keyProperties.Where(p => p.IsGenerated).Select(p => p.Property))
            .ToArray();

        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("INSERT INTO {0} (", tableName);

        var columnNames = typeProperties.Select(p => Resolvers.Column(p, sqlBuilder, false));

        sb.AppendJoin(", ", columnNames);
        sb.AppendLine(") VALUES");

        int line = 1;

        foreach (T entity in entities)
        {
            sb.Append('(');

            foreach (PropertyInfo typeProperty in typeProperties)
            {
                ITypeMapper typeMapper = GetTypeMapper(typeProperty.PropertyType);

                sb.Append($"{typeMapper.Map(typeProperty.GetValue(entity))}, ");
            }

            sb.Remove(sb.Length - 2, 2);

            sb.AppendLine("),");
            line++;
        }

        sb.Remove(sb.Length - 3, 3);
        sb.Append(';');

        return sb.ToString();
    }

    internal static string BuildInsertQueryExpression<T>(ISqlBuilder sqlBuilder, IEnumerable<T> entities)
    {
        Type type = typeof(T);

        var tableName = Resolvers.Table(type, sqlBuilder);

        // Use all non-key and non-generated properties for inserts
        var keyProperties = Resolvers.KeyProperties(type);
        var typeProperties = Resolvers.Properties(type)
            .Where(x => !x.IsGenerated)
            .Select(x => x.Property)
            .Except(keyProperties.Where(p => p.IsGenerated).Select(p => p.Property))
            .ToArray();

        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("INSERT INTO {0} (", tableName);

        var columnNames = typeProperties.Select(p => Resolvers.Column(p, sqlBuilder, false));

        sb.AppendJoin(", ", columnNames);
        sb.AppendLine(") VALUES");

        int line = 1;

        foreach (T entity in entities)
        {
            string format = $"({string.Join(", ", Enumerable.Range(0, typeProperties.Length).Select(x => $"{{{x}}}"))})";

            sb.Append('(');

            foreach (PropertyInfo typeProperty in typeProperties)
            {
                ITypeMapper typeMapper = GetTypeMapper(typeProperty.PropertyType);

                sb.Append($"{typeMapper.Map(typeProperty.GetValue(entity))}, ");
            }

            sb.Remove(sb.Length - 2, 2);

            sb.AppendLine("),");
            line++;
        }

        sb.Remove(sb.Length - 3, 3);
        sb.Append(';');

        return sb.ToString();
    }

    public static ITypeMapper GetTypeMapper(Type type)
    {
        if (TypeMappers.TryGetValue(type, out ITypeMapper typeMapper))
        {
            return typeMapper;
        }
        else if (type.IsEnum)
        {
            return EnumTypeMapper;
        }

        throw new NotSupportedException("Not found type mapper for current type");
    }
}