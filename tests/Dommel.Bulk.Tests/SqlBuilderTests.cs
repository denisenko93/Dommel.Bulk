using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bogus.DataSets;
using Dommel.Bulk.DatabaseAdapters;
using Dommel.Bulk.RowMap;
using Dommel.Bulk.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Dommel.Bulk.Tests;

public class SqlBuilderTests
{
    private readonly ISqlBuilder _mySqlBuilder = new MySqlSqlBuilder();
    private readonly ISqlBuilder _postgresSqlBuilder = new PostgresSqlBuilder();
    private readonly ISqlBuilder _SqLiteBuilder = new SqliteSqlBuilder();

    private readonly IDatabaseAdapter _mySqlDatabaseAdapter = new MySqlDatabaseAdapter();
    private readonly IDatabaseAdapter _postgresDatabaseAdapter = new PostgreSqlDatabaseAdapter();
    private readonly IDatabaseAdapter _sqLiteDatabaseAdapter = new SqLiteDatabaseAdapter();

    private readonly IReadOnlyList<Person> _people;
    private readonly IReadOnlyList<MySqlAllTypesEntity> _mySqlAllTypes;
    private readonly IReadOnlyList<PostgreSqlAllTypesEntity> _postgreSqlAllTypes;
    private readonly IReadOnlyList<SqLiteAllTypesEntity> _sqLiteSqlAllTypes;

    public SqlBuilderTests()
    {
        _people = JsonConvert.DeserializeObject<Person[]>(File.ReadAllText("people.json")) ?? Array.Empty<Person>();
        _mySqlAllTypes = JsonConvert.DeserializeObject<MySqlAllTypesEntity[]>(File.ReadAllText("all_types.json")) ?? Array.Empty<MySqlAllTypesEntity>();
        _postgreSqlAllTypes = JsonConvert.DeserializeObject<PostgreSqlAllTypesEntity[]>(File.ReadAllText("all_types.json")) ?? Array.Empty<PostgreSqlAllTypesEntity>();
        _sqLiteSqlAllTypes = JsonConvert.DeserializeObject<SqLiteAllTypesEntity[]>(File.ReadAllText("all_types.json")) ?? Array.Empty<SqLiteAllTypesEntity>();
    }

