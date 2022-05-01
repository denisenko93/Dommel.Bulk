using System.Data;
using System.Runtime.CompilerServices;
using Dommel.Bulk.DatabaseAdapters;
using Dommel.Bulk.TypeMap;

namespace Dommel.Bulk;

/// <summary>
/// Bulk insert to database
/// </summary>
public static partial class DommelBulkMapper
{
    private static Dictionary<string, IDatabaseAdapter> DatabaseAdapters = new Dictionary<string, IDatabaseAdapter>(StringComparer.InvariantCultureIgnoreCase)
    {
        ["MySqlConnection"] = new MySqlDatabaseAdapter()
    };

    /// <summary>
    /// Adds a custom implementation of <see cref="DatabaseAdapterBase"/>
    /// for the specified ADO.NET connection type.
    /// </summary>
    /// <param name="connectionType">
    /// The ADO.NET connection type to use the <paramref name="databaseAdapter"/> with.
    /// <example><c>typeof(SqlConnection)</c></example>
    /// </param>
    /// <param name="databaseAdapter">An implementation of the <see cref="DatabaseAdapterBase"/>.</param>
    public static void AddDatabaseAdapter(Type connectionType, IDatabaseAdapter databaseAdapter) => AddDatabaseAdapter(connectionType.Name, databaseAdapter);

    /// <summary>
    /// Adds a custom implementation of <see cref="DatabaseAdapterBase"/>
    /// for the specified connection name        ///
    /// </summary>
    /// <param name="connectionName">The name of the connection. E.g. "sqlconnection".</param>
    /// <param name="databaseAdapter">An implementation of the <see cref="DatabaseAdapterBase"/>.</param>
    public static void AddDatabaseAdapter(string connectionName, IDatabaseAdapter databaseAdapter) => DatabaseAdapters[connectionName] = databaseAdapter;

    /// <summary>
    /// Add custom type mapper for the generic <see cref="T"/>. Must be implementation of <see cref="GenericTypeMapper{T}"/>
    /// </summary>
    /// <param name="connectionName">The name of the connection. E.g. "sqlconnection".</param>
    /// <param name="genericTypeMapper">Custom implementation of <see cref="GenericTypeMapper{T}"/></param>
    /// <typeparam name="T">Type to map</typeparam>
    public static void AddTypeMapper<T>(string connectionName, GenericTypeMapper<T> genericTypeMapper)
    {
        DatabaseAdapters[connectionName].AddTypeMapper(genericTypeMapper);
    }

    /// <summary>
    /// Add custom type mapper for the generic <see cref="T"/>. Must be implementation of <see cref="GenericTypeMapper{T}"/>
    /// </summary>
    /// <param name="connectionType">
    /// The ADO.NET connection type to use the <see cref="GenericTypeMapper{T}"/> with.
    /// <example><c>typeof(SqlConnection)</c></example>
    /// </param>
    /// <param name="genericTypeMapper">Custom implementation of <see cref="GenericTypeMapper{T}"/></param>
    /// <typeparam name="T">Type to map</typeparam>
    public static void AddTypeMapper<T>(Type connectionType, GenericTypeMapper<T> genericTypeMapper)
    {
        AddTypeMapper(connectionType.Name, genericTypeMapper);
    }

    private static IDatabaseAdapter GetDatabaseAdapter(IDbConnection connection)
    {
        return DatabaseAdapters[connection.GetType().Name];
    }

    private static void LogQuery<T>(string? query, [CallerMemberName] string? method = null)
        => DommelMapper.LogReceived?.Invoke(method != null ? $"{method}<{typeof(T).Name}>: {query}" : query ?? string.Empty);
}