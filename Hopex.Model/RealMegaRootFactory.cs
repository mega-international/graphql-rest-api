using Hopex.Model.Abstractions;
using Mega.Macro.API;
using Mega.Macro.API.Enums;
using Mega.Macro.API.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hopex.Model
{
    public class RealMegaRootFactory
    {
        public static IMegaRoot FromNativeRoot(object nativeRoot)
        {
            return new RealMegaRoot(MegaWrapperObject.Cast<MegaRoot>(nativeRoot));
        }
    }

    internal class RealMegaCurrentEnvironment : RealMegaWrapperObject, IMegaCurrentEnvironment
    {
        private MegaCurrentEnvironment RealEnvironment => (MegaCurrentEnvironment)_realWrapperObject;

        public RealMegaCurrentEnvironment(MegaCurrentEnvironment realEnvironment) : base(realEnvironment) { }

        public IMegaToolkit Toolkit => new RealToolkit(RealEnvironment.Toolkit);

        public IMegaSite Site => new RealMegaSite(RealEnvironment.Site);

        public string EnvironmentPath => RealEnvironment.EnvironmentPath;

        public IMegaResources Resources => new RealMegaResources(RealEnvironment.Resources);

        public dynamic GetMacro(string macroId)
        {
            return RealEnvironment.NativeObject.GetMacro(macroId);
        }
    }

    internal class RealMegaSite : RealMegaWrapperObject, IMegaSite
    {
        private MegaSite RealSite => (MegaSite)_realWrapperObject;

        public RealMegaSite(MegaSite realSite) : base(realSite) { }

        public IMegaVersionInformation VersionInformation => new RealMegaVersionInformation(RealSite.VersionInformation);
    }

    internal class RealMegaVersionInformation : RealMegaWrapperObject, IMegaVersionInformation
    {
        private MegaVersionInformation RealVersionInformation => (MegaVersionInformation)_realWrapperObject;

        public RealMegaVersionInformation(MegaVersionInformation realVersionInformation) : base(realVersionInformation) { }

        public string Name => RealVersionInformation.Name;
    }

    internal class RealToolkit : IMegaToolkit
    {
        private MegaToolkit _realToolkit;

        public RealToolkit(MegaToolkit toolkit)
        {
            _realToolkit = toolkit;
        }

        public bool IsSameId(MegaId objectId1, MegaId objectId2)
        {
            return _realToolkit.IsSameId(objectId1, objectId2);
        }
    }

    internal class RealMegaResources : RealMegaWrapperObject, IMegaResources
    {
        public RealMegaResources(MegaResources realResources) : base(realResources) { }
    }

    internal class RealMegaWrapperObject : IMegaWrapperObject
    {
        protected MegaWrapperObject _realWrapperObject;

        public dynamic NativeObject => _realWrapperObject.NativeObject;

        internal RealMegaWrapperObject(MegaWrapperObject realWrapperObject)
        {
            _realWrapperObject = realWrapperObject;
        }

        public void InvokeMethod(string method, params object[] args)
        {
            _realWrapperObject.InvokeMethod(method, args);
        }

        public void InvokePropertyPut(string property, params object[] args)
        {
            _realWrapperObject.InvokePropertyPut(property, args);
        }

        public T InvokeFunction<T>(string function, params object[] args)
        {
            return _realWrapperObject.InvokeFunction<T>(function, args);
        }

        public void Dispose()
        {
            _realWrapperObject.Dispose();
        }
    }

    internal class RealMegaItem : RealMegaWrapperObject, IMegaItem
    {
        protected MegaItem _realItem => (MegaItem)_realWrapperObject;

        public virtual IMegaRoot Root => new RealMegaRoot(_realItem.Root);

        internal RealMegaItem(MegaItem realItem) : base(realItem) { }

        public T CallFunction<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
            where T : IMegaWrapperObject
        {
            if (typeof(T) == typeof(IMegaWizardContext))
                return (T)CallFunction<IMegaWizardContext, MegaWizardContext>(r => new RealWizardContext(r), methodId, arg1, arg2, arg3, arg4, arg5, arg6);

            if (typeof(T) == typeof(IMegaRoot))
                return (T)CallFunction<IMegaRoot, MegaRoot>(r => new RealMegaRoot(r), methodId, arg1, arg2, arg3, arg4, arg5, arg6);

            if (typeof(T) == typeof(IMegaObject))
                return (T)CallFunction<IMegaObject, MegaObject>(r => new RealMegaObject(r), methodId, arg1, arg2, arg3, arg4, arg5, arg6);

            if (typeof(T) == typeof(IMegaCollection))
                return (T)CallFunction<IMegaCollection, MegaCollection>(r => new RealMegaCollection(r), methodId, arg1, arg2, arg3, arg4, arg5, arg6);

            if (typeof(T) == typeof(IMegaItem))
                return (T)CallFunction<IMegaItem, MegaItem>(r => new RealMegaItem(r), methodId, arg1, arg2, arg3, arg4, arg5, arg6);

            if (typeof(T) == typeof(IMegaWrapperObject))
                return (T)CallFunction<IMegaWrapperObject, MegaWrapperObject>(r => new RealMegaWrapperObject(r), methodId, arg1, arg2, arg3, arg4, arg5, arg6);

            throw new NotImplementedException();
        }

        private I CallFunction<I, R>(Func<R, I> creator, MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
            where I : IMegaWrapperObject
            where R : MegaWrapperObject, new()
        {
            var nativeResult = _realItem.CallFunction<R>(methodId, arg1, arg2, arg3, arg4, arg5, arg6);
            return creator(nativeResult);
        }

        public string CallFunctionString(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            var wrappedResult = _realItem.CallFunction<MegaWrapperObject>(methodId, arg1, arg2, arg3, arg4, arg5, arg6);
            return wrappedResult?.NativeObject as string;
        }

        public T CallFunctionValue<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : struct
        {
            var wrappedResult = _realItem.CallFunction<MegaWrapperObject>(methodId, arg1, arg2, arg3, arg4, arg5, arg6);
            if (wrappedResult == null)
                return default(T);
            else
                return (T)wrappedResult.NativeObject;
        }

        public bool ConditionEvaluate(MegaId methodId)
        {
            return _realItem.NativeObject.ConditionEvaluate($"ApplyTest({methodId})");
        }

        public IMegaObject GetTypeObject()
        {
            return new RealMegaObject(_realItem.GetTypeObject());
        }
    }
    
    internal class RealMegaObject : RealMegaItem, IMegaObject
    {
        internal MegaObject RealObject => (MegaObject)_realWrapperObject;

        public RealMegaObject(MegaObject megaObject) : base(megaObject) { }

        public bool Exists => RealObject.Exists;

        public string MegaField => RealObject.MegaField;

        public string MegaUnnamedField => RealObject.MegaUnnamedField;

        public bool IsConfidential => RealObject.NativeObject.IsConfidential;

        public bool IsAvailable => RealObject.NativeObject.CallFunction("IsAvailable");

        public MegaId Id => RealObject.Id;
        
        public MegaId GetClassId()
        {
            var classId = RealObject.GetClassId<double>();
            return MegaId.Create(classId);
        }

        public bool IsSameId(MegaId objectId)
        {
            return RealObject.IsSameId(objectId);
        }

        public IMegaCollection GetCollection(MegaId linkId, int sortDirection1 = 1, string sortAttribute1 = null, int sortDirection2 = 1, string sortAttribute2 = null)
        {
            return new RealMegaCollection(RealObject.GetCollection(linkId, sortDirection1, sortAttribute1, sortDirection2, sortAttribute2));
        }

        public string GetPropertyValue(MegaId propertyId, string format = "ASCII")
        {
            return RealObject.GetPropertyValue(propertyId, format);
        }

        public T GetPropertyValue<T>(MegaId propertyId, string format = "internal")
        {
            return RealObject.GetPropertyValue<T>(propertyId, format);
        }

        public dynamic GetFormated(string propertyId, string format)
        {
            return RealObject.NativeObject.GetFormated(propertyId, format);
        }

        public IMegaAttribute GetAttribute(MegaId propertyId)
        {
            return new RealMegaAttribute(RealObject.GetAttribute(propertyId));
        }

        public void SetPropertyValue(MegaId propertyId, string value)
        {
            RealObject.SetPropertyValue(propertyId, value);
        }

        public void SetPropertyValue(MegaId propertyId, object value, string format = "internal")
        {
            RealObject.SetPropertyValue(propertyId, value, format);
        }

        public void Delete(string options = "")
        {
            RealObject.Delete(options);
        }

        public void CallMethod(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            RealObject.CallMethod(methodId, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public IMegaObject GetPhysicalType()
        {
            return new RealMegaObject(RealObject.GetPhysicalType());
        }
    }

    internal class RealMegaAttribute : RealMegaWrapperObject, IMegaAttribute
    {
        internal MegaAttribute RealAttribute => (MegaAttribute)_realWrapperObject;

        public RealMegaAttribute(MegaAttribute megaAttribute) : base(megaAttribute) { }

        public IMegaAttribute Translate(MegaId languageId)
        {
            return new RealMegaAttribute(RealAttribute.Translate(languageId));
        }

        public dynamic Value()
        {
            return RealAttribute.NativeObject.Value();
        }
    }

    internal class RealWizardContext : RealMegaWrapperObject, IMegaWizardContext
    {
        private MegaWizardContext RealWizard => (MegaWizardContext)_realWrapperObject;

        public RealWizardContext(MegaWizardContext realWizard) : base(realWizard) { }

        public WizardCreateMode Mode
        { get => RealWizard.Mode; set => RealWizard.Mode = value; }

        public object Create()
        {
            return InvokeFunction<double>("Create");
        }
    }

    internal class RealMegaCollection : RealMegaItem, IMegaCollection
    {

        private MegaCollection RealCollection => (MegaCollection)_realWrapperObject;

        public RealMegaCollection(MegaCollection megaCollection) : base(megaCollection) { }


        public IMegaObject Item(int index)
        {
            return new RealMegaObject(RealCollection.Item(index));
        }

        public IMegaObject Item(MegaId objectId)
        {
            return new RealMegaObject(RealCollection.Item(objectId));
        }

        public IEnumerator<IMegaObject> GetEnumerator()
        {
            foreach (MegaObject realObject in RealCollection)
                yield return new RealMegaObject(realObject);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IMegaObject Create(MegaId objectId = null, string paramId1 = null, string paramValue1 = null, string paramId2 = null, string paramValue2 = null)
        {
            return new RealMegaObject(RealCollection.Create(objectId, paramId1, paramValue1, paramId2, paramValue2));
        }

        public IMegaObject Add(MegaId objectId, MegaId propertyId = null)
        {
            var updatedCollection = propertyId == null ? RealCollection.Add(objectId) : RealCollection.Add(objectId, propertyId);
            return new RealMegaObject(updatedCollection);
        }

        public void RemoveChild(MegaId id)
        {
            RealCollection.NativeObject.Item(id.Value).Remove();            
        }

        public IMegaCollection GetType(string targetMetaClassId)
        {
            return new RealMegaCollection(MegaWrapperObject.Cast<MegaCollection>(RealCollection.NativeObject.GetType(targetMetaClassId)));
        }
    }

    internal class RealMegaRoot : RealMegaObject, IMegaRoot
    {
        internal MegaRoot RealRoot => (MegaRoot)_realWrapperObject;

        public IMegaCurrentEnvironment CurrentEnvironment => new RealMegaCurrentEnvironment(RealRoot.CurrentEnvironment);

        public RealMegaRoot(MegaRoot realRoot) : base(realRoot) { }


        public IMegaObject GetObjectFromId(MegaId objectId)
        {
            return new RealMegaObject(RealRoot.GetObjectFromId<MegaObject>(objectId));
        }

        public IMegaObject GetClassDescription(MegaId classId)
        {
            return new RealMegaObject(RealRoot.GetClassDescription(classId));
        }

        public IMegaObject GetCollectionDescription(MegaId collectionId)
        {
            return new RealMegaObject(RealRoot.GetCollectionDescription(collectionId));
        }

        public IMegaDrawingFactory GetDrawingFactory()
        {
            return new RealMegaDrawingFactory();
        }

        public IMegaCollection GetSelection(string query, int sortDirection1 = 1, string sortCriterion1 = null, int sortDirection2 = 1, string sortCriterion2 = null, int sortDirection3 = 1, string sortCriterion3 = null)
        {
            return new RealMegaCollection(RealRoot.GetSelection(query, sortDirection1, sortCriterion1, sortDirection2, sortCriterion2, sortDirection3, sortCriterion3));
        }
    }
}
