using Mega.WebService.GraphQL.Tests.Models.Interfaces.Safety;

namespace Mega.WebService.GraphQL.Tests.Models.Safety
{
    public class SafeField<T> : ISafeField<T>
    {
        protected T _value = default(T);
        public ISafeClass SafeClass { get; }

        public SafeField(ISafeClass safeClass)
        {
            SafeClass = safeClass;
        }

        public T Get()
        {
            lock (SafeClass.ObjectLock)
            {
                return _value;
            }
        }
        public void Set(T value)
        {
            lock (SafeClass.ObjectLock)
            {
                _value = value;
                SafeClass.Update();
            }
        }

        public virtual void Reset()
        {
            _value = default(T);
        }
    }
}