    [Fact]
    public void MySqlParametersSqlBuilderPersonTest()
    {
        IRowMapper rowMapper = new ParametersRowMapper();

        var sql = _mySqlDatabaseAdapter.BuildBulkInsertQuery(_mySqlBuilder, rowMapper, _people, ExecutionFlags.None, Array.Empty<string>(), null);

        Assert.Equal(@"INSERT INTO `people` (`ref`, `first_name`, `last_name`, `gender`, `age`, `birth_day`) VALUES
(@Ref_1, @FirstName_1, @LastName_1, @Gender_1, @Age_1, @BirthDay_1),
(@Ref_2, @FirstName_2, @LastName_2, @Gender_2, @Age_2, @BirthDay_2);", sql.Query);

        Assert.Equal(12, sql.Parameters.ParameterNames.Count());

        Assert.Equal(_people[0].Ref, sql.Parameters.Get<Guid>("Ref_1"));
        Assert.Equal(_people[0].FirstName, sql.Parameters.Get<string>("FirstName_1"));
        Assert.Equal(_people[0].LastName, sql.Parameters.Get<string>("LastName_1"));
        Assert.Equal(_people[0].Gender, sql.Parameters.Get<Name.Gender>("Gender_1"));
        Assert.Equal(_people[0].Age, sql.Parameters.Get<int>("Age_1"));
        Assert.Equal(_people[0].BirthDay, sql.Parameters.Get<DateTime>("BirthDay_1"));

        Assert.Equal(_people[1].Ref, sql.Parameters.Get<Guid>("Ref_2"));
        Assert.Equal(_people[1].FirstName, sql.Parameters.Get<string>("FirstName_2"));
        Assert.Equal(_people[1].LastName, sql.Parameters.Get<string>("LastName_2"));
        Assert.Equal(_people[1].Gender, sql.Parameters.Get<Name.Gender>("Gender_2"));
        Assert.Equal(_people[1].Age, sql.Parameters.Get<int>("Age_2"));
        Assert.Equal(_people[1].BirthDay, sql.Parameters.Get<DateTime>("BirthDay_2"));
    }

    [Fact]
    public void MySqlSqlBuilderPersonTest()
    {
        IRowMapper rowMapper = new ExpressionRowMapper();

        var query = _mySqlDatabaseAdapter.BuildBulkInsertQuery(_mySqlBuilder, rowMapper, _people, ExecutionFlags.None, Array.Empty<string>(), null);

        Assert.Equal(@"INSERT INTO `people` (`ref`, `first_name`, `last_name`, `gender`, `age`, `birth_day`) VALUES
('971af92c-f70e-4916-99e0-03c916cf8b70', 'Marcos', 'Hilll', 0, 46, '1952-04-18 19:32:19.440141'),
('e2265ba5-1f21-47d6-8b01-567a36684e07', 'Johnny', 'Ankunding', 0, 40, '1989-04-08 00:15:03.419836');", query.Query);
    }

    [Fact]
    public void PostgreSqlBuilderPersonTest()
    {
        IRowMapper rowMapper = new ExpressionRowMapper();

        var sqlQuery = _postgresDatabaseAdapter.BuildBulkInsertQuery(_postgresSqlBuilder, rowMapper, _people, ExecutionFlags.None, Array.Empty<string>(), null);

        Assert.Equal(@"INSERT INTO ""people"" (""ref"", ""first_name"", ""last_name"", ""gender"", ""age"", ""birth_day"") VALUES
('971af92c-f70e-4916-99e0-03c916cf8b70', E'Marcos', E'Hilll', 0, 46, '1952-04-18 19:32:19.440141'),
('e2265ba5-1f21-47d6-8b01-567a36684e07', E'Johnny', E'Ankunding', 0, 40, '1989-04-08 00:15:03.419836');", sqlQuery.Query);
    }

    [Fact]
    public void SqLiteBuilderPersonTest()
    {
        IRowMapper rowMapper = new ExpressionRowMapper();

        var sqlQuery = _sqLiteDatabaseAdapter.BuildBulkInsertQuery(_SqLiteBuilder, rowMapper, _people, ExecutionFlags.None, Array.Empty<string>(), null);

        Assert.Equal(@"INSERT INTO people (ref, first_name, last_name, gender, age, birth_day) VALUES
('971af92c-f70e-4916-99e0-03c916cf8b70', 'Marcos', 'Hilll', 0, 46, '1952-04-18 19:32:19.440141'),
('e2265ba5-1f21-47d6-8b01-567a36684e07', 'Johnny', 'Ankunding', 0, 40, '1989-04-08 00:15:03.419836');", sqlQuery.Query);
    }

    [Fact]
    public void MySqlParametersSqlBuilderAllTypesTest()
    {
        IRowMapper rowMapper = new ParametersRowMapper();

        var query = _mySqlDatabaseAdapter.BuildBulkInsertQuery(_mySqlBuilder, rowMapper, _mySqlAllTypes, ExecutionFlags.None, Array.Empty<string>(), null);

        Assert.Equal(@"INSERT INTO `AllTypesEntities` (`Id`, `Ref`, `Short`, `ShortNull`, `UShort`, `UShortNull`, `Int`, `IntNull`, `UInt`, `UIntNull`, `Long`, `LongNull`, `ULong`, `ULongNull`, `Decimal`, `DecimalNull`, `Float`, `FloatNull`, `Double`, `DoubleNull`, `Byte`, `ByteNull`, `SByte`, `SByteNull`, `Bool`, `BoolNull`, `Char`, `CharNull`, `String`, `StringNull`, `Enum`, `EnumNull`, `DateTime`, `DateTimeNull`, `TimeSpan`, `TimeSpanNull`, `ByteArray`, `ByteArrayNull`) VALUES
(@Id_1, @Ref_1, @Short_1, @ShortNull_1, @UShort_1, @UShortNull_1, @Int_1, @IntNull_1, @UInt_1, @UIntNull_1, @Long_1, @LongNull_1, @ULong_1, @ULongNull_1, @Decimal_1, @DecimalNull_1, @Float_1, @FloatNull_1, @Double_1, @DoubleNull_1, @Byte_1, @ByteNull_1, @SByte_1, @SByteNull_1, @Bool_1, @BoolNull_1, @Char_1, @CharNull_1, @String_1, @StringNull_1, @Enum_1, @EnumNull_1, @DateTime_1, @DateTimeNull_1, @TimeSpan_1, @TimeSpanNull_1, @ByteArray_1, @ByteArrayNull_1),
(@Id_2, @Ref_2, @Short_2, @ShortNull_2, @UShort_2, @UShortNull_2, @Int_2, @IntNull_2, @UInt_2, @UIntNull_2, @Long_2, @LongNull_2, @ULong_2, @ULongNull_2, @Decimal_2, @DecimalNull_2, @Float_2, @FloatNull_2, @Double_2, @DoubleNull_2, @Byte_2, @ByteNull_2, @SByte_2, @SByteNull_2, @Bool_2, @BoolNull_2, @Char_2, @CharNull_2, @String_2, @StringNull_2, @Enum_2, @EnumNull_2, @DateTime_2, @DateTimeNull_2, @TimeSpan_2, @TimeSpanNull_2, @ByteArray_2, @ByteArrayNull_2);",
            query.Query);

        Assert.Equal(76, query.Parameters.ParameterNames.Count());
    }

    [Fact]
    public void ParametersPostgreSqlBuilderAllTypesTest()
    {
        IRowMapper rowMapper = new ParametersRowMapper();

        var sqlQuery = _postgresDatabaseAdapter.BuildBulkInsertQuery(_postgresSqlBuilder, rowMapper, _postgreSqlAllTypes, ExecutionFlags.None, Array.Empty<string>(), null);

        Assert.Equal(@"INSERT INTO ""AllTypesEntities"" (""Id"", ""Ref"", ""Int"", ""IntNull"", ""Long"", ""LongNull"", ""Decimal"", ""DecimalNull"", ""Float"", ""FloatNull"", ""Double"", ""DoubleNull"", ""Bool"", ""BoolNull"", ""Char"", ""CharNull"", ""String"", ""StringNull"", ""Enum"", ""EnumNull"", ""DateTime"", ""DateTimeNull"") VALUES
(@Id_1, @Ref_1, @Int_1, @IntNull_1, @Long_1, @LongNull_1, @Decimal_1, @DecimalNull_1, @Float_1, @FloatNull_1, @Double_1, @DoubleNull_1, @Bool_1, @BoolNull_1, @Char_1, @CharNull_1, @String_1, @StringNull_1, @Enum_1, @EnumNull_1, @DateTime_1, @DateTimeNull_1),
(@Id_2, @Ref_2, @Int_2, @IntNull_2, @Long_2, @LongNull_2, @Decimal_2, @DecimalNull_2, @Float_2, @FloatNull_2, @Double_2, @DoubleNull_2, @Bool_2, @BoolNull_2, @Char_2, @CharNull_2, @String_2, @StringNull_2, @Enum_2, @EnumNull_2, @DateTime_2, @DateTimeNull_2);",
            sqlQuery.Query);

        Assert.Equal(44, sqlQuery.Parameters.ParameterNames.Count());
    }

    [Fact]
    public void ParametersSqLiteBuilderAllTypesTest()
    {
        IRowMapper rowMapper = new ParametersRowMapper();

        var sqlQuery = _sqLiteDatabaseAdapter.BuildBulkInsertQuery(_SqLiteBuilder, rowMapper, _sqLiteSqlAllTypes, ExecutionFlags.None, Array.Empty<string>(), null);

        Assert.Equal(@"INSERT INTO AllTypesEntities (Id, Ref, Int, IntNull, Long, LongNull, Decimal, DecimalNull, Float, FloatNull, Double, DoubleNull, Bool, BoolNull, Char, CharNull, String, StringNull, Enum, EnumNull, DateTime, DateTimeNull) VALUES
(@Id_1, @Ref_1, @Int_1, @IntNull_1, @Long_1, @LongNull_1, @Decimal_1, @DecimalNull_1, @Float_1, @FloatNull_1, @Double_1, @DoubleNull_1, @Bool_1, @BoolNull_1, @Char_1, @CharNull_1, @String_1, @StringNull_1, @Enum_1, @EnumNull_1, @DateTime_1, @DateTimeNull_1),
(@Id_2, @Ref_2, @Int_2, @IntNull_2, @Long_2, @LongNull_2, @Decimal_2, @DecimalNull_2, @Float_2, @FloatNull_2, @Double_2, @DoubleNull_2, @Bool_2, @BoolNull_2, @Char_2, @CharNull_2, @String_2, @StringNull_2, @Enum_2, @EnumNull_2, @DateTime_2, @DateTimeNull_2);",
            sqlQuery.Query);

        Assert.Equal(44, sqlQuery.Parameters.ParameterNames.Count());
    }

    [Fact]
    public void MySqlSqlBuilderAllTypesTest()
    {
        IRowMapper rowMapper = new ExpressionRowMapper();

        var query = _mySqlDatabaseAdapter.BuildBulkInsertQuery(_mySqlBuilder, rowMapper, _mySqlAllTypes, ExecutionFlags.None, Array.Empty<string>(), null);

#if NET6_0_OR_GREATER
        Assert.Equal(@"INSERT INTO `AllTypesEntities` (`Id`, `Ref`, `Short`, `ShortNull`, `UShort`, `UShortNull`, `Int`, `IntNull`, `UInt`, `UIntNull`, `Long`, `LongNull`, `ULong`, `ULongNull`, `Decimal`, `DecimalNull`, `Float`, `FloatNull`, `Double`, `DoubleNull`, `Byte`, `ByteNull`, `SByte`, `SByteNull`, `Bool`, `BoolNull`, `Char`, `CharNull`, `String`, `StringNull`, `Enum`, `EnumNull`, `DateTime`, `DateTimeNull`, `TimeSpan`, `TimeSpanNull`, `ByteArray`, `ByteArrayNull`) VALUES
('aed35796-3462-cf11-abc9-8f4bb383070b', '51165bf2-d9b9-25f0-35aa-b61feac477ee', -17084, NULL, 49233, 5778, 1007865200, NULL, 361619009, 1840038885, -3381095505472851184, -1840330474280982528, 4045215493295386624, NULL, 0.05535433864779650, 0.6653645810681940, 0.1929997, NULL, 0.8013071173331539, 0.8670134518490181, 107, 191, 54, NULL, 1, 1, '儔', NULL, NULL, NULL, 5, 3, '2022-03-19 14:11:09.779916', NULL, '111:40:15.760806', NULL, 0xc5a0e328c7319ccbf4a5104ad37b31dbfa69d3e268135f0113f7ae6b5339518e2d5736e001bbd7456104e716a3937dd184c164885f225c16640d3c9aeca34087f19a598790ed29eccbdc829919c5f7b98271a3d88fa1e084de4e973b6ef60f4ee7cb384742854b78900ac3695f6863768cf2197b20b82583e2b45a1edbb125f439b8d66da891ef11b58b8b3cab4e09ff2bd839f2fc63362743375ad57749b4b0d8c994161c9501149b4e3b0cfa5826aeb7c6eba007f6f5b946ef92a39ae15e5d6398d5e4af5f161bb26db348128126ce5d08568602085bd4381dcc4641d1d59b1a870b454e11da268e78e9aec1b29227015bfa1f198fdb1db06debd48b5027d223308924a6c31ace72594f19233185e60f8a3310f845ba6053b79ecc8ce228c3b5ce09d253171992e94c8e9f831eafe6bd547a52308e2be7bfae2dd1ac6ac1842a7beca5f431bf139dfb783a4bc3b4655c13ce0939d151ee706bb6f66b2e4c75ea836a9ffb9ce6a017b281af096ae4308f7dd0fe4a84cd65e6f60d0cfed11b699040080ca835a0363baf217b4a4b0f6ec000baf39b0ffa5559addad7454d5726db1c605c6f05dafe9d41291e28e48dda9dccfab3d4586aa8244a496ad081e33a4099aa06d05580e18d5934fac1b06187c3e7bd8751573b7cfeea7172babdb905ac05c04401f7033ddde55d4cdfae4c46314c34d99b89f6e8137968c39f69a07ed5775a8c2e13a9222a2d96cda9365f156b85c0ad02bcb38d5553d65486dd4dcdd0e219e39015880af99abcc875bf1150cb5102077dcf608d9020691fd7c2eedee690393420b76b1065aa9342e0cc0e9e65a84bb96db728a175e98ab6d17edd081afba5372b5e876035ebdd1bbeee019d9553e0432192bad02bc54bb528bad249298bbca52bfaf5a1424aed3c3ae8f12b4e6e86ffbb0cb2f5dc2a798e2740942bde04b443a98ad42947e65991a0f641e078ff957b7c026377755216e29341f0a90befc5c1ce7e5c009a9ca1da347e1c3b0abd40d3ff7d3b28c931dfcddc4e2ae3e96c25d95691cb23ec68d609231842d182a804700eac4d13b05f219d8596f90ec1b5b4ceb320b957021ad802fa6ec0425e6b51cb742f3a386680186ac3156e8bc056c00a61a190ecf53a9c93ac2e80dbdeec03a3d8ee52be55351d1accdf2382f5f8807b1311eecc5af7a13e720efacaa865ba21c3e9ffee5640659454e0610dc2d76db4de9923ddfca1ded251f73ff8efc7459fa4e3168f8c3774a8e699140efed850c2f7f08b9a7dec90b3b88ebb73540144698b4ba870d9237bf41038a68a40ead8aee393acab40913adac592a27a3499b86fb225c75c05a6af9f239bcb01aae0e84e85785eba784eb386664b10c93c8f9c1bd3a36094dfc7b9b741920d737e16dd5e219fc9c1, 0x582d23d9d0da9d6341bad8841e3fb8d88aa6f0f190b6b75560feacc5adfc70de389c92f006959d3692e8e5e8555223581aec1fee63e483e1a086ba1d475481e664d657dd881a016998068232fd27fac939595a25119b22db1477b9e9889b0494280b951da20b93889bbc38d62cf881b238e754f0fbbe13dbb45c30fb2ef0945f07f3d477ecf26e1afafa203cc8ff63e8a04babe160d7b8a784b4088f5ba89c5ce62258fdbc903ca152bc0de258350c824bd272d61acbd09a240819852d5d19453282b5718edba087cf936f55f232a27dd6ad009a3ab71f2488208e5349f6afe37e7ffcc2c27772cce6479f1ea1996edece0c9b020db3406fc34571e6e1665ef9ccd466f5c334f72774373a5210be71382124a8deb65f8ec0ac37c5924a50c37aa5faa6b52711f0fc951166dd5758e8fe52d6978057dd0fbb6e6b53373653ab774e0801c0db2c9cd489dd90e93639301cfca51673914ca964223acbb18327904ed05438d0ac92f705f84f9bb31bf82e609bc70795290d2d11b1ed29a5c1efcf68dc4fae4a07439aad867035b53b39a38f177f99480ef30d05b435736fdfb236251b149524ab21ed32f32cb3eff1ecd75ca28ff4a7b6833f8da18157065e8e58a67a866d439afba1d0a405f94e0b98ba73d0868690d2144550d99d942633c4479a5ccf4f384b2ac0df9484ef9076f883397968ae64c726c43c8dc23318409fdbb79eb788db7f1c29c7eb095f61ed4bd9d36267803d5bbdca4a9c1f3544b512ec645f63f1b67bc944108782777548bacfbe95e846158c685a629dd7d2c13fb98bcea40ae817b344642b20aef363a865becbf5ed9ba5399405701d51f92fa06c586b9f9a3bb874b95a4a30d1dddb89686c4a3e338a0b1ec7a70855a23bb5e2c70513ccb4b1b1b53ef79ecf6ee08d07ac59561e983238eddff8597d7e90ef9328a254f2b523f8b3bd54f171c51e628a5d48823baa2aded561d54620acf78ce30785fade7c1033972938c4f2d8007c20872cf2926c70987fc2f5824bd90bfa9c3135636d492db80aa8754085b44a47ff54eab7b5311a37cc5c98cc7a2462e356331863ec8a73297c4206354e8a95989ad084032ebe87ab508be9abb8bb72265553eb46cf3439cf85ef3c6f6c5ff1a22b39a64b47bf000ed495d08cc3428896f6a1c9382c39e662c81fe085bf695d6842cf81341db2a89a3cbc925277a0c9be2c77939041bebe807694f3740b032b7a69b5b0b7179f2f7f5dce02b51f9dbe9c236006dbe9527baea8bed85821f7d66cd2e051b4d5d38e5af8087282092032c2230cb0a31e20387706e84626d37b9d3130d5a497c2de500bc7e07e90d57a09f80490b9250d38cb86c1f546a14c3ede1c73c0843c28799f47e62ac3497b881a66ab0342cf1679ed7095b709b6),
('78646df8-9c27-1572-6ac3-0dbf216cb4f3', '10079e41-a3e4-75ff-bcd2-053a04a33a88', -31925, NULL, 12507, 38595, -1243668567, NULL, 1382782515, NULL, -3087096581176634022, 1578508211703644570, 12091159821409646592, 658054436413290496, 0.4827388352787690, 0.09229320173773420, 0.93099713, NULL, 0.7580539914295663, 0.9779688528743772, 164, NULL, -10, -113, 0, NULL, '嵅', '嗈', NULL, NULL, 6, 1, '2022-03-19 17:01:29.458386', '2022-04-04 08:22:27.084511', '149:54:37.430252', NULL, 0x34a384820734fb2cbbc248128f19487a3e0f378e240af72045ab330d6b08811c7c32da56b65ece58897c1c4438b056843231813c9d9cb5bca5333a1269758fefbc5ab1e8eabad3c9d663cf5a52a07d836719357e35a19b9971004dc49f56b77d42b41957770f5849a7402aae21eed77192beff8711a1088af7b3fb0ed772c4e6d4912e3fa39bae02fab8221d7efdc6356f004a12a157003c3030c5b3e62740c8dabbe5b47d85e793c9f3c234f2bcb858160a3f306e8397416d113ae86d6e51c71a2b3774af88d005af79246e46976c889d532442059e1f8b1718d16d942a8aaabd3b1523dcb94cdf3cd220da18f83db5cae55014976f113d20ff77f4e4946b2eeba74cc062a984805700f2513e8ca0092a71b961e2de1c3d2f1123924c7db9715fc277a6861965c33e1e7915e7e18e66417ad25e1e21b004ae380c875ae443c4eef1db8d503464cbd91b29f19fbf357099a86fe23278455951d6759f032fb7b3cf6f014c523f2c186a87d9820fa8ff1412c576e30ca37bb2ea136bd55bbf97dd13ecee57cbf6c29179635fd1d1822c8491140a3ff7d769c5079b78aa705f02640c42d552c6074a0794ef32d54fa7a32f3f899ac0162690d7c76668e42291c6ebec82cbd2dbe15ae76747f5bd814247aac541680906483fc8a2ae075735a86f8b2a712a5eb52b933f39cb163000a6b7084bfdb6460458839fa7499338646605761251e97a811528a7685a76cc4a3d4a565f7f73c8fa632e5679c1c3b4856d6a92a770da3a2e898fc9894776467880d9ff51f0182f424c3908cd9e81ccbab3eb5882fd3dc2d0b430fa91a2f3cf6e89b5884238dfb9a3a94ebdedb299b297de60f83182ac5d538297e438fe20ba8c50a18858e87dcb6c1b06c4f9efed0d8a4517334441290649482de13bcfcac2bc205eaf2327df3b6a5d69e82be5efdad3b9d35315acf1102ad949924c572ff3d7f92871e67e8b32f2bfd18dc788edf36b9688cdd1cab2e89dd1528a979d6b10f7db152b93f36486d917824ca9cb49449d6e7f47afba1e46eabe5052439ea29af876f5ba22f4160fa603ddc1c5d6159f72b9da7da47816b5f678ca66c7c03df6648f411d9921df91b6ad7b9604a9c23fb1fe56d1d4f8cb5b21e3b71ac2e8a5896ea5d491ab77d793d4aecc7403ca840361d9fc4732b64f2d45f68070cbded21a938ce19e4a4b893c945733fa642baf0d82765fa0fe67da5c38c42584b4867f61414555c2f50e8ce7bb6363d148e62985b1a3bfa1d8759c14c35c6564782f691824e7bfc0ae59a154564140109e9b48d8f2ce715594ef56f1c0820c512f720fb99d7d3e50a8ae40d910e194a774d9e016431f67eafaab5ac35c7c781179aff888961a5398125460966f368a8dfa8e82a92c105635b5d5a4cf97711959, 0xb064407113c24fbb0f7757720012c24a5a7d434835d82f326ffa5457daa62e9cbab8960e024e6505171dd792f9a93c2a686d899889d1d0be30606e66dca1cccae99fa83e057bdc4e649dbb61023e5ac75e8a383d7e887a506da2b4987fe1fe51e6ddbfe5a6e6899dd7bc345dd76f2e77cd7907373deef37ad8319657eaf028ac7fe1ce2457c93c051bdb86293cb3d03341e60acd866e9ddabf97b219647119677dd2db96cdff790952f207c70ff25360ba418e4b8fdb600025a57ce80edd5f7fec90913cfe8694b56b7c5ec41cd12ef367c6f03b534e0a42fe3e3d3457271072183c78ae771fb3c719b7a5c886d0e2da1095829377b10602f1e99062bc48bf1e89655b510644ef2ae9094c94c6c426102f64d274f59fad3e6097976b484b1abd8e0387c377adf4990be66fa2fe35e6a09be29d9d404ad3f913a9edefcc0ffdce87e362db9fc4f11cb72a96cf8942bc96782a533f11d8789234b901be45a01cb43066d67df3305d2a82c7c11898424b8a24627f7209c88ec1103e7fbd048f67c29d06b726a5c64ff08ad927201917465a86344144e73aa4c5891679fb630261547916d2b99c5b14478aa2ef71958b35284a8a0dcccae5f1bf1e3f6daea48909a974e42608ddb90253dd19b87c2b0bfc30948f3851df11d4c20e14880308697850af566963f330fbfcfd5180c912dd6fb344fdceb66cd1bb5a1cb21f42cf17388aa79d9c7bd538d767f6e006eaea849a4dce73c1ed59478df64a60d5aa845ac9adaf97412a94d25e6e4bfe18a2b3ff835aac575b8cd7b2b2b1f9f2292833e8841d462b585a03903c8f31aea062019abc3f23d41e09f353485fd4e3f67eafd28f909e1d0aba1f550b41eaf4e6739d79546de5d0ab223750a40103912c755f80c923529dc6a568f14bc3902cd00af2f4b59bd243903e261d1c77dd2c23704f28da406404311dbeb5e0b551f4138faeb567ca3a5a0f7f4b44e6b3552648db0de7623b2c918ef0e816f93295e1c6286569bbf7778b830cbe213982ad2c8cea46da0fdbd7fe3c67bb163cffd7e0c104c088edd2536d1c6df5f8214a424d2e5aa3ce1969c1be38bb7cc7d263383dfc439ff2a988b644e4947abf12da19b6b57ff394bc03aa6b42f200c038d03b80515b1d86b478fec2b3c60f0d4bcfe35d76a55c2725295b1819f9a1d0a375c60fd95653a353a68bae4fa8254d05c6ab5b7d4eb7631f43d74988ecc7f56cbead6be83557509942834ad548cb22538ad30295662738d3b27c2a290a283144bedf17e582f63437733fc183c2e80dc3876fda174c1a8880a98a16880f76d7c29d59262d299e9e7bcc08ff61ce2497de8ad2df293fa14ad4256bc4cd447c4d769d8911d432b41e492626c71a34c4309a203ea0a612c943ec66db53c2b172cfd35c);",
            query.Query);
#else
        Assert.Equal(@"INSERT INTO `AllTypesEntities` (`Id`, `Ref`, `Short`, `ShortNull`, `UShort`, `UShortNull`, `Int`, `IntNull`, `UInt`, `UIntNull`, `Long`, `LongNull`, `ULong`, `ULongNull`, `Decimal`, `DecimalNull`, `Float`, `FloatNull`, `Double`, `DoubleNull`, `Byte`, `ByteNull`, `SByte`, `SByteNull`, `Bool`, `BoolNull`, `Char`, `CharNull`, `String`, `StringNull`, `Enum`, `EnumNull`, `DateTime`, `DateTimeNull`, `TimeSpan`, `TimeSpanNull`, `ByteArray`, `ByteArrayNull`) VALUES
('aed35796-3462-cf11-abc9-8f4bb383070b', '51165bf2-d9b9-25f0-35aa-b61feac477ee', -17084, NULL, 49233, 5778, 1007865200, NULL, 361619009, 1840038885, -3381095505472851184, -1840330474280982528, 4045215493295386624, NULL, 0.05535433864779650, 0.6653645810681940, 0.1929997, NULL, 0.801307117333154, 0.867013451849018, 107, 191, 54, NULL, 1, 1, '儔', NULL, NULL, NULL, 5, 3, '2022-03-19 14:11:09.779916', NULL, '111:40:15.760806', NULL, 0xc5a0e328c7319ccbf4a5104ad37b31dbfa69d3e268135f0113f7ae6b5339518e2d5736e001bbd7456104e716a3937dd184c164885f225c16640d3c9aeca34087f19a598790ed29eccbdc829919c5f7b98271a3d88fa1e084de4e973b6ef60f4ee7cb384742854b78900ac3695f6863768cf2197b20b82583e2b45a1edbb125f439b8d66da891ef11b58b8b3cab4e09ff2bd839f2fc63362743375ad57749b4b0d8c994161c9501149b4e3b0cfa5826aeb7c6eba007f6f5b946ef92a39ae15e5d6398d5e4af5f161bb26db348128126ce5d08568602085bd4381dcc4641d1d59b1a870b454e11da268e78e9aec1b29227015bfa1f198fdb1db06debd48b5027d223308924a6c31ace72594f19233185e60f8a3310f845ba6053b79ecc8ce228c3b5ce09d253171992e94c8e9f831eafe6bd547a52308e2be7bfae2dd1ac6ac1842a7beca5f431bf139dfb783a4bc3b4655c13ce0939d151ee706bb6f66b2e4c75ea836a9ffb9ce6a017b281af096ae4308f7dd0fe4a84cd65e6f60d0cfed11b699040080ca835a0363baf217b4a4b0f6ec000baf39b0ffa5559addad7454d5726db1c605c6f05dafe9d41291e28e48dda9dccfab3d4586aa8244a496ad081e33a4099aa06d05580e18d5934fac1b06187c3e7bd8751573b7cfeea7172babdb905ac05c04401f7033ddde55d4cdfae4c46314c34d99b89f6e8137968c39f69a07ed5775a8c2e13a9222a2d96cda9365f156b85c0ad02bcb38d5553d65486dd4dcdd0e219e39015880af99abcc875bf1150cb5102077dcf608d9020691fd7c2eedee690393420b76b1065aa9342e0cc0e9e65a84bb96db728a175e98ab6d17edd081afba5372b5e876035ebdd1bbeee019d9553e0432192bad02bc54bb528bad249298bbca52bfaf5a1424aed3c3ae8f12b4e6e86ffbb0cb2f5dc2a798e2740942bde04b443a98ad42947e65991a0f641e078ff957b7c026377755216e29341f0a90befc5c1ce7e5c009a9ca1da347e1c3b0abd40d3ff7d3b28c931dfcddc4e2ae3e96c25d95691cb23ec68d609231842d182a804700eac4d13b05f219d8596f90ec1b5b4ceb320b957021ad802fa6ec0425e6b51cb742f3a386680186ac3156e8bc056c00a61a190ecf53a9c93ac2e80dbdeec03a3d8ee52be55351d1accdf2382f5f8807b1311eecc5af7a13e720efacaa865ba21c3e9ffee5640659454e0610dc2d76db4de9923ddfca1ded251f73ff8efc7459fa4e3168f8c3774a8e699140efed850c2f7f08b9a7dec90b3b88ebb73540144698b4ba870d9237bf41038a68a40ead8aee393acab40913adac592a27a3499b86fb225c75c05a6af9f239bcb01aae0e84e85785eba784eb386664b10c93c8f9c1bd3a36094dfc7b9b741920d737e16dd5e219fc9c1, 0x582d23d9d0da9d6341bad8841e3fb8d88aa6f0f190b6b75560feacc5adfc70de389c92f006959d3692e8e5e8555223581aec1fee63e483e1a086ba1d475481e664d657dd881a016998068232fd27fac939595a25119b22db1477b9e9889b0494280b951da20b93889bbc38d62cf881b238e754f0fbbe13dbb45c30fb2ef0945f07f3d477ecf26e1afafa203cc8ff63e8a04babe160d7b8a784b4088f5ba89c5ce62258fdbc903ca152bc0de258350c824bd272d61acbd09a240819852d5d19453282b5718edba087cf936f55f232a27dd6ad009a3ab71f2488208e5349f6afe37e7ffcc2c27772cce6479f1ea1996edece0c9b020db3406fc34571e6e1665ef9ccd466f5c334f72774373a5210be71382124a8deb65f8ec0ac37c5924a50c37aa5faa6b52711f0fc951166dd5758e8fe52d6978057dd0fbb6e6b53373653ab774e0801c0db2c9cd489dd90e93639301cfca51673914ca964223acbb18327904ed05438d0ac92f705f84f9bb31bf82e609bc70795290d2d11b1ed29a5c1efcf68dc4fae4a07439aad867035b53b39a38f177f99480ef30d05b435736fdfb236251b149524ab21ed32f32cb3eff1ecd75ca28ff4a7b6833f8da18157065e8e58a67a866d439afba1d0a405f94e0b98ba73d0868690d2144550d99d942633c4479a5ccf4f384b2ac0df9484ef9076f883397968ae64c726c43c8dc23318409fdbb79eb788db7f1c29c7eb095f61ed4bd9d36267803d5bbdca4a9c1f3544b512ec645f63f1b67bc944108782777548bacfbe95e846158c685a629dd7d2c13fb98bcea40ae817b344642b20aef363a865becbf5ed9ba5399405701d51f92fa06c586b9f9a3bb874b95a4a30d1dddb89686c4a3e338a0b1ec7a70855a23bb5e2c70513ccb4b1b1b53ef79ecf6ee08d07ac59561e983238eddff8597d7e90ef9328a254f2b523f8b3bd54f171c51e628a5d48823baa2aded561d54620acf78ce30785fade7c1033972938c4f2d8007c20872cf2926c70987fc2f5824bd90bfa9c3135636d492db80aa8754085b44a47ff54eab7b5311a37cc5c98cc7a2462e356331863ec8a73297c4206354e8a95989ad084032ebe87ab508be9abb8bb72265553eb46cf3439cf85ef3c6f6c5ff1a22b39a64b47bf000ed495d08cc3428896f6a1c9382c39e662c81fe085bf695d6842cf81341db2a89a3cbc925277a0c9be2c77939041bebe807694f3740b032b7a69b5b0b7179f2f7f5dce02b51f9dbe9c236006dbe9527baea8bed85821f7d66cd2e051b4d5d38e5af8087282092032c2230cb0a31e20387706e84626d37b9d3130d5a497c2de500bc7e07e90d57a09f80490b9250d38cb86c1f546a14c3ede1c73c0843c28799f47e62ac3497b881a66ab0342cf1679ed7095b709b6),
('78646df8-9c27-1572-6ac3-0dbf216cb4f3', '10079e41-a3e4-75ff-bcd2-053a04a33a88', -31925, NULL, 12507, 38595, -1243668567, NULL, 1382782515, NULL, -3087096581176634022, 1578508211703644570, 12091159821409646592, 658054436413290496, 0.4827388352787690, 0.09229320173773420, 0.9309971, NULL, 0.758053991429566, 0.977968852874377, 164, NULL, -10, -113, 0, NULL, '嵅', '嗈', NULL, NULL, 6, 1, '2022-03-19 17:01:29.458386', '2022-04-04 08:22:27.084511', '149:54:37.430252', NULL, 0x34a384820734fb2cbbc248128f19487a3e0f378e240af72045ab330d6b08811c7c32da56b65ece58897c1c4438b056843231813c9d9cb5bca5333a1269758fefbc5ab1e8eabad3c9d663cf5a52a07d836719357e35a19b9971004dc49f56b77d42b41957770f5849a7402aae21eed77192beff8711a1088af7b3fb0ed772c4e6d4912e3fa39bae02fab8221d7efdc6356f004a12a157003c3030c5b3e62740c8dabbe5b47d85e793c9f3c234f2bcb858160a3f306e8397416d113ae86d6e51c71a2b3774af88d005af79246e46976c889d532442059e1f8b1718d16d942a8aaabd3b1523dcb94cdf3cd220da18f83db5cae55014976f113d20ff77f4e4946b2eeba74cc062a984805700f2513e8ca0092a71b961e2de1c3d2f1123924c7db9715fc277a6861965c33e1e7915e7e18e66417ad25e1e21b004ae380c875ae443c4eef1db8d503464cbd91b29f19fbf357099a86fe23278455951d6759f032fb7b3cf6f014c523f2c186a87d9820fa8ff1412c576e30ca37bb2ea136bd55bbf97dd13ecee57cbf6c29179635fd1d1822c8491140a3ff7d769c5079b78aa705f02640c42d552c6074a0794ef32d54fa7a32f3f899ac0162690d7c76668e42291c6ebec82cbd2dbe15ae76747f5bd814247aac541680906483fc8a2ae075735a86f8b2a712a5eb52b933f39cb163000a6b7084bfdb6460458839fa7499338646605761251e97a811528a7685a76cc4a3d4a565f7f73c8fa632e5679c1c3b4856d6a92a770da3a2e898fc9894776467880d9ff51f0182f424c3908cd9e81ccbab3eb5882fd3dc2d0b430fa91a2f3cf6e89b5884238dfb9a3a94ebdedb299b297de60f83182ac5d538297e438fe20ba8c50a18858e87dcb6c1b06c4f9efed0d8a4517334441290649482de13bcfcac2bc205eaf2327df3b6a5d69e82be5efdad3b9d35315acf1102ad949924c572ff3d7f92871e67e8b32f2bfd18dc788edf36b9688cdd1cab2e89dd1528a979d6b10f7db152b93f36486d917824ca9cb49449d6e7f47afba1e46eabe5052439ea29af876f5ba22f4160fa603ddc1c5d6159f72b9da7da47816b5f678ca66c7c03df6648f411d9921df91b6ad7b9604a9c23fb1fe56d1d4f8cb5b21e3b71ac2e8a5896ea5d491ab77d793d4aecc7403ca840361d9fc4732b64f2d45f68070cbded21a938ce19e4a4b893c945733fa642baf0d82765fa0fe67da5c38c42584b4867f61414555c2f50e8ce7bb6363d148e62985b1a3bfa1d8759c14c35c6564782f691824e7bfc0ae59a154564140109e9b48d8f2ce715594ef56f1c0820c512f720fb99d7d3e50a8ae40d910e194a774d9e016431f67eafaab5ac35c7c781179aff888961a5398125460966f368a8dfa8e82a92c105635b5d5a4cf97711959, 0xb064407113c24fbb0f7757720012c24a5a7d434835d82f326ffa5457daa62e9cbab8960e024e6505171dd792f9a93c2a686d899889d1d0be30606e66dca1cccae99fa83e057bdc4e649dbb61023e5ac75e8a383d7e887a506da2b4987fe1fe51e6ddbfe5a6e6899dd7bc345dd76f2e77cd7907373deef37ad8319657eaf028ac7fe1ce2457c93c051bdb86293cb3d03341e60acd866e9ddabf97b219647119677dd2db96cdff790952f207c70ff25360ba418e4b8fdb600025a57ce80edd5f7fec90913cfe8694b56b7c5ec41cd12ef367c6f03b534e0a42fe3e3d3457271072183c78ae771fb3c719b7a5c886d0e2da1095829377b10602f1e99062bc48bf1e89655b510644ef2ae9094c94c6c426102f64d274f59fad3e6097976b484b1abd8e0387c377adf4990be66fa2fe35e6a09be29d9d404ad3f913a9edefcc0ffdce87e362db9fc4f11cb72a96cf8942bc96782a533f11d8789234b901be45a01cb43066d67df3305d2a82c7c11898424b8a24627f7209c88ec1103e7fbd048f67c29d06b726a5c64ff08ad927201917465a86344144e73aa4c5891679fb630261547916d2b99c5b14478aa2ef71958b35284a8a0dcccae5f1bf1e3f6daea48909a974e42608ddb90253dd19b87c2b0bfc30948f3851df11d4c20e14880308697850af566963f330fbfcfd5180c912dd6fb344fdceb66cd1bb5a1cb21f42cf17388aa79d9c7bd538d767f6e006eaea849a4dce73c1ed59478df64a60d5aa845ac9adaf97412a94d25e6e4bfe18a2b3ff835aac575b8cd7b2b2b1f9f2292833e8841d462b585a03903c8f31aea062019abc3f23d41e09f353485fd4e3f67eafd28f909e1d0aba1f550b41eaf4e6739d79546de5d0ab223750a40103912c755f80c923529dc6a568f14bc3902cd00af2f4b59bd243903e261d1c77dd2c23704f28da406404311dbeb5e0b551f4138faeb567ca3a5a0f7f4b44e6b3552648db0de7623b2c918ef0e816f93295e1c6286569bbf7778b830cbe213982ad2c8cea46da0fdbd7fe3c67bb163cffd7e0c104c088edd2536d1c6df5f8214a424d2e5aa3ce1969c1be38bb7cc7d263383dfc439ff2a988b644e4947abf12da19b6b57ff394bc03aa6b42f200c038d03b80515b1d86b478fec2b3c60f0d4bcfe35d76a55c2725295b1819f9a1d0a375c60fd95653a353a68bae4fa8254d05c6ab5b7d4eb7631f43d74988ecc7f56cbead6be83557509942834ad548cb22538ad30295662738d3b27c2a290a283144bedf17e582f63437733fc183c2e80dc3876fda174c1a8880a98a16880f76d7c29d59262d299e9e7bcc08ff61ce2497de8ad2df293fa14ad4256bc4cd447c4d769d8911d432b41e492626c71a34c4309a203ea0a612c943ec66db53c2b172cfd35c);",
            query.Query);
#endif
    }

    [Fact]
    public void SqLiteSqlBuilderAllTypesTest()
    {
        IRowMapper rowMapper = new ExpressionRowMapper();

        var sqlQuery = _sqLiteDatabaseAdapter.BuildBulkInsertQuery(_SqLiteBuilder, rowMapper, _sqLiteSqlAllTypes, ExecutionFlags.None, Array.Empty<string>(), null);

#if NET6_0_OR_GREATER
        Assert.Equal(@"INSERT INTO AllTypesEntities (Id, Ref, Int, IntNull, Long, LongNull, Decimal, DecimalNull, Float, FloatNull, Double, DoubleNull, Bool, BoolNull, Char, CharNull, String, StringNull, Enum, EnumNull, DateTime, DateTimeNull) VALUES
('aed35796-3462-cf11-abc9-8f4bb383070b', '51165bf2-d9b9-25f0-35aa-b61feac477ee', 1007865200, NULL, -3381095505472851184, -1840330474280982528, 0.05535433864779650, 0.6653645810681940, 0.1929997, NULL, 0.8013071173331539, 0.8670134518490181, 1, 1, '儔', NULL, NULL, NULL, 5, 3, '2022-03-19 14:11:09.779916', NULL),
('78646df8-9c27-1572-6ac3-0dbf216cb4f3', '10079e41-a3e4-75ff-bcd2-053a04a33a88', -1243668567, NULL, -3087096581176634022, 1578508211703644570, 0.4827388352787690, 0.09229320173773420, 0.93099713, NULL, 0.7580539914295663, 0.9779688528743772, 0, NULL, '嵅', '嗈', NULL, NULL, 6, 1, '2022-03-19 17:01:29.458386', '2022-04-04 08:22:27.084511');",
            sqlQuery.Query);
#else
        Assert.Equal(@"INSERT INTO AllTypesEntities (Id, Ref, Int, IntNull, Long, LongNull, Decimal, DecimalNull, Float, FloatNull, Double, DoubleNull, Bool, BoolNull, Char, CharNull, String, StringNull, Enum, EnumNull, DateTime, DateTimeNull) VALUES
('aed35796-3462-cf11-abc9-8f4bb383070b', '51165bf2-d9b9-25f0-35aa-b61feac477ee', 1007865200, NULL, -3381095505472851184, -1840330474280982528, 0.05535433864779650, 0.6653645810681940, 0.1929997, NULL, 0.801307117333154, 0.867013451849018, 1, 1, '儔', NULL, NULL, NULL, 5, 3, '2022-03-19 14:11:09.779916', NULL),
('78646df8-9c27-1572-6ac3-0dbf216cb4f3', '10079e41-a3e4-75ff-bcd2-053a04a33a88', -1243668567, NULL, -3087096581176634022, 1578508211703644570, 0.4827388352787690, 0.09229320173773420, 0.9309971, NULL, 0.758053991429566, 0.977968852874377, 0, NULL, '嵅', '嗈', NULL, NULL, 6, 1, '2022-03-19 17:01:29.458386', '2022-04-04 08:22:27.084511');",
            sqlQuery.Query);
#endif
    }

    [Fact]
    public void PostgreSqlSqlBuilderAllTypesTest()
    {
        IRowMapper rowMapper = new ExpressionRowMapper();

        var sqlQuery = _postgresDatabaseAdapter.BuildBulkInsertQuery(_postgresSqlBuilder, rowMapper, _postgreSqlAllTypes, ExecutionFlags.None, Array.Empty<string>(), null);

#if NET6_0_OR_GREATER
        Assert.Equal(@"INSERT INTO ""AllTypesEntities"" (""Id"", ""Ref"", ""Int"", ""IntNull"", ""Long"", ""LongNull"", ""Decimal"", ""DecimalNull"", ""Float"", ""FloatNull"", ""Double"", ""DoubleNull"", ""Bool"", ""BoolNull"", ""Char"", ""CharNull"", ""String"", ""StringNull"", ""Enum"", ""EnumNull"", ""DateTime"", ""DateTimeNull"") VALUES
('aed35796-3462-cf11-abc9-8f4bb383070b', '51165bf2-d9b9-25f0-35aa-b61feac477ee', 1007865200, NULL, -3381095505472851184, -1840330474280982528, 0.05535433864779650, 0.6653645810681940, 0.1929997, NULL, 0.8013071173331539, 0.8670134518490181, true, true, E'儔', NULL, NULL, NULL, 5, 3, '2022-03-19 14:11:09.779916', NULL),
('78646df8-9c27-1572-6ac3-0dbf216cb4f3', '10079e41-a3e4-75ff-bcd2-053a04a33a88', -1243668567, NULL, -3087096581176634022, 1578508211703644570, 0.4827388352787690, 0.09229320173773420, 0.93099713, NULL, 0.7580539914295663, 0.9779688528743772, false, NULL, E'嵅', E'嗈', NULL, NULL, 6, 1, '2022-03-19 17:01:29.458386', '2022-04-04 08:22:27.084511');",
            sqlQuery.Query);
#else
        Assert.Equal(@"INSERT INTO ""AllTypesEntities"" (""Id"", ""Ref"", ""Int"", ""IntNull"", ""Long"", ""LongNull"", ""Decimal"", ""DecimalNull"", ""Float"", ""FloatNull"", ""Double"", ""DoubleNull"", ""Bool"", ""BoolNull"", ""Char"", ""CharNull"", ""String"", ""StringNull"", ""Enum"", ""EnumNull"", ""DateTime"", ""DateTimeNull"") VALUES
('aed35796-3462-cf11-abc9-8f4bb383070b', '51165bf2-d9b9-25f0-35aa-b61feac477ee', 1007865200, NULL, -3381095505472851184, -1840330474280982528, 0.05535433864779650, 0.6653645810681940, 0.1929997, NULL, 0.801307117333154, 0.867013451849018, true, true, E'儔', NULL, NULL, NULL, 5, 3, '2022-03-19 14:11:09.779916', NULL),
('78646df8-9c27-1572-6ac3-0dbf216cb4f3', '10079e41-a3e4-75ff-bcd2-053a04a33a88', -1243668567, NULL, -3087096581176634022, 1578508211703644570, 0.4827388352787690, 0.09229320173773420, 0.9309971, NULL, 0.758053991429566, 0.977968852874377, false, NULL, E'嵅', E'嗈', NULL, NULL, 6, 1, '2022-03-19 17:01:29.458386', '2022-04-04 08:22:27.084511');",
            sqlQuery.Query);
#endif
    }
}