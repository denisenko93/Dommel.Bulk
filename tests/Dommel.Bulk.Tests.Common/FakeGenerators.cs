﻿using Bogus;

namespace Dommel.Bulk.Tests.Common;

public static class FakeGenerators
{
    public static readonly Faker<Person> PersonFaker = new Faker<Person>()
            .RuleFor(x => x.Ref, Guid.NewGuid)
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.LastName, f => f.Person.LastName)
            .RuleFor(x => x.Age, f => f.Person.Random.Number(100))
            .RuleFor(x => x.Gender, f => f.Person.Gender)
            .RuleFor(x => x.BirthDay, f => f.Person.DateOfBirth);

    public static readonly Faker<AllTypesEntity> AllTypesFaker = new Faker<AllTypesEntity>()
        .StrictMode(true)
        .RuleFor(x => x.Id, f => Guid.NewGuid())
        .RuleFor(x => x.Ref, f => Guid.NewGuid())
        .RuleFor(x => x.Short, f => f.Random.Short())
        .RuleFor(x => x.ShortNull, f => f.Random.Short().OrNull(f))
        .RuleFor(x => x.UShort, f => f.Random.UShort())
        .RuleFor(x => x.UShortNull, f => f.Random.UShort().OrNull(f))
        .RuleFor(x => x.Int, f => f.Random.Int())
        .RuleFor(x => x.IntNull, f => f.Random.Int().OrNull(f))
        .RuleFor(x => x.UInt, f => f.Random.UInt())
        .RuleFor(x => x.UIntNull, f => f.Random.UInt().OrNull(f))
        .RuleFor(x => x.Long, f => f.Random.Long())
        .RuleFor(x => x.LongNull, f => f.Random.Long().OrNull(f))
        .RuleFor(x => x.ULong, f => f.Random.ULong())
        .RuleFor(x => x.ULongNull, f => f.Random.ULong().OrNull(f))
        .RuleFor(x => x.Decimal, f => f.Random.Decimal())
        .RuleFor(x => x.DecimalNull, f => f.Random.Decimal().OrNull(f))
        .RuleFor(x => x.Float, f => f.Random.Float())
        .RuleFor(x => x.FloatNull, f => f.Random.Float().OrNull(f))
        .RuleFor(x => x.Double, f => f.Random.Double())
        .RuleFor(x => x.DoubleNull, f => f.Random.Double().OrNull(f))
        .RuleFor(x => x.Byte, f => f.Random.Byte())
        .RuleFor(x => x.ByteNull, f => f.Random.Byte().OrNull(f))
        .RuleFor(x => x.SByte, f => f.Random.SByte())
        .RuleFor(x => x.SByteNull, f => f.Random.SByte().OrNull(f))
        .RuleFor(x => x.Bool, f => f.Random.Bool())
        .RuleFor(x => x.BoolNull, f => f.Random.Bool().OrNull(f))
        .RuleFor(x => x.Char, f => f.Random.Char(max:'\uD7FF'))
        .RuleFor(x => x.CharNull, f => f.Random.Char(max:'\uD7FF').OrNull(f))
        .RuleFor(x => x.String, f => f.Random.String2(100, 200, "abcdefghijklmnopqrstuvwxyz\r\n\t\b\0'\"\\"+(char)26).OrNull(f))
        .RuleFor(x => x.StringNull, f => f.Random.String2(100, 200, "abcdefghijklmnopqrstuvwxyz\r\n\t\b\0'\"\\"+(char)26).OrNull(f))
        .RuleFor(x => x.Enum, f => f.Random.Enum<DayOfWeek>())
        .RuleFor(x => x.EnumNull, f => f.Random.Enum<DayOfWeek>().OrNull(f))
        .RuleFor(x => x.DateTime, f => f.Date.Recent())
        .RuleFor(x => x.DateTimeNull, f => f.Date.Future().OrNull(f))
        .RuleFor(x => x.TimeSpan, f => f.Date.Timespan())
        .RuleFor(x => x.TimeSpanNull, f => f.Date.Timespan().OrNull(f))
#if NET6_0_OR_GREATER
       .RuleFor(x => x.DateOnly, f => f.Date.RecentDateOnly())
        .RuleFor(x => x.DateOnlyNull, f => f.Date.FutureDateOnly().OrNull(f))
        .RuleFor(x => x.TimeOnly, f => f.Date.RecentTimeOnly())
        .RuleFor(x => x.TimeOnlyNull, f => f.Date.RecentTimeOnly())
#endif
        .RuleFor(x => x.ByteArray, f => f.Random.Bytes(1000))
        .RuleFor(x => x.ByteArrayNull, f => f.Random.Bytes(1000).OrNull(f))
        ;
}