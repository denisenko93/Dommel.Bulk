using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bogus.DataSets;
using Dommel.Bulk.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Dommel.Bulk.Tests;

public class SqlBuilderTests
{
    private readonly ISqlBuilder _sqlBuilder = new MySqlSqlBuilder();
    private readonly IReadOnlyList<Person>? _people;
    private readonly IReadOnlyList<AllTypesEntity>? _allTypes;

    public SqlBuilderTests()
    {
        _people = JsonConvert.DeserializeObject<Person[]>(File.ReadAllText("people.json"));
        _allTypes = JsonConvert.DeserializeObject<AllTypesEntity[]>(File.ReadAllText("all_types.json"));
    }

    [Fact]
    public void ParametersSqlBuilderPersonTest()
    {
        var sql = DommelBulkMapper.BuildInsertParametersQuery(_sqlBuilder, _people);

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
    public void SqlBuilderPersonTest()
    {
        var sql = DommelBulkMapper.BuildInsertQuery(_sqlBuilder, _people);

        Assert.Equal(@"INSERT INTO `people` (`ref`, `first_name`, `last_name`, `gender`, `age`, `birth_day`) VALUES
('971af92c-f70e-4916-99e0-03c916cf8b70', 'Marcos', 'Hilll', 0, 46, '1952-04-18 19:32:19.440141'),
('e2265ba5-1f21-47d6-8b01-567a36684e07', 'Johnny', 'Ankunding', 0, 40, '1989-04-08 00:15:03.419836');", sql);
    }

    [Fact]
    public void ParametersSqlBuilderAllTypesTest()
    {
        var sql = DommelBulkMapper.BuildInsertParametersQuery(_sqlBuilder, _allTypes);

        Assert.Equal(@"INSERT INTO `AllTypesEntities` (`Id`, `Ref`, `Short`, `ShortNull`, `UShort`, `UShortNull`, `Int`, `IntNull`, `UInt`, `UIntNull`, `Long`, `LongNull`, `ULong`, `ULongNull`, `Decimal`, `DecimalNull`, `Float`, `FloatNull`, `Double`, `DoubleNull`, `Byte`, `ByteNull`, `SByte`, `SByteNull`, `Bool`, `BoolNull`, `Char`, `CharNull`, `String`, `StringNull`, `Enum`, `EnumNull`, `DateTime`, `DateTimeNull`, `DateTimeOffset`, `DateTimeOffsetNull`, `TimeSpan`, `TimeSpanNull`, `ByteArray`, `ByteArrayNull`) VALUES
(@Id_1, @Ref_1, @Short_1, @ShortNull_1, @UShort_1, @UShortNull_1, @Int_1, @IntNull_1, @UInt_1, @UIntNull_1, @Long_1, @LongNull_1, @ULong_1, @ULongNull_1, @Decimal_1, @DecimalNull_1, @Float_1, @FloatNull_1, @Double_1, @DoubleNull_1, @Byte_1, @ByteNull_1, @SByte_1, @SByteNull_1, @Bool_1, @BoolNull_1, @Char_1, @CharNull_1, @String_1, @StringNull_1, @Enum_1, @EnumNull_1, @DateTime_1, @DateTimeNull_1, @DateTimeOffset_1, @DateTimeOffsetNull_1, @TimeSpan_1, @TimeSpanNull_1, @ByteArray_1, @ByteArrayNull_1),
(@Id_2, @Ref_2, @Short_2, @ShortNull_2, @UShort_2, @UShortNull_2, @Int_2, @IntNull_2, @UInt_2, @UIntNull_2, @Long_2, @LongNull_2, @ULong_2, @ULongNull_2, @Decimal_2, @DecimalNull_2, @Float_2, @FloatNull_2, @Double_2, @DoubleNull_2, @Byte_2, @ByteNull_2, @SByte_2, @SByteNull_2, @Bool_2, @BoolNull_2, @Char_2, @CharNull_2, @String_2, @StringNull_2, @Enum_2, @EnumNull_2, @DateTime_2, @DateTimeNull_2, @DateTimeOffset_2, @DateTimeOffsetNull_2, @TimeSpan_2, @TimeSpanNull_2, @ByteArray_2, @ByteArrayNull_2);",
            sql.Query);

        Assert.Equal(80, sql.Parameters.ParameterNames.Count());
    }

    [Fact]
    public void SqlBuilderAllTypesTest()
    {
        var sql = DommelBulkMapper.BuildInsertQuery(_sqlBuilder, _allTypes);

        Assert.Equal(@"INSERT INTO `AllTypesEntities` (`Id`, `Ref`, `Short`, `ShortNull`, `UShort`, `UShortNull`, `Int`, `IntNull`, `UInt`, `UIntNull`, `Long`, `LongNull`, `ULong`, `ULongNull`, `Decimal`, `DecimalNull`, `Float`, `FloatNull`, `Double`, `DoubleNull`, `Byte`, `ByteNull`, `SByte`, `SByteNull`, `Bool`, `BoolNull`, `Char`, `CharNull`, `String`, `StringNull`, `Enum`, `EnumNull`, `DateTime`, `DateTimeNull`, `DateTimeOffset`, `DateTimeOffsetNull`, `TimeSpan`, `TimeSpanNull`, `ByteArray`, `ByteArrayNull`) VALUES
('aed35796-3462-cf11-abc9-8f4bb383070b', '51165bf2-d9b9-25f0-35aa-b61feac477ee', -17084, NULL, 49233, 5778, 1007865200, NULL, 361619009, 1840038885, -3381095505472851184, -1840330474280982528, 4045215493295386624, NULL, 0.05535433864779650, 0.6653645810681940, 0.192999706, NULL, 0.80130711733315385, 0.86701345184901812, 107, 191, 54, NULL, 1, 1, '儔', NULL, NULL, NULL, 5, 3, '2022-03-19 14:11:09.779916', NULL, '2022-03-18 19:50:21.818354+02:00', '2022-04-23 01:26:30.988390+02:00', '111:40:15.760806', NULL, 0xC5A0E328C7319CCBF4A5104AD37B31DBFA69D3E268135F0113F7AE6B5339518E2D5736E001BBD7456104E716A3937DD184C164885F225C16640D3C9AECA34087F19A598790ED29ECCBDC829919C5F7B98271A3D88FA1E084DE4E973B6EF60F4EE7CB384742854B78900AC3695F6863768CF2197B20B82583E2B45A1EDBB125F439B8D66DA891EF11B58B8B3CAB4E09FF2BD839F2FC63362743375AD57749B4B0D8C994161C9501149B4E3B0CFA5826AEB7C6EBA007F6F5B946EF92A39AE15E5D6398D5E4AF5F161BB26DB348128126CE5D08568602085BD4381DCC4641D1D59B1A870B454E11DA268E78E9AEC1B29227015BFA1F198FDB1DB06DEBD48B5027D223308924A6C31ACE72594F19233185E60F8A3310F845BA6053B79ECC8CE228C3B5CE09D253171992E94C8E9F831EAFE6BD547A52308E2BE7BFAE2DD1AC6AC1842A7BECA5F431BF139DFB783A4BC3B4655C13CE0939D151EE706BB6F66B2E4C75EA836A9FFB9CE6A017B281AF096AE4308F7DD0FE4A84CD65E6F60D0CFED11B699040080CA835A0363BAF217B4A4B0F6EC000BAF39B0FFA5559ADDAD7454D5726DB1C605C6F05DAFE9D41291E28E48DDA9DCCFAB3D4586AA8244A496AD081E33A4099AA06D05580E18D5934FAC1B06187C3E7BD8751573B7CFEEA7172BABDB905AC05C04401F7033DDDE55D4CDFAE4C46314C34D99B89F6E8137968C39F69A07ED5775A8C2E13A9222A2D96CDA9365F156B85C0AD02BCB38D5553D65486DD4DCDD0E219E39015880AF99ABCC875BF1150CB5102077DCF608D9020691FD7C2EEDEE690393420B76B1065AA9342E0CC0E9E65A84BB96DB728A175E98AB6D17EDD081AFBA5372B5E876035EBDD1BBEEE019D9553E0432192BAD02BC54BB528BAD249298BBCA52BFAF5A1424AED3C3AE8F12B4E6E86FFBB0CB2F5DC2A798E2740942BDE04B443A98AD42947E65991A0F641E078FF957B7C026377755216E29341F0A90BEFC5C1CE7E5C009A9CA1DA347E1C3B0ABD40D3FF7D3B28C931DFCDDC4E2AE3E96C25D95691CB23EC68D609231842D182A804700EAC4D13B05F219D8596F90EC1B5B4CEB320B957021AD802FA6EC0425E6B51CB742F3A386680186AC3156E8BC056C00A61A190ECF53A9C93AC2E80DBDEEC03A3D8EE52BE55351D1ACCDF2382F5F8807B1311EECC5AF7A13E720EFACAA865BA21C3E9FFEE5640659454E0610DC2D76DB4DE9923DDFCA1DED251F73FF8EFC7459FA4E3168F8C3774A8E699140EFED850C2F7F08B9A7DEC90B3B88EBB73540144698B4BA870D9237BF41038A68A40EAD8AEE393ACAB40913ADAC592A27A3499B86FB225C75C05A6AF9F239BCB01AAE0E84E85785EBA784EB386664B10C93C8F9C1BD3A36094DFC7B9B741920D737E16DD5E219FC9C1, 0x582D23D9D0DA9D6341BAD8841E3FB8D88AA6F0F190B6B75560FEACC5ADFC70DE389C92F006959D3692E8E5E8555223581AEC1FEE63E483E1A086BA1D475481E664D657DD881A016998068232FD27FAC939595A25119B22DB1477B9E9889B0494280B951DA20B93889BBC38D62CF881B238E754F0FBBE13DBB45C30FB2EF0945F07F3D477ECF26E1AFAFA203CC8FF63E8A04BABE160D7B8A784B4088F5BA89C5CE62258FDBC903CA152BC0DE258350C824BD272D61ACBD09A240819852D5D19453282B5718EDBA087CF936F55F232A27DD6AD009A3AB71F2488208E5349F6AFE37E7FFCC2C27772CCE6479F1EA1996EDECE0C9B020DB3406FC34571E6E1665EF9CCD466F5C334F72774373A5210BE71382124A8DEB65F8EC0AC37C5924A50C37AA5FAA6B52711F0FC951166DD5758E8FE52D6978057DD0FBB6E6B53373653AB774E0801C0DB2C9CD489DD90E93639301CFCA51673914CA964223ACBB18327904ED05438D0AC92F705F84F9BB31BF82E609BC70795290D2D11B1ED29A5C1EFCF68DC4FAE4A07439AAD867035B53B39A38F177F99480EF30D05B435736FDFB236251B149524AB21ED32F32CB3EFF1ECD75CA28FF4A7B6833F8DA18157065E8E58A67A866D439AFBA1D0A405F94E0B98BA73D0868690D2144550D99D942633C4479A5CCF4F384B2AC0DF9484EF9076F883397968AE64C726C43C8DC23318409FDBB79EB788DB7F1C29C7EB095F61ED4BD9D36267803D5BBDCA4A9C1F3544B512EC645F63F1B67BC944108782777548BACFBE95E846158C685A629DD7D2C13FB98BCEA40AE817B344642B20AEF363A865BECBF5ED9BA5399405701D51F92FA06C586B9F9A3BB874B95A4A30D1DDDB89686C4A3E338A0B1EC7A70855A23BB5E2C70513CCB4B1B1B53EF79ECF6EE08D07AC59561E983238EDDFF8597D7E90EF9328A254F2B523F8B3BD54F171C51E628A5D48823BAA2ADED561D54620ACF78CE30785FADE7C1033972938C4F2D8007C20872CF2926C70987FC2F5824BD90BFA9C3135636D492DB80AA8754085B44A47FF54EAB7B5311A37CC5C98CC7A2462E356331863EC8A73297C4206354E8A95989AD084032EBE87AB508BE9ABB8BB72265553EB46CF3439CF85EF3C6F6C5FF1A22B39A64B47BF000ED495D08CC3428896F6A1C9382C39E662C81FE085BF695D6842CF81341DB2A89A3CBC925277A0C9BE2C77939041BEBE807694F3740B032B7A69B5B0B7179F2F7F5DCE02B51F9DBE9C236006DBE9527BAEA8BED85821F7D66CD2E051B4D5D38E5AF8087282092032C2230CB0A31E20387706E84626D37B9D3130D5A497C2DE500BC7E07E90D57A09F80490B9250D38CB86C1F546A14C3EDE1C73C0843C28799F47E62AC3497B881A66AB0342CF1679ED7095B709B6),
('78646df8-9c27-1572-6ac3-0dbf216cb4f3', '10079e41-a3e4-75ff-bcd2-053a04a33a88', -31925, NULL, 12507, 38595, -1243668567, NULL, 1382782515, NULL, -3087096581176634022, 1578508211703644570, 12091159821409646592, 658054436413290496, 0.4827388352787690, 0.09229320173773420, 0.930997133, NULL, 0.75805399142956631, 0.97796885287437718, 164, NULL, -10, -113, 0, NULL, '嵅', '嗈', NULL, NULL, 6, 1, '2022-03-19 17:01:29.458386', '2022-04-04 08:22:27.084511', '2022-03-19 11:44:40.086632+02:00', '2022-04-26 10:47:21.621928+02:00', '149:54:37.430252', NULL, 0x34A384820734FB2CBBC248128F19487A3E0F378E240AF72045AB330D6B08811C7C32DA56B65ECE58897C1C4438B056843231813C9D9CB5BCA5333A1269758FEFBC5AB1E8EABAD3C9D663CF5A52A07D836719357E35A19B9971004DC49F56B77D42B41957770F5849A7402AAE21EED77192BEFF8711A1088AF7B3FB0ED772C4E6D4912E3FA39BAE02FAB8221D7EFDC6356F004A12A157003C3030C5B3E62740C8DABBE5B47D85E793C9F3C234F2BCB858160A3F306E8397416D113AE86D6E51C71A2B3774AF88D005AF79246E46976C889D532442059E1F8B1718D16D942A8AAABD3B1523DCB94CDF3CD220DA18F83DB5CAE55014976F113D20FF77F4E4946B2EEBA74CC062A984805700F2513E8CA0092A71B961E2DE1C3D2F1123924C7DB9715FC277A6861965C33E1E7915E7E18E66417AD25E1E21B004AE380C875AE443C4EEF1DB8D503464CBD91B29F19FBF357099A86FE23278455951D6759F032FB7B3CF6F014C523F2C186A87D9820FA8FF1412C576E30CA37BB2EA136BD55BBF97DD13ECEE57CBF6C29179635FD1D1822C8491140A3FF7D769C5079B78AA705F02640C42D552C6074A0794EF32D54FA7A32F3F899AC0162690D7C76668E42291C6EBEC82CBD2DBE15AE76747F5BD814247AAC541680906483FC8A2AE075735A86F8B2A712A5EB52B933F39CB163000A6B7084BFDB6460458839FA7499338646605761251E97A811528A7685A76CC4A3D4A565F7F73C8FA632E5679C1C3B4856D6A92A770DA3A2E898FC9894776467880D9FF51F0182F424C3908CD9E81CCBAB3EB5882FD3DC2D0B430FA91A2F3CF6E89B5884238DFB9A3A94EBDEDB299B297DE60F83182AC5D538297E438FE20BA8C50A18858E87DCB6C1B06C4F9EFED0D8A4517334441290649482DE13BCFCAC2BC205EAF2327DF3B6A5D69E82BE5EFDAD3B9D35315ACF1102AD949924C572FF3D7F92871E67E8B32F2BFD18DC788EDF36B9688CDD1CAB2E89DD1528A979D6B10F7DB152B93F36486D917824CA9CB49449D6E7F47AFBA1E46EABE5052439EA29AF876F5BA22F4160FA603DDC1C5D6159F72B9DA7DA47816B5F678CA66C7C03DF6648F411D9921DF91B6AD7B9604A9C23FB1FE56D1D4F8CB5B21E3B71AC2E8A5896EA5D491AB77D793D4AECC7403CA840361D9FC4732B64F2D45F68070CBDED21A938CE19E4A4B893C945733FA642BAF0D82765FA0FE67DA5C38C42584B4867F61414555C2F50E8CE7BB6363D148E62985B1A3BFA1D8759C14C35C6564782F691824E7BFC0AE59A154564140109E9B48D8F2CE715594EF56F1C0820C512F720FB99D7D3E50A8AE40D910E194A774D9E016431F67EAFAAB5AC35C7C781179AFF888961A5398125460966F368A8DFA8E82A92C105635B5D5A4CF97711959, 0xB064407113C24FBB0F7757720012C24A5A7D434835D82F326FFA5457DAA62E9CBAB8960E024E6505171DD792F9A93C2A686D899889D1D0BE30606E66DCA1CCCAE99FA83E057BDC4E649DBB61023E5AC75E8A383D7E887A506DA2B4987FE1FE51E6DDBFE5A6E6899DD7BC345DD76F2E77CD7907373DEEF37AD8319657EAF028AC7FE1CE2457C93C051BDB86293CB3D03341E60ACD866E9DDABF97B219647119677DD2DB96CDFF790952F207C70FF25360BA418E4B8FDB600025A57CE80EDD5F7FEC90913CFE8694B56B7C5EC41CD12EF367C6F03B534E0A42FE3E3D3457271072183C78AE771FB3C719B7A5C886D0E2DA1095829377B10602F1E99062BC48BF1E89655B510644EF2AE9094C94C6C426102F64D274F59FAD3E6097976B484B1ABD8E0387C377ADF4990BE66FA2FE35E6A09BE29D9D404AD3F913A9EDEFCC0FFDCE87E362DB9FC4F11CB72A96CF8942BC96782A533F11D8789234B901BE45A01CB43066D67DF3305D2A82C7C11898424B8A24627F7209C88EC1103E7FBD048F67C29D06B726A5C64FF08AD927201917465A86344144E73AA4C5891679FB630261547916D2B99C5B14478AA2EF71958B35284A8A0DCCCAE5F1BF1E3F6DAEA48909A974E42608DDB90253DD19B87C2B0BFC30948F3851DF11D4C20E14880308697850AF566963F330FBFCFD5180C912DD6FB344FDCEB66CD1BB5A1CB21F42CF17388AA79D9C7BD538D767F6E006EAEA849A4DCE73C1ED59478DF64A60D5AA845AC9ADAF97412A94D25E6E4BFE18A2B3FF835AAC575B8CD7B2B2B1F9F2292833E8841D462B585A03903C8F31AEA062019ABC3F23D41E09F353485FD4E3F67EAFD28F909E1D0ABA1F550B41EAF4E6739D79546DE5D0AB223750A40103912C755F80C923529DC6A568F14BC3902CD00AF2F4B59BD243903E261D1C77DD2C23704F28DA406404311DBEB5E0B551F4138FAEB567CA3A5A0F7F4B44E6B3552648DB0DE7623B2C918EF0E816F93295E1C6286569BBF7778B830CBE213982AD2C8CEA46DA0FDBD7FE3C67BB163CFFD7E0C104C088EDD2536D1C6DF5F8214A424D2E5AA3CE1969C1BE38BB7CC7D263383DFC439FF2A988B644E4947ABF12DA19B6B57FF394BC03AA6B42F200C038D03B80515B1D86B478FEC2B3C60F0D4BCFE35D76A55C2725295B1819F9A1D0A375C60FD95653A353A68BAE4FA8254D05C6AB5B7D4EB7631F43D74988ECC7F56CBEAD6BE83557509942834AD548CB22538AD30295662738D3B27C2A290A283144BEDF17E582F63437733FC183C2E80DC3876FDA174C1A8880A98A16880F76D7C29D59262D299E9E7BCC08FF61CE2497DE8AD2DF293FA14AD4256BC4CD447C4D769D8911D432B41E492626C71A34C4309A203EA0A612C943EC66DB53C2B172CFD35C);",
            sql);
    }
}