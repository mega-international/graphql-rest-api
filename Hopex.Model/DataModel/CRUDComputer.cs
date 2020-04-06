using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.DataModel
{
    public class CrudComputer
    {
        public static CrudResult GetCrud(MegaObject obj)
        {
            return new CrudResult(obj.CallFunction<MegaWrapperObject>("~R2mHVReGFP46[WFQuery]", "CRUD"));
        }

        public static CrudResult GetCrud(IMegaObject obj)
        {
            return new CrudResult(obj.CallFunctionString("~R2mHVReGFP46[WFQuery]", "CRUD"));
        }

        public static CrudResult GetPropertyCrud(IMegaObject obj, IPropertyDescription property)
        {
            return new CrudResult(obj.CallFunctionString("~R2mHVReGFP46[WFQuery]", property.Id));
        }

        public static CrudResult GetPathCrud(IMegaObject obj, IPathDescription path)
        {
            return new CrudResult(obj.CallFunctionString("~R2mHVReGFP46[WFQuery]", Utils.NormalizeHopexId(path.RoleId), Utils.NormalizeHopexId(path.TargetSchemaId)));
        }

        public static CrudResult GetCollectionCrud(IMegaObject source, string collectionId, IMegaObject child)
        {
            var legId = Utils.NormalizeHopexId(collectionId);
            var targetMetaclassId = child.GetClassId().Value;
            var crud = new CrudResult(source.CallFunctionString("~R2mHVReGFP46[WFQuery]", legId, targetMetaclassId));
            return crud;
        }

        public static CrudResult GetCollectionMetaPermission(IMegaRoot root, string collectionMegaId)
        {
            return new CrudResult(root.GetCollectionDescription(collectionMegaId).CallFunctionString("~f8pQpjMDK1SP[GetMetaPermission]"));            
        }
    }

    public class CrudResult
    {
        public bool IsCreatable => _crudString.Contains("C");
        public bool IsReadable => _crudString.Contains("R");
        public bool IsUpdatable => _crudString.Contains("U");
        public bool IsDeletable => _crudString.Contains("D");

        public CrudResult(MegaWrapperObject crud)
            : this(crud?.NativeObject as string)
        { }

        public CrudResult(string crudString)
        {
            if (crudString != null)
                _crudString = crudString;
        }

        private string _crudString = "";
    }
}
