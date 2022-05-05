using System.ComponentModel.DataAnnotations.Schema;

namespace Dommel.Bulk.Tests.Common;

[Table("AllTypesEntities")]
public class MySqlAllTypesEntity : IEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public Guid? Ref { get; set; }

    public short Short { get; set; }

    public short? ShortNull { get; set; }

    public ushort UShort { get; set; }

    public ushort? UShortNull { get; set; }

    public int Int { get; set; }

    public int? IntNull { get; set; }

    public uint UInt { get; set; }

    public uint? UIntNull { get; set; }

    public long Long { get; set; }

    public long? LongNull { get; set; }

    public ulong ULong { get; set; }

    public ulong? ULongNull { get; set; }

    public decimal Decimal { get; set; }

    public decimal? DecimalNull { get; set; }

    public float Float { get; set; }

    public float? FloatNull { get; set; }

    public double Double { get; set; }

    public double? DoubleNull { get; set; }

    public byte Byte { get; set; }

    public byte? ByteNull { get; set; }

    public sbyte SByte { get; set; }

    public sbyte? SByteNull { get; set; }

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

    public TimeSpan TimeSpan { get; set; }

    public TimeSpan? TimeSpanNull { get; set; }

    public byte[] ByteArray { get; set; }

    public byte[]? ByteArrayNull { get; set; }

#if NET6_0_OR_GREATER
    public DateOnly DateOnly { get; set; }

    public DateOnly? DateOnlyNull { get; set; }

    public TimeOnly TimeOnly { get; set; }

    public TimeOnly? TimeOnlyNull { get; set; }
#endif
}