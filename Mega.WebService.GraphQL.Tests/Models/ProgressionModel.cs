using Mega.WebService.GraphQL.Tests.Models.Interfaces.Safety;
using Mega.WebService.GraphQL.Tests.Models.Safety;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Models
{
    public enum TestStatus
    {
        Running,
        Failure,
        Completed
    }

    public class ProgressionModel : ISafeClass
    {
        [JsonProperty("messageTime")]
        public readonly IAddableSafeField<string> MessageTime;

        [JsonProperty("messageCounts")]
        public readonly IAddableSafeField<string> MessageCounts;

        [JsonProperty("messageDetails")]
        public readonly IAddableSafeField<string> MessageDetails;

        [JsonProperty("status")]
        [JsonConverter(typeof(SafeFieldJsonConverter<TestStatus>))]
        public readonly ISafeField<TestStatus> Status;

        [JsonProperty("error")]
        [JsonConverter(typeof(SafeFieldJsonConverter<ExceptionContent>))]
        public readonly ISafeField<ExceptionContent> Error;

        [JsonIgnore]
        public object ObjectLock { get; } = new object();

        [JsonIgnore]
        private TaskCompletionSource<bool> _tcs;

        [JsonIgnore]
        private bool _updated = false;
        

        public ProgressionModel()
        {
            MessageTime = new StringSafeField(this);
            MessageCounts = new StringSafeField(this);
            MessageDetails = new StringSafeField(this);
            Status = new SafeField<TestStatus>(this);
            Error = new SafeField<ExceptionContent>(this);
        }

        public void Reset()
        {
            lock(ObjectLock)
            {
                MessageTime.Reset();
                MessageCounts.Reset();
                MessageDetails.Reset();
                Status.Reset();
                Error.Reset();
                Update();
            }
        }

        public async Task WaitForUpdate()
        {
            lock(ObjectLock)
            {
                if(_updated)
                {
                    _tcs = new TaskCompletionSource<bool>();
                }
            }
            if(_tcs != null)
            {
                await _tcs.Task;
            }
        }

        public void Update()
        {
            lock(ObjectLock)
            {
                _updated = false;
                _tcs?.TrySetResult(true);
            }
        }

        public void Updated()
        {
            lock(ObjectLock)
            {
                _updated = true;
            }
        }
    }
}
