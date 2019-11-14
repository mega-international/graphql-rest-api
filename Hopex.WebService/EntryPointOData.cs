//using Hopex.ApplicationServer.WebServices;
//using Hopex.DataModel;
//using Hopex.DataModel.Serializers;
//using Hopex.MetaModel;
//using Hopex.Modules;
//using Newtonsoft.Json;
//using System;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Hopex.WebService
//{
//    [HopexWebService(WebServiceRoute)]
//    [HopexMacro(MacroId = "C340C36F5C870BB7")]
//    public class EntryPointOData : HopexWebService<InputArguments>
//    {
//        private const string WebServiceRoute = "data";

//        public override async Task<HopexResponse> Execute(InputArguments args)
//        {
//            try
//            {
//                var parts = ExtractParts();
//                if (parts.Length < 2)
//                {
//                    return HopexResponse.Error(400, "You must provide a valid path");
//                }

//                var hopexSchemaManager = new HopexSchemaManager(CreateSchemaLoader(), new PivotConvertor());

//                var dataModel = CreateDataModel(await hopexSchemaManager.GetSchemaAsync("itpm"));

//                IHasCollection current = dataModel;
//                IClassDescription currentSchema = null;
//                IModelCollection currentCollection = null;
//                for (int ix = 1; ix < parts.Length; ix++)
//                {
//                    var part = parts[ix];
//                    if (currentCollection == null)
//                    {
//                        currentSchema = current.MetaModel.GetClassDescription(part);
//                        currentCollection = await dataModel.GetCollectionAsync(part);
//                        return HopexResponse.Error(400, $"{part} not found");
//                    }
//                    else
//                    {
//                        current = currentCollection.Item(part);
//                        if (current?.Exists == false)
//                        {
//                            return HopexResponse.Error(400, $"{part} not found");
//                        }
//                        coll.Dispose();
//                        coll = null;
//                    }
//                }
//                StringBuilder sb = new StringBuilder();
//                StringWriter sw = new StringWriter(sb);

//                using (JsonWriter writer = new JsonTextWriter(sw))
//                {
//                    writer.Formatting = Formatting.Indented;
//                    if (coll != null)
//                    {
//                        writer.WriteStartArray();
//                        foreach (MegaObject item in coll)
//                        {
//                            SerializeObject(writer, item, currentSchema);
//                        }
//                        writer.WriteEndArray();
//                    }
//                    else
//                    {
//                        SerializeObject(writer, current, currentSchema);
//                    }
//                }

//                return HopexResponse.Json(sb.ToString());
//            }
//            catch (Exception ex)
//            {
//                Logger?.LogError(ex);
//                return HopexResponse.Text(ex.Message, statusCode: 500);
//            }
//        }

//        private void SerializeObject(JsonWriter writer, IModelElement item, IClassDescription currentSchema)
//        {
//            writer.WriteStartObject();
//            foreach (var prop in currentSchema.Properties)
//            {
//                writer.WritePropertyName(prop.Name);
//                writer.WriteValue(item.GetValue<object>(prop));
//            }
//            writer.WriteEndObject();
//        }

//        protected virtual ISchemaLoader CreateSchemaLoader()
//        {
//            return new ResourcesLoader(typeof(EntryPoint).Assembly, "Hopex.Modules.GraphQL.Data");
//        }

//        protected virtual IHopexDataModel CreateDataModel(IHopexMetaModel metaModel)
//        {
//            return new HopexDataModel(metaModel, HopexContext.NativeRoot);
//        }

//        private string[] ExtractParts()
//        {
//            var path = HopexContext.Request.Path.Substring($"/api/{WebServiceRoute}/".Length);
//            return path.Split('/');
//        }


//    }
//}
