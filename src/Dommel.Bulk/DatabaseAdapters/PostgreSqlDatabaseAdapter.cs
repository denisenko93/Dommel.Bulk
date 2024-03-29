﻿using System.Globalization;
using System.Reflection;
using Dommel.Bulk.Extensions;
using Dommel.Bulk.TypeMap;

namespace Dommel.Bulk.DatabaseAdapters;

public class PostgreSqlDatabaseAdapter : DatabaseAdapterBase
{
    private static readonly Dictionary<Type, ITypeMapper> TypeMappers = new Dictionary<Type, ITypeMapper>
    {
        [typeof(bool)] = new GenericTypeMapper<bool>((x, tw) => tw.Write(x ? "true" : "false")),
        [typeof(byte)] = new GenericTypeMapper<byte>((x, tw) => tw.Write(x)),
        [typeof(char)] = new GenericTypeMapper<char>((x, tw) => tw.WriteEscapePostgreSql(x, true)),
        [typeof(double)] = new GenericTypeMapper<double>((x, tw) => tw.Write(x.ToString(CultureInfo.InvariantCulture))),
        [typeof(float)] = new GenericTypeMapper<float>((x, tw) => tw.Write(x.ToString(CultureInfo.InvariantCulture))),
        [typeof(int)] = new GenericTypeMapper<int>((x, tw) => tw.Write(x)),
        [typeof(long)] = new GenericTypeMapper<long>((x, tw) => tw.Write(x)),
        [typeof(decimal)] = new GenericTypeMapper<decimal>((x, tw) => tw.Write(x.ToString(CultureInfo.InvariantCulture))),
        [typeof(DateTime)] = new GenericTypeMapper<DateTime>((x, tw) => tw.WritePostgreSqlDateTime(x, true)),
        [typeof(Guid)] = new GenericTypeMapper<Guid>((x, tw) => tw.WriteGuid(x, true)),
        [typeof(string)] = new GenericTypeMapper<string>((x, tw) => tw.WriteEscapePostgreSql(x, true))
    };

    public PostgreSqlDatabaseAdapter()
        : base(TypeMappers)
    {
    }

    protected override void BuildInsertFooter<T>(
        TextWriter textWriter, ISqlBuilder sqlBuilder,
        ExecutionFlags flags,
        IEnumerable<PropertyInfo> propertiesToUpdate,
        string constraintName)
    {
        PropertyInfo[] properties = propertiesToUpdate?.ToArray() ?? Array.Empty<PropertyInfo>();

        if (properties.Length > 0 && (flags & ExecutionFlags.IgnoreErrors) == ExecutionFlags.IgnoreErrors)
        {
            throw new InvalidOperationException("PostgreSql does not support the combination of the ExecutionFlags.UpdateIfExists and ExecutionFlags.IgnoreErrors flags.");
        }

        if (properties.Length > 0 && string.IsNullOrEmpty(constraintName))
        {
            throw new InvalidOperationException("To update values on conflicts constraintName must be filled.");
        }

        if (properties.Length > 0)
        {
            textWriter.WriteLine();
            textWriter.Write("ON CONFLICT ON CONSTRAINT ");
            textWriter.Write(sqlBuilder.QuoteIdentifier(constraintName));
            textWriter.Write(" DO UPDATE SET ");

            bool isFirst = true;
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    textWriter.Write(", ");
                }

                string columnName = Resolvers.Column(propertyInfo, sqlBuilder, false);

                textWriter.Write(columnName);
                textWriter.Write(" = EXCLUDED.");
                textWriter.Write(columnName);
            }
        }

        if ((flags & ExecutionFlags.IgnoreErrors) == ExecutionFlags.IgnoreErrors)
        {
            textWriter.WriteLine();
            textWriter.Write("ON CONFLICT DO NOTHING");
        }
    }
}