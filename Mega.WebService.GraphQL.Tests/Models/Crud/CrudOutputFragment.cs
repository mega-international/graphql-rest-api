namespace Mega.WebService.GraphQL.Tests.Models.Crud
{
    public class CrudOutputFragment : CrudOutput
    {
        public CrudOutputFragment(string name): base(name) {}

        public override string Serialize()
        {
            return $"...{_name}";
        }
    }
}
