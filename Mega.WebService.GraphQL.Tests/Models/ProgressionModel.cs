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

    public class ProgressionModel
    {
        [JsonProperty("messageTime")]
        private string _messageTime = "";

        [JsonProperty("messageCounts")]
        private string _messageCounts = "";

        [JsonProperty("messageDetails")]
        private string _messageDetails = "";

        [JsonProperty("status")]
        private TestStatus _status = TestStatus.Running;

        private readonly object _objLock = new object();
        private TaskCompletionSource<bool> _tcs;
        private bool _updated = false;
        

        public ProgressionModel()
        {}

        public void Reset()
        {
            lock(_objLock)
            {
                _messageTime = "";
                _messageCounts = "";
                _messageDetails = "";
                _status = TestStatus.Running;
            }
            Update();
        }

        public void SetMessageTime(string message)
        {
            lock(_objLock)
            {
                _messageTime = message;
                Update();
            }
        }

        public void AddToMessageCounts(string message)
        {
            lock(_objLock)
            {
                _messageCounts += message;
                Update();
            }
        }

        public void AddToMessageDetails(string message)
        {
            lock(_objLock)
            {
                _messageDetails += message;
                Update();
            }
        }

        public void SetStatus(TestStatus status)
        {
            lock(_objLock)
            {
                _status = status;
                Update();
            }
        }

        public async Task WaitForUpdate()
        {
            lock(_objLock)
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

        private void Update()
        {
            lock(_objLock)
            {
                _updated = false;
                _tcs?.TrySetResult(true);
            }
        }

        public void Updated()
        {
            lock(_objLock)
            {
                _updated = true;
            }
        }
    }
}
