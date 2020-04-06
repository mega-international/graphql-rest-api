using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class TreeMetaClasses
    {
        private class Node
        {
            private Node _parent = null;
            private readonly HashSet<Node> _childs = new HashSet<Node>();
            private readonly MetaClass _metaclass;
            private readonly string _linkToParentName;

            public Node Parent
            {
                get => _parent;
                set
                {
                    _parent?._childs.Remove(this);
                    _parent = value;
                    _parent?._childs.Add(this);
                }
            }
            
            public Node(MetaClass metaclass, string linkName)
            {
                _metaclass = metaclass;
                _linkToParentName = linkName;
            }

            public void AddChild(Node node)
            {
                node.Parent = this;
            }

            public void RemoveChild(Node node)
            {
                node.Parent = null;
            }

            public List<Field> GenerateOutputsFields()
            {
                var outputs = new List<Field>(_metaclass.Fields);
                foreach(var child in _childs)
                {
                    var linkField = new ListField(child._linkToParentName,
                        new ObjectField(null, child.GenerateOutputsFields(), false), false);
                    outputs.Add(linkField);
                }
                return outputs;
            }

            public void FillPathsByMetaClass(ref Dictionary<MetaClass, List<Tuple<string, MetaClass>>> pathsByMetaClass)
            {
                if(!pathsByMetaClass.TryGetValue(_metaclass, out var paths))
                {
                    paths = new List<Tuple<string, MetaClass>>();
                    pathsByMetaClass.Add(_metaclass, paths);
                }
                foreach(var child in _childs)
                {
                    paths.Add(new Tuple<string, MetaClass>(child._linkToParentName, child._metaclass));
                    child.FillPathsByMetaClass(ref pathsByMetaClass);
                }
            }

            public void FillItemsByMetaClass(List<JObject> linkedItems, ref Dictionary<MetaClass, List<JObject>> itemsByMetaClass, ref HashSet<string> added)
            {
                if(!itemsByMetaClass.TryGetValue(_metaclass, out var items))
                {
                    items = new List<JObject>();
                    itemsByMetaClass.Add(_metaclass, items);
                }
                foreach(var linkedItem in linkedItems)
                {
                    var id = linkedItem.GetValue(MetaFieldNames.id).ToString();
                    if(!added.Contains(id))
                    {
                        items.Add(linkedItem);
                        added.Add(id);
                        foreach(var child in _childs)
                        {
                            var childLinkedItems = (linkedItem.GetValue(child._linkToParentName) as JArray).ToObject<List<JObject>>();
                            child.FillItemsByMetaClass(childLinkedItems, ref itemsByMetaClass, ref added);
                        }
                    }
                };
            }

            public override int GetHashCode()
            {
                return GetHashCodeFrom(_metaclass, _linkToParentName);
            }

            public override string ToString()
            {
                var value = $"Node {_metaclass.Name}";
                if(Parent != null)
                {
                    value += $" linked to {Parent._metaclass.Name} by path {_linkToParentName}";
                }
                return value;
            }

            static public int GetHashCodeFrom(MetaClass metaclass, string linkName)
            {
                var hashString = $"{metaclass.Name}<=>{linkName}";
                return hashString.GetHashCode();
            }
        }

        private readonly Node _root;
        private Node _currentNode;
        public readonly HashSet<MetaClass> MetaClasses = new HashSet<MetaClass>();

        public TreeMetaClasses(MetaClass metaClass, string linkName) : this(new Node(metaClass, linkName))
        {
            MetaClasses.Add(metaClass);
        }

        private TreeMetaClasses(Node root)
        {
            
            _root = root;
            _currentNode = root;
        }

        public void AddChildToCurrent(MetaClass metaClass, string linkName, bool moveToChild = false)
        {
            Node node = new Node(metaClass, linkName);
            MetaClasses.Add(metaClass);
            AddChildToCurrent(node, moveToChild);
        }

        private void AddChildToCurrent(Node node, bool moveToChild = false)
        {
            _currentNode.AddChild(node);
            if(moveToChild)
            {
                _currentNode = node;
            }
        }

        public void MoveCurrentToParent()
        {
            if(_currentNode.Parent != null)
            {
                _currentNode = _currentNode.Parent;
            }
        }

        public void Reset()
        {
            _currentNode = _root;
        }

        public bool MoveCurrentToParent(MetaClass metaclass, string linkName)
        {
            var temp = _currentNode;
            int hashCode = Node.GetHashCodeFrom(metaclass, linkName);
            while(_currentNode != null && hashCode != _currentNode.GetHashCode())
            {
                _currentNode = _currentNode.Parent;
            }
            bool found = _currentNode != null;
            if(!found)
            {
                _currentNode = temp;
            }
            return found;
        }

        public List<Field> GenerateOutputsFields()
        {
            return _root.GenerateOutputsFields();
        }

        public Dictionary<MetaClass, List<Tuple<string, MetaClass>>> GeneratePathsByMetaClass()
        {
            var pathsByMetaClass = new Dictionary<MetaClass, List<Tuple<string, MetaClass>>>();
            _root.FillPathsByMetaClass(ref pathsByMetaClass);
            return pathsByMetaClass;
        }

        public Dictionary<MetaClass, List<JObject>>  GetItemsByMetaClass(List<JObject> linkedItems)
        {
            var added = new HashSet<string>(); 
            var itemsByMetaClass = new Dictionary<MetaClass, List<JObject>>();
            _root.FillItemsByMetaClass(linkedItems, ref itemsByMetaClass, ref added);
            return itemsByMetaClass;
        }
    }
}
