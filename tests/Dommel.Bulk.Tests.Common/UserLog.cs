using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dommel.Bulk.Tests.Common;

[Table("UserLog")]
public class UserLog
{
    public int Increment { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Ref { get; set; }

    public string Name { get; set; }

    public DateTime TimeStamp { get; set; }
}