using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using Xunit;

namespace Dommel.Bulk.IntegrationTests;

public class MysqlBulkInsertTests : BulkInsertTestsBase
{
    public MysqlBulkInsertTests()
    {
        using (IDbConnection connection = GetOpenConnection())
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

                primary key(id),

                unique(first_name, last_name)
            );

            create trigger people_before_insert
                before insert
                on people
                for each row
            BEGIN
                set NEW.created_on = NOW();
            end;

            create table `AllTypesEntities`(
                `Id` char(36) not null,
                `Ref` char(36) null,

                `Short` SMALLINT not null,
                `ShortNull` SMALLINT null,
                `UShort` SMALLINT UNSIGNED not null,
                `UShortNull` SMALLINT UNSIGNED null,

                `Int` int not null,
                `IntNull` int null,
                `UInt` int UNSIGNED not null,
                `UIntNull` int UNSIGNED null,

                `Long` BIGINT not null,
                `LongNull` BIGINT null,
                `ULong` BIGINT UNSIGNED not null,
                `ULongNull` BIGINT UNSIGNED null,

                `Decimal` decimal(65,28) not null,
                `DecimalNull` decimal(65,28) null,

                `Float` float not null,
                `FloatNull` float null,

                `Double` double not null,
                `DoubleNull` double null,

                `Byte` TINYINT UNSIGNED not null,
                `ByteNull` TINYINT UNSIGNED null,
                `SByte` TINYINT not null,
                `SByteNull` TINYINT null,

                `Bool` tinyint(1) not null,
                `BoolNull` tinyint(1) null,

                `Char` char(1) not null,
                `CharNull` char(1) null,

                `String` varchar(255) null,
                `StringNull` text null,

                `Enum` int not null,
                `EnumNull` int null,

                `DateTime` datetime(6) not null,
                `DateTimeNull` datetime(6) null,

                `TimeSpan` TIME(6) not null,
                `TimeSpanNull` TIME(6) null,

                `ByteArray` VARBINARY(1003) not null,
                `ByteArrayNull` blob null,


                primary key(Id));

            CREATE TABLE string_value(
                `id` int AUTO_INCREMENT,
                `value` text not null,
                PRIMARY KEY(`id`));");
        }
    }

    protected override IDbConnection GetConnection()
    {
        return new MySqlConnection($"Server=localhost;Uid=root;Pwd=root;UseAffectedRows=false;");
    }
}