using Mega.WebService.GraphQL.Tests.Models.FieldModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Mega.WebService.GraphQL.Tests.Models.FakeDatas
{
    public class Container
    {
        static private int idIncrement = 1;

        private readonly JObject internalContainer;
        public Container(bool hasDatas)
        {
            if(hasDatas)
            {
                string strFileName = HttpContext.Current.Server.MapPath("~/App_Data/FakeDatas.json");
                string strFakeDatas = File.ReadAllText(strFileName);
                internalContainer = JObject.Parse(strFakeDatas);
            }
            else
            {
                internalContainer = new JObject();
            }
        }

        public JArray GetFields(string metaclassName, string kindFields)
        {
            if(metaclassName.StartsWith("Input"))
            {
                metaclassName = metaclassName.Substring(5);
            }
            return internalContainer.GetValue("Types") [metaclassName] [kindFields] as JArray;
        }

        public JArray GetAll(string metaclassName, List<Field> outputFields, List<string> idsFilter = null)
        {
            if(!internalContainer.TryGetValue(metaclassName, out JToken tokDatas))
            {
                tokDatas = new JArray();
            }
            JArray arrDatas = tokDatas as JArray;
            JArray result = new JArray();
            foreach(JObject objData in arrDatas)
            {
                if(idsFilter != null)
                {
                    string objDataId = objData.GetValue("id").ToString();
                    if(!idsFilter.Exists(idFilter => objDataId == idFilter))
                    {
                        continue;
                    }
                }
                JObject copy = new JObject();
                outputFields.ForEach(field =>
                {
                    JToken value = field.GetFakeValue(this, objData);
                    copy.Add(field.Name, value);
                });
                result.Add(copy);
            }
            return result;
        }

        public List<JObject> CreateMulti(string metaclassName, List<JObject> objs, List<Field> inputFields, List<Field> outputFields)
        {
            if(!internalContainer.TryGetValue(metaclassName, out JToken tokDatas))
            {
                tokDatas = new JArray();
                internalContainer.Add(metaclassName, tokDatas);
            }
            JArray arrDatas = tokDatas as JArray;
            List<string> newIds = new List<string>();

            objs.ForEach(obj =>
            {
                JObject newObj = new JObject();
                var properties = obj.Properties();
                foreach(var property in properties)
                {
                    if(inputFields.Exists(field => field.Name == property.Name))
                    {
                        JProperty newProperty;
                        if(property.Name == "businessProcess" || property.Name == "application")
                        {
                            JArray jarrIds = new JArray();
                            JArray jarrObjs = property.Value as JArray;
                            foreach(JObject jobj  in jarrObjs)
                            {
                                string arrId = jobj.GetValue("id").ToString();
                                jarrIds.Add(new JValue(arrId));
                            }
                            newProperty = new JProperty(property.Name, jarrIds);
                        }
                        else
                        {
                            newProperty = new JProperty(property);
                        }
                        newObj.Add(newProperty);
                    }
                }
                string newId = $"{metaclassName}-created-{idIncrement++}";
                newObj.Add("id", new JValue(newId));
                newIds.Add(newId);
                arrDatas.Add(newObj);
            });
            return GetAll(metaclassName, outputFields, newIds).ToObject<List<JObject>>();
        }

        public List<JObject> UpdateMulti(string metaclassName, List<Tuple<string, JObject>> args, List<Field> inputFields, List<Field> outputFields)
        {
            JArray arrDatas = internalContainer.GetValue(metaclassName) as JArray;
            List<string> ids = new List<string>();

            args.ForEach(idAndObj =>
            {
                string id = idAndObj.Item1;
                JObject newObj = idAndObj.Item2;
                JObject existingObj = arrDatas.First(token => id == token["id"].ToString()) as JObject;

                var newProperties = newObj.Properties();
                foreach(var newProperty in newProperties)
                {
                    if(inputFields.Exists(field => field.Name == newProperty.Name))
                    {
                        JToken newValue = null;
                        if(newProperty.Name == "businessProcess" || newProperty.Name == "application")
                        {
                            string action = newProperty.Value ["action"].ToString();
                            if(action == "ADD")
                            {
                                newValue = newProperty.Value ["list"];
                            }
                            else if(action == "REMOVE")
                            {
                                newValue = new JArray();
                            }
                        }
                        else
                        {
                            newValue = newProperty.Value;
                        }

                        JProperty existingProperty = existingObj.Property(newProperty.Name);
                        if(existingProperty == null)
                        {
                            existingObj.Add(newProperty.Name, newValue);
                        }
                        else
                        {
                            existingProperty.Value = newValue;
                        }
                    }
                }
                ids.Add(id);
            });
            return GetAll(metaclassName, outputFields, ids).ToObject<List<JObject>>();
        }

        public void DeleteMulti(string metaclassName, List<Tuple<string, bool>> args)
        {
            JArray arrDatas = internalContainer.GetValue(metaclassName) as JArray;
            args.ForEach(arg =>
            {
                string argId = arg.Item1;
                foreach(JObject objData in arrDatas)
                {
                    string dataId = objData.GetValue("id").ToString();
                    if(dataId == argId)
                    {
                        arrDatas.Remove(objData);
                        break;
                    }
                }
            });
        }
    }
}
