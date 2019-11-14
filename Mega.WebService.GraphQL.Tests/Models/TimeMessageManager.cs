using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models
{
    public class TimeMessageManager
    {
        internal class Node
        {
            private Node parent = null;
            private readonly List<Node> childs = new List<Node>();
            internal string StepName { get; set; }
            internal string StepDuration { get; set; }

            internal Node(string stepName)
            {
                StepName = stepName;
            }

            internal Node CreateChild(string stepName)
            {
                Node child = new Node(stepName);
                childs.Add(child);
                child.parent = this;
                return child;
            }

            internal Node GetParent()
            {
                return parent;
            }

            internal bool IsRoot()
            {
                return parent == null;
            }

            internal string BuildMessage()
            {
                return GetMessageWithDepth(0);
            }

            private string GetMessageWithDepth(int depth)
            {
                string space = new string(' ', 2*depth).Replace(" ", "&nbsp;");
                string sub = depth > 0 ? "- " : "";
                string message = $"{space}{sub}{StepName} duration: {StepDuration}<br>";
                childs.ForEach(child => message += child.GetMessageWithDepth(depth + 1));
                return message;
            }
        }

        private Node node = null;

        public void Start(string stepName)
        {
            node = node?.CreateChild(stepName) ?? new Node(stepName);
        }

        public void End(string stepTime)
        {
            node.StepDuration = stepTime;
            if(!node.IsRoot())
            {
                node = node.GetParent();
            }
        }

        public void Reset()
        {
            node = null;
        }

        public string BuildMessage()
        {
            if(node == null)
            {
                return "";
            }

            while(!node.IsRoot())
            {
                node = node.GetParent();
            }

            return node.BuildMessage();
        }
    }
}
