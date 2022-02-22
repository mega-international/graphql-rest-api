using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API.Library;
using System;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    class IntCount {}

    class CountNode : Node<IDictionary<string, object>>
    {
        private readonly IPathDescription _lastPath;

        private readonly List<Tuple<ComparatorNode, ComparatorNode>> _childs = new List<Tuple<ComparatorNode, ComparatorNode>>();

        public CountNode(IPathDescription lastPath, object filterValue) : base(filterValue)
        {
            _lastPath = lastPath;
        }

        public override void Build()
        {
            var attributeName = GetAttributeName();
            foreach(var item in Value)
            {
                var (_, countOperator) = GetOperator(item.Key);
                var countValue = (int)item.Value;
                
                var compareNode = new ComparatorNode($"{attributeName} HAVING COUNT", countOperator, item.Value, typeof(IntCount));
                Tuple<ComparatorNode, ComparatorNode> pair;

                if(countOperator.Name == "=" && countValue == 0
                    || countOperator.Name == "Not=" && countValue != 0
                    || countOperator.Name == "<" && countValue > 0
                    || countOperator.Name == "<=" && countValue >= 0
                    || countOperator.Name == ">=" && countValue <= 0)
                {
                    var nullCompareNode = new ComparatorNode($"{attributeName}", GetOperator("_empty").Item2, true, null);
                    pair = new Tuple<ComparatorNode, ComparatorNode>(compareNode, nullCompareNode);
                }
                else
                {
                    pair = new Tuple<ComparatorNode, ComparatorNode>(compareNode, null);
                }
                _childs.Add(pair);
            }

            foreach(var child in _childs)
            {
                child.Item1.Build();
                child.Item2?.Build();
            }
        }

        public override string GetQuery()
        {
            var many = _childs.Count > 1;
            var body = CreateQueryWithSeparator(Logical.AND, _childs, c =>
            {
                if(c.Item2 != null)
                {
                    return $"{(many ? "(" : "")}{c.Item1.GetQuery()} OR {c.Item2.GetQuery()}{(many ? ")" : "")}";
                }
                else
                {
                    return $"{c.Item1.GetQuery()}";
                }
            });
            return $"{GetQueryStart()}{body}{GetQueryEnd()}";
        }

        private string GetQueryStart()
        {
            return _lastPath.Condition == null ? "" :
                $"{_lastPath.RoleId}[{_lastPath.RoleName}]:{_lastPath.TargetSchemaId}[{_lastPath.TargetSchemaName}].(";
        }

        private string GetQueryEnd()
        {
            return _lastPath.Condition == null ? "" : ")";
        }

        private string GetAttributeName()
        {
            var condition = _lastPath.Condition;
            return condition == null ?
                $"{_lastPath.RoleId}[{_lastPath.RoleName}]" :
                $"{condition.RoleId}[{condition.RoleName}]:{condition.MetaClassId}[{condition.MetaClassName}].{MetaAttributeLibrary.AbsoluteIdentifier} = \"~{condition.ObjectFilterId}\"";
        }
    }
}
