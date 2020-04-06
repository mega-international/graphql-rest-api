namespace Mega.WebService.GraphQL.Tests.Models.Interfaces.Safety
{
    public interface ISafeField<T>
    {
        ISafeClass SafeClass { get; }
        T Get();
        void Set(T value);
        void Reset();
    }

    public interface IAddableSafeField<T> : ISafeField<T>
    {
        void Add(T value);
    }


}
