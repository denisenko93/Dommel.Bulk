namespace Dommel.Bulk;

[Flags]
public enum ExecutionFlags
{
    None = 0,
    InsertDatabaseGeneratedKeys = 1,
    UpdateIfExists = 2,
    IgnoreErrors = 4
}