using System.Reflection;
using Dapper;
using Dommel.Bulk.DatabaseAdapters;

namespace Dommel.Bulk.RowMap;

public class ParametersRowMapper : IRowMapper
{
    public void MapRows<T>(IEnumerable<T> entities, ISqlBuilder sqlBuilder, IDatabaseAdapter databaseAdapter, TextWriter textWriter, DynamicParameters parameters,
        IReadOnlyCollection<PropertyInfo> properties, string columnSeparator, string rowSeparator, string rowStart, string rowEnd)
    {
        int line = 1;
        foreach (T entity in entities)
        {
            if (line != 1)
            {
                textWriter.Write(rowSeparator);
            }

            textWriter.Write(rowStart);

            bool isFirst = true;
            foreach (PropertyInfo typeProperty in properties)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    textWriter.Write(columnSeparator);
                }

                string parameterName = sqlBuilder.PrefixParameter($"{typeProperty.Name}_{line}");

                parameters.Add(parameterName, typeProperty.GetValue(entity));

                textWriter.Write(parameterName);
            }

            textWriter.Write(rowEnd);

            line++;
        }
    }
}