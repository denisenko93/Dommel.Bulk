using System.Linq.Expressions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// <see cref="ITypeMapper"/> implementation for <see cref="TimeSpan"/> type.
/// </summary>
public class TimeSpanTimeMapper : GenericTypeMapper<TimeSpan>
{
    public TimeSpanTimeMapper()
    : base(x => $"{(int) x.TotalHours}{x:\\:mm\\:ss.ffffff}")
    {
    }
}