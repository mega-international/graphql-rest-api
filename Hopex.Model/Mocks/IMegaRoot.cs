using Mega.Macro.API;
using Mega.Macro.API.Drawings;
using Mega.Macro.API.Utils;
using System;
using System.Collections.Generic;

namespace Hopex.Model.Mocks
{
    public interface ISupportsDiagnostics
    {
        void AddGeneratedERQL(string erql);
        IEnumerable<string> GeneratedERQLs { get; }
    }

    public interface IMegaWrapperObject
    {
        void InvokePropertyPut(string property, params object[] args);
        T InvokeFunction<T>(string function, params object[] args);
    }

    public interface IMegaWizardContext : IMegaWrapperObject
    {

    }

    public interface IMegaItem : IMegaWrapperObject
    {
        IMegaRoot Root { get; }

        T CallFunction<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : IMegaWrapperObject;
        T CallFunctionValue<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : struct;
        string CallFunctionString(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null);

    }

    public interface IMegaObject : IMegaItem
    {
        MegaId GetClassId();
        IMegaCollection GetCollection(MegaId linkId, int sortDirection1 = 1, string sortAttribute1 = null, int sortDirection2 = 1, string sortAttribute2 = null);
        string GetPropertyValue(MegaId propertyId, string format = "ASCII");
        void SetPropertyValue(MegaId propertyId, string value);
    }

    public interface IMegaCollection : IMegaItem
    {
        IMegaObject Item(int index);
    }

    public interface IMegaRoot : IMegaObject
    {
        IMegaCurrentEnvironment CurrentEnvironment { get; }
        IMegaObject GetObjectFromId(MegaId objectId);
        IMegaObject GetCollectionDescription(MegaId collectionId);

        IMegaDrawingFactory GetDrawingFactory();
    }    

    public interface IMegaCurrentEnvironment : IMegaWrapperObject
    {
        IMegaToolkit Toolkit { get; }
        IMegaSite Site { get; }
        string EnvironmentPath { get; }
    }

    public interface IMegaSite : IMegaWrapperObject
    {
        IMegaVersionInformation VersionInformation { get; }
    }

    public interface IMegaVersionInformation : IMegaWrapperObject
    {
        string Name { get; }
    }

    public interface IMegaToolkit
    {
        bool IsSameId(MegaId objectId1, MegaId objectId2);
    }

    internal class RealMegaWrapperObject : IMegaWrapperObject
    {
        protected MegaWrapperObject _realWrapperObject;

        internal RealMegaWrapperObject(MegaWrapperObject realWrapperObject)
        {
            _realWrapperObject = realWrapperObject;
        }
        
        public void InvokePropertyPut(string property, params object[] args)
        {
            _realWrapperObject.InvokePropertyPut(property, args);
        }

        public T InvokeFunction<T>(string function, params object[] args)
        {
            return _realWrapperObject.InvokeFunction<T>(function, args);
        }
    }

    internal class RealMegaItem : RealMegaWrapperObject, IMegaItem
    {
        protected MegaItem _realItem => (MegaItem) _realWrapperObject;

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

        private I CallFunction<I, R>(Func<R,I> creator, MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
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
    }
       

    internal class RealMegaObject : RealMegaItem, IMegaObject
    {
        internal MegaObject RealObject => (MegaObject)_realWrapperObject;

        public RealMegaObject(MegaObject megaObject) : base(megaObject) { }       

        public MegaId GetClassId()
        {
            var classId = RealObject.GetClassId<double>();
            return MegaId.Create(classId);
        }

        public IMegaCollection GetCollection(MegaId linkId, int sortDirection1 = 1, string sortAttribute1 = null, int sortDirection2 = 1, string sortAttribute2 = null)
        {
            return new RealMegaCollection(RealObject.GetCollection(linkId, sortDirection1, sortAttribute1, sortDirection2, sortAttribute2));
        }

        public string GetPropertyValue(MegaId propertyId, string format = "ASCII")
        {
            return RealObject.GetPropertyValue(propertyId, format);
        }

        public void SetPropertyValue(MegaId propertyId, string value)
        {
            RealObject.SetPropertyValue(propertyId, value);
        }
    }

    internal class RealWizardContext : RealMegaWrapperObject, IMegaWizardContext
    {
        private MegaWizardContext RealWizard => (MegaWizardContext) _realWrapperObject;

        public RealWizardContext(MegaWizardContext realWizard) : base(realWizard) { }
    }

    internal class RealMegaCollection : RealMegaItem, IMegaCollection
    {

        private MegaCollection RealCollection => (MegaCollection)_realWrapperObject;

        public RealMegaCollection(MegaCollection megaCollection) : base(megaCollection) { }


        public IMegaObject Item(int index)
        {
            return new RealMegaObject(RealCollection.Item(index));
        }
    }

    internal class RealMegaRoot : RealMegaObject, IMegaRoot
    {
        private MegaRoot RealRoot => (MegaRoot) _realWrapperObject;

        public IMegaCurrentEnvironment CurrentEnvironment => new RealMegaCurrentEnvironment(RealRoot.CurrentEnvironment);

        public RealMegaRoot(MegaRoot realRoot) : base(realRoot) { }
       

        public IMegaObject GetObjectFromId(MegaId objectId)
        {
            return new RealMegaObject(RealRoot.GetObjectFromId<MegaObject>(objectId));
        }

        public IMegaObject GetCollectionDescription(MegaId collectionId)
        {
            return new RealMegaObject(RealRoot.GetCollectionDescription(collectionId));
        }

        public IMegaDrawingFactory GetDrawingFactory()
        {
            return new RealMegaDrawingFactory();
        }
    }

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
}
