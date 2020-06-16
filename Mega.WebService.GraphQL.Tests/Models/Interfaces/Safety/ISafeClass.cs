namespace Mega.WebService.GraphQL.Tests.Models.Interfaces.Safety
{
    public interface ISafeClass
    {
        object ObjectLock { get; }
        void Reset();
        void Update();
    }
}
