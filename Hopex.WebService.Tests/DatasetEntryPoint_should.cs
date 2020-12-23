using FluentAssertions;
using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model.Abstractions;
using Hopex.Modules.GraphQL;
using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Moq;
using System.Linq;
using Xunit;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Mega.Macro.API;
using System;
using Hopex.Modules.GraphQL.Dataset;

using static Mega.Macro.API.Library.MetaAttributeLibrary;
using static Hopex.WebService.Tests.Assertions.MegaIdMatchers;


namespace Hopex.WebService.Tests
{
    public class DatasetEntryPoint_should
    {
        private readonly FakeDatasetBuilder _datasetBuilder = new FakeDatasetBuilder();

        [Fact]
        public async void Returns_a_empty_dataset()
        {
            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{""header"":{""columns"": []}, ""data"" : []}");
        }
        
        [Fact]
        public async void Returns_a_dataset_with_single_column_and_no_data()
        {
            _datasetBuilder
                .WithColumnCollection("AwQjrTxCV53E", "Application");

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [{""id"": ""AwQjrTxCV53E"", ""label"": ""Application""}]},
                ""data"" : []}");
        }

        [Fact]
        public async void Returns_a_dataset_with_2_columns_and_no_data()
        {
            _datasetBuilder
                .WithColumnCollection("AwQjrTxCV53E", "Application")
                .WithColumnProperty("ZxQjsUxCVXKE", "Application Id");

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [
                    {""id"": ""AwQjrTxCV53E"", ""label"": ""Application""},
                    {""id"": ""ZxQjsUxCVXKE"", ""label"": ""Application Id""},
                ]},
                ""data"" : []}");
        }

        [Fact]
        public async void Ignore_invisible_column_in_header()
        {
            _datasetBuilder
                .WithColumnCollection("AwQjrTxCV53E", "Application", DataSetItemVisiblity.Never)
                .WithColumnProperty("ZxQjsUxCVXKE", "Application Id");

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [
                    {""id"": ""ZxQjsUxCVXKE"", ""label"": ""Application Id""},
                ]},
                ""data"" : []}");
        }

        [Fact]
        public async void Returns_a_dataset_with_single_column_and_a_single_line_of_data()
        {
            _datasetBuilder
                .WithColumnProperty("nuQjiWxCVvXE", "Application Name")
                .WithLine(new MockMegaObject("8vQjrOxCVbpB")
                    .WithProperty("nuQjiWxCVvXE", "Sample Application"));

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [{""id"": ""nuQjiWxCVvXE"", ""label"": ""Application Name""}]},
                ""data"" : [{""Application Name"": ""Sample Application""}]}");
        }

        [Fact]
        public async void Returns_a_dataset_with_2_columns_and_2_lines_of_data()
        {
            _datasetBuilder
                .WithColumnProperty("nuQjiWxCVvXE", "Application Name")
                .WithColumnProperty("ZxQjsUxCVXKE", "Application Id")
                .WithLine(new MockMegaObject("8vQjrOxCVbpB")
                    .WithProperty("nuQjiWxCVvXE", "Sample Application")
                    .WithProperty("ZxQjsUxCVXKE", "8vQjrOxCVbpB"))
                .WithLine(new MockMegaObject("XwQjGPxCVDtC")
                    .WithProperty("nuQjiWxCVvXE", "Sample Application 2")
                    .WithProperty("ZxQjsUxCVXKE", "XwQjGPxCVDtC"));

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [
                    {""id"": ""nuQjiWxCVvXE"", ""label"": ""Application Name""},
                    {""id"": ""ZxQjsUxCVXKE"", ""label"": ""Application Id""}]},
                ""data"" : [
                    {""Application Name"": ""Sample Application"", ""Application Id"": ""8vQjrOxCVbpB""},
                    {""Application Name"": ""Sample Application 2"", ""Application Id"": ""XwQjGPxCVDtC""}]}");
        }        

        [Theory]
        [InlineData(DatasetNullValues.Never,
            @"{""Name"": ""Sample""},
              {""Name"": ""Sample""}")]
        [InlineData(DatasetNullValues.FirstLine,
            @"{""Name"": ""Sample"", ""Type"": null, ""Date"": null, ""Score"": null},
              {""Name"": ""Sample""}")]
        [InlineData(DatasetNullValues.Always,
            @"{""Name"": ""Sample"", ""Type"": null, ""Date"": null, ""Score"": null},
              {""Name"": ""Sample"", ""Type"": null, ""Date"": null, ""Score"": null}")]
        public async void Control_skipping_of_empty_values(DatasetNullValues nullValues, string expectedLines)
        {
            _datasetBuilder
                .WithColumnProperty("nuQjiWxCVvXE", "Name")
                .WithColumnProperty("ZxQjsUxCVXKE", "Type")
                .WithColumnProperty("9r7sJzDPV9OV", "Date", MetaAttributeFormat.Standard, MetaAttributeType.DateTime)
                .WithColumnProperty("Lt7swzDPVTUV", "Score", MetaAttributeFormat.Standard, MetaAttributeType.Float)
                .WithLine(createObjWithEmptyValues("8vQjrOxCVbpB"))
                .WithLine(createObjWithEmptyValues("XwQjGPxCVDtC"));
            MockMegaObject createObjWithEmptyValues(string id) => new MockMegaObject(id)
                    .WithProperty("nuQjiWxCVvXE", "Sample")
                    .WithProperty("ZxQjsUxCVXKE", "")
                    .WithProperty("9r7sJzDPV9OV", "")
                    .WithProperty("Lt7swzDPVTUV", null);

            var actual = await CallEntryPoint(new DatasetArguments { NullValues = nullValues });

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [
                    {""id"": ""nuQjiWxCVvXE"", ""label"": ""Name""},
                    {""id"": ""ZxQjsUxCVXKE"", ""label"": ""Type""},
                    {""id"": ""9r7sJzDPV9OV"", ""label"": ""Date""},
                    {""id"": ""Lt7swzDPVTUV"", ""label"": ""Score""}
                ]},
                ""data"" : [" + expectedLines + "]}");
        }

        [Fact]
        public async void Disambiguate_column_names()
        {
            _datasetBuilder                            
                .WithColumnProperty("nuQjiWxCVvXE", "Application")
                .WithColumnProperty("ZxQjsUxCVXKE", "Application")
                .WithLine(new MockMegaObject("8vQjrOxCVbpB")
                    .WithProperty("nuQjiWxCVvXE", "Sample Application")
                    .WithProperty("ZxQjsUxCVXKE", "8vQjrOxCVbpB"));

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [
                    {""id"": ""nuQjiWxCVvXE"", ""label"": ""Application""},
                    {""id"": ""ZxQjsUxCVXKE"", ""label"": ""Application-1""}]},
                ""data"" : [
                    {""Application"": ""Sample Application"", ""Application-1"": ""8vQjrOxCVbpB""}]}");
        }

        [Theory]
        [InlineData("a", "a", "a", 
            "a", "a-1", "a-2")]
        [InlineData("a", "a", "a-1",
            "a", "a-2", "a-1")]
        [InlineData("a", "a", "a-1", "a-1",
            "a", "a-2", "a-1", "a-1-1")]
        public void Disambiguate_column_names_in_complex_cases(params string[] strings)
        {
            var columnCount = strings.Length / 2;
            var columns = strings
                .Take(columnCount)
                .Select(s => new DatasetColumn() { Label = s })
                .ToList();

            DatasetEntryPoint.DisambiguateColumnLabels(columns);

            var expected = strings.Skip(columnCount);
            columns.Select(c => c.Label).Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async void Export_short_name_of_collection()
        {
            _datasetBuilder
                .WithColumnCollection("nuQjiWxCVvXE", "Application")
                .WithLine(new MockMegaObject("8vQjrOxCVbpB")
                    .WithFormattedProperty("nuQjiWxCVvXE", new MegaPropertyWithFormat("8vQjrOxCVbpB")
                        .WithDisplay("Sample Application")));

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [
                    {""id"": ""nuQjiWxCVvXE"", ""label"": ""Application""}]},
                ""data"" : [
                    {""Application"": ""Sample Application""}]}");
        }

        [Fact]
        public async void Regenerate_a_dataset()
        {
            _datasetBuilder
                .WithColumnProperty("nuQjiWxCVvXE", "Application Name");
            var sequence = new MockSequence();
            _datasetBuilder.mockResources.InSequence(sequence)
                .Setup(r => r.InvokeMethod("DiscardObject", IsIdString(FakeDatasetBuilder.StructureId)));
            _datasetBuilder.spyRoot.InSequence(sequence)
                .Setup(r => r.InvokeMethod("CollectionCacheReset", IsIdString(FakeDatasetBuilder.DatasetId), IsIdString(FakeDatasetBuilder.StructureId)))
                .Callback(() => {
                    _datasetBuilder
                        .WithLine(new MockMegaObject("8vQjrOxCVbpB")
                            .WithProperty("nuQjiWxCVvXE", "Sample Application"))
                        .FillLineTypeObject();
                });

            var actual = await CallEntryPoint(new DatasetArguments { Regenerate = true });

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [{""id"": ""nuQjiWxCVvXE"", ""label"": ""Application Name""}]},
                ""data"" : [{""Application Name"": ""Sample Application""}]}");
        }

        [Fact]
        public async void Use_unordered_query_if_available()
        {
            _datasetBuilder
                .WithColumnProperty("nuQjiWxCVvXE", "Application Name");
            var unorderedValues = new MockMegaCollection("~HAEykNlBUvjS[DataSubSetCreateNoSortQuery]")
                .WithChildren(new MockMegaObject("8vQjrOxCVbpB")
                    .WithProperty("nuQjiWxCVvXE", "Unordered name"));
            _datasetBuilder.dataset.WithRelation(unorderedValues);
            _datasetBuilder.rootBuilder.WithObject(new MockMegaObject("~HAEykNlBUvjS[DataSubSetCreateNoSortQuery]"));
            _datasetBuilder.FillLineTypeObject(unorderedValues);

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [{""id"": ""nuQjiWxCVvXE"", ""label"": ""Application Name""}]},
                ""data"" : [{""Application Name"": ""Unordered name""}]}");
        }

        [Fact]
        public async void Return_error_if_dataset_does_not_exist()
        {
            var emptyRoot = new MockMegaRoot.Builder().Build();
            var entryPoint = new TestableDatasetEntryPoint(emptyRoot);

            var actual = await entryPoint.Execute(null);

            actual.Should().BeError(HttpStatusCode.BadRequest, $"Dataset {FakeDatasetBuilder.DatasetId} does not exist or is confidential");
        }

        [Fact]
        public async void Return_error_if_dataset_is_confidential()
        {
            _datasetBuilder.dataset.WithConfidentiality();

            var actual = await CallEntryPoint();

            actual.Should().BeError(HttpStatusCode.BadRequest, $"Dataset {FakeDatasetBuilder.DatasetId} does not exist or is confidential");
        }

        [Fact]
        public async void Return_error_if_dataset_is_not_readable()
        {
            _datasetBuilder.dataset.WithObjectCrud("CUD");

            var actual = await CallEntryPoint();

            actual.Should().BeError(HttpStatusCode.BadRequest, $"Dataset {FakeDatasetBuilder.DatasetId} does not exist or is confidential");
        }

        [Theory]
        [InlineData(MetaAttributeFormat.Currency, MetaAttributeType.Float)]
        [InlineData(MetaAttributeFormat.Currency, MetaAttributeType.Currency)]
        public async void Return_currency_as_decimal(MetaAttributeFormat format, MetaAttributeType type)
        {
            _datasetBuilder
                .WithColumnProperty("Nt7s)yDPVHIV", "Expenses", format, type)
                .WithLine(new MockMegaObject("8vQjrOxCVbpB")
                    .WithFormattedProperty("Nt7s)yDPVHIV", new MegaPropertyWithFormat(123m)
                        .WithDisplay("$123.00")
                        .WithAscii("123.")));

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [
                    {""id"": ""Nt7s)yDPVHIV"", ""label"": ""Expenses""}]},
                ""data"" : [
                    {""Expenses"": 123.00}]}");
        }

        [Theory]
        [InlineData(MetaAttributeType.DateTime)]
        [InlineData(MetaAttributeType.DateTime64)]
        [InlineData(MetaAttributeType.AbsoluteDateTime64)]
        public async void Return_date_in_iso_format(MetaAttributeType type)
        {
            _datasetBuilder
                .WithColumnProperty("9r7sJzDPV9OV", "Deployment date", MetaAttributeFormat.Standard, type)
                .WithLine(new MockMegaObject("8vQjrOxCVbpB")
                    .WithFormattedProperty("9r7sJzDPV9OV", new MegaPropertyWithFormat(new DateTime(2020, 09, 18))
                        .WithDisplay("9/18/2020")
                        .WithAscii("2020/09/18 12:00:00")));

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [
                    {""id"": ""9r7sJzDPV9OV"", ""label"": ""Deployment date""}]},
                ""data"" : [
                    {""Deployment date"": ""2020-09-18""}]}");
        }                

        [Theory]
        [InlineData(MetaAttributeType.Short)]
        [InlineData(MetaAttributeType.Long)]
        [InlineData(MetaAttributeType.Float)]
        public async void Return_number_as_number(MetaAttributeType type)
        {
            _datasetBuilder
                .WithColumnProperty("Lt7swzDPVTUV", "Score", MetaAttributeFormat.Standard, type)
                .WithLine(new MockMegaObject("8vQjrOxCVbpB")
                    .WithFormattedProperty("Lt7swzDPVTUV", new MegaPropertyWithFormat(123.45)
                        .WithAscii("123.45")));

            var actual = await CallEntryPoint();

            actual.Should().BeJson(@"{
                ""header"":{""columns"": [
                    {""id"": ""Lt7swzDPVTUV"", ""label"": ""Score""}]},
                ""data"" : [
                    {""Score"": 123.45}]}");
        }

        private async Task<HopexResponse> CallEntryPoint(DatasetArguments datasetArguments = null)
        {
            var root = _datasetBuilder.BuildRoot();
            var entryPoint = new TestableDatasetEntryPoint(root);
            if (datasetArguments == null) datasetArguments = new DatasetArguments();
            return await entryPoint.Execute(datasetArguments);
        }

        internal class TestableDatasetEntryPoint : DatasetEntryPoint
        {
            internal TestableDatasetEntryPoint(IMegaRoot root)
                : base()
            {
                var _request = new FakeDatasetRequest();
                (this as IHopexWebService).SetHopexContext(root, _request, new Logger());
            }

            protected override IMegaRoot GetRoot()
            {
                return (IMegaRoot)HopexContext.NativeRoot;
            }
        }

        internal class FakeDatasetRequest : BaseMockRequest
        {
            public override string Path => $"/api/dataset/{FakeDatasetBuilder.DatasetId}/content";
        }

        internal enum DataSetItemVisiblity
        {
            Visible = 0,
            NoByDefault = 1,
            Never = 2
        }

        internal class FakeDatasetBuilder
        {
            public const string DatasetId = "7IST0Q78NzfG";
            public const string StructureId = "zTd)uv)CV100";

            public readonly Mock<MockMegaRoot> spyRoot = new Mock<MockMegaRoot>() { CallBase = true };
            public readonly MockMegaRoot.Builder rootBuilder;
            public readonly Mock<IMegaResources> mockResources = new Mock<IMegaResources>();
            public readonly MockMegaObject dataset;

            private readonly MockMegaCollection _columns;
            private readonly MockMegaCollection _values;
            private readonly Dictionary<MegaId, MetaAttributeFormat> _columnsFormat = new Dictionary<MegaId, MetaAttributeFormat>(new MegaIdComparer());
            private readonly Dictionary<MegaId, MetaAttributeType> _columnsType = new Dictionary<MegaId, MetaAttributeType>(new MegaIdComparer());

            internal FakeDatasetBuilder()
            {
                _columns = new MockMegaCollection("~pjlXeYArK5LA[Report DataSet Item]");
                _values = new MockMegaCollection("~Yvazr2mvKf21[Report DataSet Create]");
                var structure = new MockMegaObject(StructureId).WithRelation(_columns);
                dataset = new MockMegaObject(DatasetId);
                rootBuilder = new MockMegaRoot.Builder(spyRoot)
                    .WithResources(mockResources.Object)
                    .WithObject(dataset
                        .WithRelation(new MockMegaCollection("~rLU9sCZtKLx6[Report DataSet Definition]")
                            .WithChildren(new MockMegaObject("9k0yC0)CV100")
                                .WithRelation(new MockMegaCollection("~NMU9ZfYtKHh6[Report DataSet Structure]")
                                    .WithChildren(structure))))
                        .WithRelation(_values));
            }

            internal MockMegaRoot BuildRoot()
            {
                FillLineTypeObject();
                return rootBuilder.Build();
            }

            internal FakeDatasetBuilder WithColumnCollection(string id, string name, DataSetItemVisiblity visible = DataSetItemVisiblity.Visible)
            {
                _columns.WithChildren(new MockMegaObject(id, "~yklXYXArK5EA[Report DataSet Collection]")
                    .WithProperty(ShortName, name)
                    .WithProperty("~Ex(oKGVvK9fG[Visible]", ((int)visible).ToString()));
                _columnsFormat.Add(id, MetaAttributeFormat.Object);
                _columnsType.Add(id, MetaAttributeType.MegaIdentifier);
                return this;
            }

            internal FakeDatasetBuilder WithColumnProperty(string id, string name, MetaAttributeFormat format = MetaAttributeFormat.Standard, MetaAttributeType type = MetaAttributeType.String)
            {
                _columns.WithChildren(new MockMegaObject(id, "~rllXkXArKrGA[Report DataSet Property]")
                    .WithProperty(ShortName, name)
                    .WithProperty("~Ex(oKGVvK9fG[Visible]", "0"));
                _columnsFormat.Add(id, format);
                _columnsType.Add(id, type);
                return this;
            }

            internal FakeDatasetBuilder WithLine(MockMegaObject mockMegaObject)
            {
                _values
                    .WithChildren(mockMegaObject);
                return this;
            }

            internal void FillLineTypeObject()
            {
                FillLineTypeObject(_values);
            }

            internal void FillLineTypeObject(MockMegaCollection values)
            {
                var properties = new MockMegaCollection("~7fs9P58ig1fC[Properties]");
                var typeObject = new MockMegaObject().WithRelation(properties);
                foreach (var column in _columns)
                    properties.WithChildren(new MockMegaObject(column.Id)
                        .WithProperty("~rhs9P5uNf1fC[Tabulated]", ((char)_columnsFormat[column.Id]).ToString())
                        .WithProperty("~mhs9P5eMf1fC[Format]", ((char)_columnsType[column.Id]).ToString()));
                foreach (MockMegaObject value in values)
                    value.WithTypeObject(typeObject);
            }
        }
    }    
}
