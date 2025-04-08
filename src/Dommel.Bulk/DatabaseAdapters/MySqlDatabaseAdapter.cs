using System.Globalization;
using System.Reflection;
using Dommel.Bulk.Extensions;
using Dommel.Bulk.TypeMap;

namespace Dommel.Bulk.DatabaseAdapters;

public class MySqlDatabaseAdapter : DatabaseAdapterBase
{
    private static readonly Dictionary<Type, ITypeMapper> TypeMappers = new Dictionary<Type, ITypeMapper>
    {
        [typeof(bool)] = new GenericTypeMapper<bool>((x, tw) => tw.Write(x ? '1' : '0')),
        [typeof(byte)] = new GenericTypeMapper<byte>((x, tw) => tw.Write(x)),
        [typeof(char)] = new GenericTypeMapper<char>((x, tw) => tw.WriteEscapeMysql(x, true)),
        [typeof(double)] = new GenericTypeMapper<double>((x, tw) => tw.Write(x.ToString(CultureInfo.InvariantCulture))),
        [typeof(float)] = new GenericTypeMapper<float>((x, tw) => tw.Write(x.ToString(CultureInfo.InvariantCulture))),
        [typeof(int)] = new GenericTypeMapper<int>((x, tw) => tw.Write(x)),
        [typeof(long)] = new GenericTypeMapper<long>((x, tw) => tw.Write(x)),
        [typeof(sbyte)] = new GenericTypeMapper<sbyte>((x, tw) => tw.Write(x)),
        [typeof(short)] = new GenericTypeMapper<short>((x, tw) => tw.Write(x)),
        [typeof(uint)] = new GenericTypeMapper<uint>((x, tw) => tw.Write(x)),
        [typeof(ulong)] = new GenericTypeMapper<ulong>((x, tw) => tw.Write(x)),
        [typeof(ushort)] = new GenericTypeMapper<ushort>((x, tw) => tw.Write(x)),
        [typeof(decimal)] = new GenericTypeMapper<decimal>((x, tw) => tw.Write(x.ToString(CultureInfo.InvariantCulture))),
        [typeof(DateTime)] = new GenericTypeMapper<DateTime>((x, tw) => tw.WriteMysqlDateTime(x, true)),
        [typeof(Guid)] = new GenericTypeMapper<Guid>((x, tw) => tw.WriteGuid(x, true)),
        [typeof(string)] = new GenericTypeMapper<string>((x, tw) => tw.WriteEscapeMysql(x, true)),
        [typeof(TimeSpan)] = new GenericTypeMapper<TimeSpan>((x, tw) => tw.WriteMysqlTimeSpan(x, true)),
        [typeof(ArraySegment<byte>)] = new GenericTypeMapper<ArraySegment<byte>>((x, tw) => tw.WriteMySqlHexString(x, true)),
        [typeof(byte[])] = new GenericTypeMapper<byte[]>((x, tw) => tw.WriteMySqlHexString(x, true)),
#if NET6_0_OR_GREATER
        [typeof(DateOnly)] = new GenericTypeMapper<DateOnly>((x, tw) => tw.WriteMysqlDateTime(x.ToDateTime(TimeOnly.MinValue), true)),
        [typeof(TimeOnly)] = new GenericTypeMapper<TimeOnly>((x, tw) => tw.WriteMysqlTimeOnly(x, true)),
#endif
    };

    public MySqlDatabaseAdapter()
        : base(TypeMappers)
    {
    }

    protected override void BuildInsertHeader<T>(
        TextWriter textWriter,
        ISqlBuilder sqlBuilder,
        ExecutionFlags flags,
        string[] propertiesToUpdate,
        string constraintName)
    {
        base.BuildInsertHeader<T>(textWriter, sqlBuilder, flags, propertiesToUpdate, constraintName);

        if ((flags & ExecutionFlags.IgnoreErrors) == ExecutionFlags.IgnoreErrors)
        {
            textWriter.Write(" IGNORE");
        }
    }

    protected override void BuildInsertFooter<T>(
        TextWriter textWriter,
        ISqlBuilder sqlBuilder,
        ExecutionFlags flags,
        IEnumerable<PropertyInfo> propertiesToUpdate,
        string constraintName)
    {
        PropertyInfo[] properties = propertiesToUpdate?.ToArray() ?? Array.Empty<PropertyInfo>();

        base.BuildInsertFooter<T>(textWriter, sqlBuilder, flags, properties, constraintName);

        if (properties.Length > 0)
        {
            textWriter.WriteLine();
            textWriter.Write("ON DUPLICATE KEY UPDATE ");

            bool isFirst = true;
            foreach (PropertyInfo propertyToUpdate in properties)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    textWriter.Write(", ");
                }

                string columnName = Resolvers.Column(propertyToUpdate, sqlBuilder, false);

                textWriter.Write(columnName);
                textWriter.Write(" = VALUES(");
                textWriter.Write(columnName);
                textWriter.Write(")");
            }
        }
    }
}