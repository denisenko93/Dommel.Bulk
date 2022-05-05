using System.ComponentModel.DataAnnotations.Schema;

namespace Dommel.Bulk.Tests.Common;

[Table("AllTypesEntities")]
public class PostgreSqlAllTypesEntity : IEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public Guid? Ref { get; set; }

    public int Int { get; set; }

    public int? IntNull { get; set; }

    public long Long { get; set; }

    public long? LongNull { get; set; }

    public decimal Decimal { get; set; }

    public decimal? DecimalNull { get; set; }

    public float Float { get; set; }

    public float? FloatNull { get; set; }

    public double Double { get; set; }

    public double? DoubleNull { get; set; }

    public bool Bool { get; set; }

    public bool? BoolNull { get; set; }

    public char Char { get; set; }

    public char? CharNull { get; set; }

    public string String { get; set; }

    public string? StringNull { get; set; }

    public DayOfWeek Enum { get; set; }

    public DayOfWeek? EnumNull { get; set; }

    // date and time types
    public DateTime DateTime { get; set; }

    public DateTime? DateTimeNull { get; set; }
}