using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Mega.Macro.API.Library;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace Hopex.WebService.Tests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CustomDirectives_should : MockRootBasedFixture
    {
        private async Task ConvertName(string name, string expected, string convertorName)
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.ShortName, name))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{application{name @" + convertorName + @"}}");
            result.Should().MatchGraphQL("data.application[0].name", expected);
        }

        [Theory]
        [InlineData("foo bar", "Foo Bar")]
        [InlineData("fOo BAR", "Foo BAR")]
        [InlineData("fOo--bAR", "Foo--Bar")]
        [InlineData("i want tO capiTalizE a STring PLEASE!!!", "I Want To Capitalize A String PLEASE!!!")]
        public async void Capitalize_name(string name, string expected)
        {
            await ConvertName(name, expected, "capitalize");
        }

        [Theory]
        [InlineData("fôô bàr", "foo bar")]
        [InlineData("ça va?", "ca va?")]
        public async void Deburr_name(string name, string expected)
        {
            await ConvertName(name, expected, "deburr");
        }

        [Theory]
        [InlineData("foo bar", "FooBar")]
        [InlineData("-foo1-bar_", "Foo1Bar")]
        [InlineData("je m'appelle pascal. j'aime démarrer par des majuscules.", "JeMappellePascalJaimeDémarrerParDesMajuscules")]
        public async void Convert_name_ToPascalCase(string name, string expected)
        {
            await ConvertName(name, expected, "pascalCase");
        }

        [Theory]
        [InlineData("foo bar", "fooBar")]
        [InlineData("-foo1-bar_", "foo1Bar")]
        [InlineData("je m'appelle camel, la première fois c'est 1 minuscule.", "jeMappelleCamelLaPremièreFoisCest1Minuscule")]
        public async void Convert_name_ToCamelCase(string name, string expected)
        {
            await ConvertName(name, expected, "camelCase");
        }

        [Theory]
        [InlineData("foo bar", "foo-bar")]
        [InlineData("foo.bar_foo::bar", "foo-bar-foo-bar")]
        [InlineData("1 kebab: sauce samouraÏ", "1-kebab-sauce-samouraï")]
        public async void Convert_name_ToKebabCase(string name, string expected)
        {
            await ConvertName(name, expected, "kebabCase");
        }

        [Theory]
        [InlineData("foo bar", "foo_bar")]
        [InlineData("foo-bar-foo-bar", "foo_bar_foo_bar")]
        [InlineData("J'ai 1 Serpent Dans Ma Botte!", "j_ai_1_serpent_dans_ma_botte")]
        public async void Convert_name_ToSnakeCase(string name, string expected)
        {
            await ConvertName(name, expected, "snakeCase");
        }

        [Theory]
        [InlineData("FOO BAR", "foo bar")]
        [InlineData("Tout-EN_MINUSCULE, VoilÀ!", "tout-en_minuscule, voilà!")]
        public async void Convert_name_ToLower(string name, string expected)
        {
            await ConvertName(name, expected, "toLower");
        }

        [Theory]
        [InlineData("FOO BAR", "fOO BAR")]
        [InlineData("La première en minuscule!", "la première en minuscule!")]
        public async void Convert_name_ToLowerFirst(string name, string expected)
        {
            await ConvertName(name, expected, "lowerFirst");
        }

        [Theory]
        [InlineData("foo bar", "FOO BAR")]
        [InlineData("tout-En_Majuscule, Voilà!", "TOUT-EN_MAJUSCULE, VOILÀ!")]
        public async void Convert_name_ToUpper(string name, string expected)
        {
            await ConvertName(name, expected, "toUpper");
        }

        [Theory]
        [InlineData("foo bar", "Foo bar")]
        [InlineData("la première en majuscule!", "La première en majuscule!")]
        public async void Convert_name_ToUpperFirst(string name, string expected)
        {
            await ConvertName(name, expected, "upperFirst");
        }

        [Theory]
        [InlineData("  foo bar  ", "foo bar")]
        public async void Trim_name(string name, string expected)
        {
            await ConvertName(name, expected, "trim");
        }

        [Theory]
        [InlineData("  foo bar  ", "foo bar  ")]
        public async void Trim_start_name(string name, string expected)
        {
            await ConvertName(name, expected, "trimStart");
        }

        [Theory]
        [InlineData("  foo bar  ", "  foo bar")]
        public async void Trim_end_name(string name, string expected)
        {
            await ConvertName(name, expected, "trimEnd");
        }

        [Fact]
        public async void Format_phone_numbers()
        {
            await ConvertName("8005551212", "(800) 555-1212", @"phone(format:""{0:(###) ###-####}"")");
        }

        [Fact]
        public async void Format_numbers()
        {
            int i = 1500;
            long l = 1500L;
            double d = 1500.404d;
            float f = 1500.404f;
            decimal m = 1500.404m;
            var format = "0,000.00";

            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), i))
                .Build();
            var result = await ExecuteQueryAsync(root, @"query{application{cost @number(format:""" + format + @""")}}");
            result.Should().MatchGraphQL("data.application[0].cost", i.ToString(format, CultureInfo.InvariantCulture));

            root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), l))
                .Build();
            result = await ExecuteQueryAsync(root, @"query{application{cost @number(format:""" + format + @""")}}");
            result.Should().MatchGraphQL("data.application[0].cost", l.ToString(format, CultureInfo.InvariantCulture));

            root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), d))
                .Build();
            result = await ExecuteQueryAsync(root, @"query{application{cost @number(format:""" + format + @""")}}");
            result.Should().MatchGraphQL("data.application[0].cost", d.ToString(format, CultureInfo.InvariantCulture));

            root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), f))
                .Build();
            result = await ExecuteQueryAsync(root, @"query{application{cost @number(format:""" + format + @""")}}");
            result.Should().MatchGraphQL("data.application[0].cost", f.ToString(format, CultureInfo.InvariantCulture));

            root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), m))
                .Build();
            result = await ExecuteQueryAsync(root, @"query{application{cost @number(format:""" + format + @""")}}");
            result.Should().MatchGraphQL("data.application[0].cost", m.ToString(format, CultureInfo.InvariantCulture));
        }

        [Fact]
        public async void Format_currencies()
        {
            int i = 1500;
            long l = 1500L;
            double d = 1500.404d;
            float f = 1500.404f;
            decimal m = 1500.404m;
            var format = "0,000.00";
            
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), i))
                .Build();
            var result = await ExecuteQueryAsync(root, @"query{application{cost @currency(format:""" + format + @""")}}");
            result.Should().MatchGraphQL("data.application[0].cost", $"{i.ToString(format, CultureInfo.InvariantCulture)}");

            
            root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), i))
                .Build();
            result = await ExecuteQueryAsync(root, @"query{application{cost @currency(format:""" + format + @""" currency:""€"")}}");
            result.Should().MatchGraphQL("data.application[0].cost", $"{i.ToString(format, CultureInfo.InvariantCulture)} €");
            
            root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), l))
                .Build();
            result = await ExecuteQueryAsync(root, @"query{application{cost @currency(format:""" + format + @""" currency:""€"")}}");
            result.Should().MatchGraphQL("data.application[0].cost", $"{l.ToString(format, CultureInfo.InvariantCulture)} €");
            
            root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), d))
                .Build();
            result = await ExecuteQueryAsync(root, @"query{application{cost @currency(format:""" + format + @""" currency:""€"")}}");
            result.Should().MatchGraphQL("data.application[0].cost", $"{d.ToString(format, CultureInfo.InvariantCulture)} €");
            
            root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), f))
                .Build();
            result = await ExecuteQueryAsync(root, @"query{application{cost @currency(format:""" + format + @""" currency:""€"")}}");
            result.Should().MatchGraphQL("data.application[0].cost", $"{f.ToString(format, CultureInfo.InvariantCulture)} €");

            root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.Cost.Substring(1, 12), m))
                .Build();
            result = await ExecuteQueryAsync(root, @"query{application{cost @currency(format:""" + format + @""" currency:""€"")}}");
            result.Should().MatchGraphQL("data.application[0].cost", $"{m.ToString(format, CultureInfo.InvariantCulture)} €");
        }

        [Fact]
        public async void Format_DateTimes()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.CreationDate.Substring(1, 12), new DateTime(2020, 12, 31)))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{application{creationDate @date(format:""" + "yyyy/MM/dd" + @""")}}");
            result.Should().MatchGraphQL("data.application[0].creationDate", "2020/12/31");
        }

        [Fact]
        public async void Format_DateTimeOffsets()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.CreationDate.Substring(1, 12), new DateTimeOffset(new DateTime(2020, 12, 31)).AddHours(1).AddMinutes(2).AddSeconds(3)))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{application{creationDate @date(format:""" + "yyyy/MM/dd HH:mm:ss" + @""")}}");
            result.Should().MatchGraphQL("data.application[0].creationDate", "2020/12/31 01:02:03");
        }

        [Fact]
        public async void Format_TimeSpans()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.CreationDate.Substring(1, 12), new TimeSpan(1,2,3)))
                .Build();
            var result = await ExecuteQueryAsync(root, @"query{application{creationDate @date(format:""" + @"hh:mm:ss" + @""")}}");

            result.Should().MatchGraphQL("data.application[0].creationDate", "01:02:03");
        }

        [Fact(Skip = "WI 42652: temporarly suspended")]
        public async void Is_read_only_property_ok()
        {
            var mockMegaObject = new Mock<MockMegaObject>(MegaId.Create("IubjeRlyFfT1"), (MegaId) MetaClassLibrary.Application) {CallBase = true};
            mockMegaObject.Setup(x => x.CallFunctionString("~R2mHVReGFP46[WFQuery]", MetaAttributeLibrary.ShortName.Substring(0, 13), null, null, null, null, null)).Returns("R").Verifiable();

            var root = new MockMegaRoot.Builder()
                .WithObject(mockMegaObject.Object.WithProperty(MetaAttributeLibrary.ShortName, "foo bar"))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{application{name @isReadOnly}}");
            result.Should().MatchGraphQL("data.application[0].name", "foo bar");
        }

        [Fact(Skip = "WI 42652: temporarly suspended")]
        public async void Is_read_only_property_not_ok()
        {
            var mockMegaObject = new Mock<MockMegaObject>(MegaId.Create("IubjeRlyFfT1"), (MegaId) MetaClassLibrary.Application) {CallBase = true};
            mockMegaObject.Setup(x => x.CallFunctionString("~R2mHVReGFP46[WFQuery]", MetaAttributeLibrary.ShortName.Substring(0, 13), null, null, null, null, null)).Returns("").Verifiable();

            var root = new MockMegaRoot.Builder()
                .WithObject(mockMegaObject.Object.WithProperty(MetaAttributeLibrary.ShortName, "foo bar"))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{application{name @isReadOnly}}");
            result.Should().MatchGraphQL("data.application[0].name", "Property Name is not ReadOnly.");
        }

        [Fact(Skip = "WI 42652: temporarly suspended")]
        public async void Is_read_write_property_ok()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
                    WithProperty(MetaAttributeLibrary.ShortName.Substring(1, 12), "foo bar"))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{application{name @isReadWrite}}");
            result.Should().MatchGraphQL("data.application[0].name", "foo bar");
        }

        [Fact(Skip = "WI 42652: temporarly suspended")]
        public async void Is_read_write_property_not_ok()
        {
            var mockMegaObject = new Mock<MockMegaObject>(MegaId.Create("IubjeRlyFfT1"), (MegaId) MetaClassLibrary.Application) {CallBase = true};
            mockMegaObject.Setup(x => x.CallFunctionString("~R2mHVReGFP46[WFQuery]", MetaAttributeLibrary.ShortName.Substring(0, 13), null, null, null, null, null)).Returns("").Verifiable();

            var root = new MockMegaRoot.Builder()
                .WithObject(mockMegaObject.Object.WithProperty(MetaAttributeLibrary.ShortName, "foo bar"))
                .Build();

            var result = await ExecuteQueryAsync(root, @"query{application{name @isReadWrite}}");
            result.Should().MatchGraphQL("data.application[0].name", "Property Name is not ReadWrite.");
        }
    }
}
