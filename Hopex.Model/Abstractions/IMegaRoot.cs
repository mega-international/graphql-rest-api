using Mega.Macro.API;
using Mega.Macro.API.Enums;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hopex.Model.Abstractions
{
    public interface ISupportsDiagnostics
    {
        void AddGeneratedERQL(string erql);
        IEnumerable<string> GeneratedERQLs { get; }
    }

    public interface IMegaWrapperObject : IDisposable
    {
        dynamic NativeObject { get; }

        void InvokeMethod(string method, params object[] args);
        void InvokePropertyPut(string property, params object[] args);
        T InvokeFunction<T>(string function, params object[] args);
    }

    public interface IMegaWizardContext : IMegaWrapperObject
    {
        WizardCreateMode Mode { get; set; }
        object Create();
    }

    public interface IMegaItem : IMegaWrapperObject
    {
        IMegaRoot Root { get; }

        dynamic CallFunction(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null);
        T CallFunction<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : IMegaWrapperObject;
        T CallFunctionValue<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : struct;
        string CallFunctionString(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null);
        bool ConditionEvaluate(MegaId methodId);
        IMegaObject GetTypeObject();
    }

    public interface IMegaObject : IMegaItem
    {
        MegaId Id { get; }
        bool Exists { get; }
        string MegaField { get; }
        string MegaUnnamedField { get; }
        bool IsConfidential { get; }
        bool IsAvailable { get; }
        IMegaObject Relationship { get; }

        MegaId GetClassId();
        IMegaObject GetPhysicalType();
        bool IsSameId(MegaId objectId);
        IMegaAttribute GetAttribute(MegaId propertyId);
        IMegaCollection GetCollection(MegaId linkId, int sortDirection1 = 1, string sortAttribute1 = null, int sortDirection2 = 1, string sortAttribute2 = null, int sortDirection3 = 1, string sortAttribute3 = null);
        string GetPropertyValue(MegaId propertyId, string format = "ASCII");
        T GetPropertyValue<T>(MegaId propertyId, string format = "internal");
        dynamic GetFormated(string propertyId, string format);
        void SetPropertyValue(MegaId propertyId, string value);
        void SetPropertyValue(MegaId propertyId, object value, string format = "internal");
        void Delete(string options = "");
        void CallMethod(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null);
    }

    public interface IMegaCollection : IMegaItem, IEnumerable<IMegaObject>, IEnumerable
    {
        IMegaObject Item(int index);
        IMegaObject Item(MegaId objectId);
        IMegaObject Create(MegaId objectId = null, string paramId1 = null, string paramValue1 = null, string paramId2 = null, string paramValue2 = null);
        IMegaObject Add(MegaId objectId, MegaId propertyId = null);
        void RemoveChild(MegaId id);
        IMegaCollection GetType(string targetMetaClassId);
    }

    public interface IMegaRoot : IMegaObject
    {
        IMegaCurrentEnvironment CurrentEnvironment { get; }
        IMegaObject GetObjectFromId(MegaId objectId);
        IMegaObject GetClassDescription(MegaId classId);
        IMegaObject GetCollectionDescription(MegaId collectionId);
        IMegaCollection GetSelection(string query, int sortDirection1 = 1, string sortCriterion1 = null, int sortDirection2 = 1, string sortCriterion2 = null, int sortDirection3 = 1, string sortCriterion3 = null);
        IMegaDrawingFactory GetDrawingFactory();
    }    

    public interface IMegaCurrentEnvironment : IMegaWrapperObject
    {
        IMegaToolkit Toolkit { get; }
        IMegaSite Site { get; }
        string EnvironmentPath { get; }
        IMegaResources Resources { get; }

        dynamic GetMacro(string macroId);
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

    public interface IMegaResources : IMegaWrapperObject
    {
    }

    public interface IMegaAttribute : IMegaWrapperObject
    {
        IMegaAttribute Translate(MegaId languageId);
        dynamic Value();
        string GetFormatted(OutputFormat format, string options = null, object parser = null);
    }
}
