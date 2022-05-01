using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Dapper;
using Dommel.Bulk.DatabaseAdapters;
using Dommel.Bulk.TypeMap;

namespace Dommel.Bulk.RowMap;

public class ExpressionRowMapper : IRowMapper
{
    private static readonly ConcurrentDictionary<string, object> StringBuilderFuncCache = new ConcurrentDictionary<string, object>();

    public void MapRows<T>(IEnumerable<T> entities, ISqlBuilder sqlBuilder, IDatabaseAdapter databaseAdapter, TextWriter textWriter, DynamicParameters parameters,
        IReadOnlyCollection<PropertyInfo> properties, string columnSeparator, string rowSeparator, string rowStart, string rowEnd)
    {
        Action<T, TextWriter> mapEntityFunc = GetMapFunc<T>(sqlBuilder, databaseAdapter, properties);

        bool isFirst = true;
        foreach (T entity in entities)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                textWriter.Write(rowSeparator);
            }

            textWriter.Write(rowStart);

            mapEntityFunc(entity, textWriter);

            textWriter.Write(rowEnd);
        }
    }

    private Action<T, TextWriter> GetMapFunc<T>(ISqlBuilder sqlBuilder, IDatabaseAdapter databaseAdapter, IEnumerable<PropertyInfo> properties)
    {
        Action<T, TextWriter> mapEntityFunc;
        Type type = typeof(T);

        string cacheKey = $"{type.FullName}{sqlBuilder.GetType().FullName}{databaseAdapter.GetType().FullName}{string.Join(string.Empty, properties.Select(x => x.Name).OrderBy(x => x))}";

        if (StringBuilderFuncCache.TryGetValue(cacheKey, out object? func))
        {
            mapEntityFunc = (Action<T, TextWriter>) func;
        }
        else
        {
            mapEntityFunc = GenerateStringBuilderMapFunc<T>(properties, databaseAdapter, ", ");

            StringBuilderFuncCache[cacheKey] = mapEntityFunc;
        }

        return mapEntityFunc;
    }

    private static readonly MethodInfo TextWriterWriteMethod = typeof(TextWriter).GetMethod("Write", new []{typeof(string)})!;

    private static Action<T, TextWriter> GenerateStringBuilderMapFunc<T>(IEnumerable<PropertyInfo> typeProperties, IDatabaseAdapter databaseAdapter, string separator)
    {
        ParameterExpression entityParameter = Expression.Parameter(typeof(T));
        ParameterExpression textWriterParameter = Expression.Parameter(typeof(TextWriter));

        Expression writeNull = Expression.Call(textWriterParameter, TextWriterWriteMethod,
            Expression.Constant(databaseAdapter.GetNullStr()));

        bool firstProperty = true;

        List<Expression> expressions = new List<Expression>();

        foreach (PropertyInfo typeProperty in typeProperties)
        {
            ITypeMapper typeMapper = databaseAdapter.GetTypeMapper(typeProperty.PropertyType);

            LambdaExpression typePropertyExpression = typeMapper.GetExpression();

            Type typeMapperParameterType = typePropertyExpression.Parameters[0].Type;

            Expression property;

            if (!typeProperty.PropertyType.IsValueType || (typeProperty.PropertyType.IsGenericType
                && typeProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                property = Expression.Property(entityParameter, typeProperty);

                ParameterExpression propertyTypeParameter = Expression.Parameter(typeProperty.PropertyType, "property");
                Expression typeMapperParameter = typeProperty.PropertyType == typeMapperParameterType
                    ? propertyTypeParameter
                    : Expression.Convert(propertyTypeParameter, typeMapperParameterType);

                typePropertyExpression = Expression.Lambda(
                    Expression.Condition(
                        Expression.Equal(propertyTypeParameter, Expression.Constant(null)),
                        writeNull,
                        Expression.Invoke(typeMapper.GetExpression(), typeMapperParameter, textWriterParameter)),
                    propertyTypeParameter,
                    textWriterParameter);
            }
            else
            {
                property = typeProperty.PropertyType == typeMapperParameterType
                    ? Expression.Property(entityParameter, typeProperty)
                    : Expression.Convert(Expression.Property(entityParameter, typeProperty), typeMapperParameterType);
            }

            if (firstProperty)
            {
                firstProperty = false;
            }
            else
            {
                expressions.Add(Expression.Call(textWriterParameter, TextWriterWriteMethod, Expression.Constant(separator)));
            }

            expressions.Add(Expression.Invoke(typePropertyExpression, property, textWriterParameter));
        }

        var lambdaExpression = Expression.Lambda<Action<T, TextWriter>>(
            Expression.Block(expressions),
            entityParameter,
            textWriterParameter);

        return lambdaExpression.Compile();
    }
}