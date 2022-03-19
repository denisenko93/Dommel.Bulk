using System.Data;
using Dapper;
using MySqlConnector;

namespace Dommel.Bulk.Benchmarks;

public class MysqlBenchmarks : DatabaseBenchmarksBase
{
    private const string DatabaseName = "dommel_bulk_benchmarks";

    public MysqlBenchmarks() : base(GetOpenConnection())
    {
    }

    protected override void SetupDatabase(IDbConnection connection)
    {
        connection.Execute($@"drop database if exists {DatabaseName};
            create database {DatabaseName};
            use {DatabaseName};

            create table people(
                id int not null auto_increment,
                ref char(36) not null,
                first_name varchar(255) not null,
                last_name varchar(255) not null,
                gender int not null,
                age int not null,
                birth_day datetime not null,
                created_on datetime not null,
                full_name varchar(600) GENERATED ALWAYS AS (CONCAT(first_name, ' ', last_name)),

                primary key(id)
            );

            create trigger people_before_insert
                before insert
                on people
                for each row
            BEGIN
                set NEW.created_on = NOW();
            end;");

        connection.ChangeDatabase(DatabaseName);
    }

    private static IDbConnection GetOpenConnection()
    {
        MySqlConnection connection = new MySqlConnection($"Server=localhost;Uid=root;Pwd=root;UseAffectedRows=false;");

        connection.Open();

        return connection;
    }
}