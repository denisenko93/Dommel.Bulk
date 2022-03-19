using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bogus.DataSets;

namespace Dommel.Bulk.Tests.Common;

[Table("people")]
public class Person
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ref")]
    public Guid Ref { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; }

    [Column("last_name")]
    public string LastName { get; set; }

    [Column("gender")]
    public Name.Gender Gender { get; set; }

    [Column("age")]
    public int Age { get; set; }

    [Column("birth_day")]
    public DateTime BirthDay { get; set; }

    [Column("created_on")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime? CreatedOn { get; set; }

    [Column("full_name")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string FullName { get; set; }
}