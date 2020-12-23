using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model.Abstractions;
using Hopex.Model.DataModel;
using Hopex.Modules.GraphQL.Dataset;
using Mega.Macro.API.Library;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL
{
    [HopexWebService(WebServiceRoute)]
    [HopexMacro(MacroId = "AAC8AB1E5D25678E")]
    public class DatasetEntryPoint : BaseSatelliteEntryPoint<DatasetArguments>
    {
        private const string WebServiceRoute = "dataset";

        protected override string PathPrefix => $"/api/{WebServiceRoute}/";

        protected override Task<HopexResponse> ExecuteOnObject(IMegaRoot root, string objectId, string method, DatasetArguments args)
        {
            var dataset = root.GetObjectFromId(objectId);

            if (!IsExportable(dataset))
                return CreateErrorResponse(objectId);

            var structure = GetStructure(dataset);
            if (args?.Regenerate ?? false)
                RegenerateDataset(root, dataset, structure);

            var datasetDTO = InitResponseHeader(structure);
            if (args != null)
                FillValues(dataset, datasetDTO, args.NullValues);

            var content = JsonConvert.SerializeObject(datasetDTO);
            return Task.FromResult(HopexResponse.Json(content));
        }

        private static bool IsExportable(IMegaObject dataset)
        {
            return dataset.Exists
                && CrudComputer.GetCrud(dataset).IsReadable
                && !dataset.IsConfidential;
        }

        private static Task<HopexResponse> CreateErrorResponse(string objectId)
        {
            var result = new ErrorMacroResponse(HttpStatusCode.BadRequest, $"Dataset {objectId} does not exist or is confidential"); ;
            return Task.FromResult(HopexResponse.Json(JsonConvert.SerializeObject(result)));
        }

        private static IMegaObject GetStructure(IMegaObject dataset)
        {
            return dataset
                    .GetCollection("~rLU9sCZtKLx6[Report DataSet Definition]")
                    .Item(1)
                    .GetCollection("~NMU9ZfYtKHh6[Report DataSet Structure]")
                    .Item(1);
        }

        private static void RegenerateDataset(IMegaRoot root, IMegaObject dataset, IMegaObject collector)
        {
            root.CurrentEnvironment.Resources.InvokeMethod("DiscardObject", collector.MegaUnnamedField);
            root.InvokeMethod("CollectionCacheReset", dataset.MegaUnnamedField, collector.MegaUnnamedField);
        }

        private static DatasetMacroResponse InitResponseHeader(IMegaObject collector)
        {
            var datasetDTO = new DatasetMacroResponse();
            var columnsDTO = datasetDTO.Header.Columns;

            var columns = collector.GetCollection("~pjlXeYArK5LA[Report DataSet Item]");
            foreach (var column in columns)
            {
                if (column.GetPropertyValue("~Ex(oKGVvK9fG[Visible]") == "0")
                {
                    columnsDTO.Add(
                        new DatasetColumn
                        {
                            Id = column.GetPropertyValue(MetaAttributeLibrary.AbsoluteIdentifier),
                            Label = column.GetPropertyValue(MetaAttributeLibrary.ShortName)
                        });
                }
            }

            DisambiguateColumnLabels(columnsDTO);
            return datasetDTO;
        }

        internal static void DisambiguateColumnLabels(List<DatasetColumn> columnsDTO)
        {
            var nameNextSuffix = new Dictionary<string, int>();
            foreach (var column in columnsDTO)
            {
                if (!nameNextSuffix.ContainsKey(column.Label))
                    nameNextSuffix.Add(column.Label, 1);
                else
                {
                    string distinctName;
                    do
                    {
                        distinctName = column.Label + "-" + nameNextSuffix[column.Label];
                        nameNextSuffix[column.Label]++;
                    } while (columnsDTO.Any(c => c.Label == distinctName));
                    column.Label = distinctName;
                }
            }
        }

        private static void FillValues(IMegaObject dataset, DatasetMacroResponse datasetDTO, DatasetNullValues nullValues)
        {
            var lines = GetLines(dataset);
            var addNull = nullValues != DatasetNullValues.Never;
            foreach (var line in lines)
            {
                var expando = new ExpandoObject();
                var expandoDict = expando as IDictionary<string, object>;
                foreach (var column in datasetDTO.Header.Columns)
                {
                    var formatter = column.GetFormatter(line);
                    var value = formatter.Format(line);
                    if (value != null || addNull)
                        expandoDict.Add(column.Label, value);                    
                }
                datasetDTO.Data.Add(expando);
                addNull = nullValues == DatasetNullValues.Always;
            }
        }

        private static IMegaCollection GetLines(IMegaObject dataset)
        {
            if (dataset.Root.GetObjectFromId("~HAEykNlBUvjS[DataSubSetCreateNoSortQuery]").Exists)
                return dataset.GetCollection("~HAEykNlBUvjS[DataSubSetCreateNoSortQuery]");
            return dataset.GetCollection("~Yvazr2mvKf21[DataSet Create]");
        }
    }


}
