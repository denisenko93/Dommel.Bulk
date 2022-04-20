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
}