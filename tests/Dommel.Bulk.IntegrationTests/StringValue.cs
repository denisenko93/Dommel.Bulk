using System.ComponentModel.DataAnnotations.Schema;

namespace Dommel.Bulk.IntegrationTests;

[Table("string_value")]
public class StringValue
{
    public int Id { get; set; }

    [Column("value")]
    public string Value { get; set; }
}