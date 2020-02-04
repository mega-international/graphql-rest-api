namespace Mega.WebService.GraphQL.Models
{
    public class ErrorContent
    {
        public ErrorContent() { }
        public ErrorContent(string message)
        {
            Error = message;
        }

        public string Error { get; set; }
    }
}
