using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.DataModel
{
    class CrudComputer
    {
        static public CrudResult GetCrud(MegaObject obj)
        {
            return new CrudResult(obj.CallFunction<MegaWrapperObject>("~R2mHVReGFP46[WFQuery]", "CRUD"));
        }

        static public CrudResult GetPropertyCrud(MegaObject obj, IPropertyDescription property)
        {
            return new CrudResult(obj.CallFunction<MegaWrapperObject>("~R2mHVReGFP46[WFQuery]", property.Id));
        }

        static public CrudResult GetPathCrud(MegaObject obj, IPathDescription path)
        {
            return new CrudResult(obj.CallFunction<MegaWrapperObject>("~R2mHVReGFP46[WFQuery]", Utils.NormalizeHopexId(path.RoleId), Utils.NormalizeHopexId(path.TargetSchemaId)));
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
