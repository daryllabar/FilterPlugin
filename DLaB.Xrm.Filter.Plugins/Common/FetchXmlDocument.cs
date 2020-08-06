using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DLaB.Xrm.Filter.Plugins.Common
{
    public class FetchXmlDocument
    {
        public struct Definition
        {
            public static readonly string Attribute = "attribute";
            public static readonly string AttributePath = "fetch/entity/attribute";
            public static readonly string ConditionOperatorAttribute = "operator";
            public static readonly string Entity = "entity";
            public static readonly string EntityPath = "fetch/entity";
            public static readonly string EntityFilterConditionPath = "filter/condition";
            public static readonly string LikeOperator = "like";
            public static readonly string Order = "order";
        }

        public XmlDocument Doc { get; set; }

        private List<XmlNode> _attributes;
        public List<XmlNode> Attributes => _attributes ?? (_attributes = EntityNode?.SelectNodes(Definition.Attribute)?.Cast<XmlNode>().ToList() ?? new List<XmlNode>());
        public IEnumerable<string> AttributeNames => Attributes.SelectMany(a => a.Attributes?.Cast<XmlAttribute>())
                                                               .Where(a => a.Name == "name")
                                                               .Select(a => a.Value);

        private XmlNode _entityNode;
        public XmlNode EntityNode => _entityNode ?? (_entityNode= Doc?.FirstChild?.FirstChild);

        private List<XmlNode> _filterConditions;
        public List<XmlNode> FilterConditions => _filterConditions ?? (_filterConditions = EntityNode?.SelectNodes(Definition.EntityFilterConditionPath)?.Cast<XmlNode>().ToList() ?? new List<XmlNode>());

        private List<XmlNode> _orderBys;
        public List<XmlNode> OrderBys => _orderBys ?? (_orderBys = EntityNode?.SelectNodes(Definition.Order)?.Cast<XmlNode>().ToList() ?? new List<XmlNode>());

        public XmlNode LikeCondition => FilterConditions.SelectMany(c => c.Attributes?.Cast<XmlAttribute>())
                                                        .FirstOrDefault(a => a.Name == Definition.ConditionOperatorAttribute
                                                                             && a.Value == Definition.LikeOperator);


        public void LoadXml(string xml)
        {
            Clear();
            Doc = new XmlDocument();
            Doc.LoadXml(xml);
        }

        private void Clear()
        {
            _attributes = null;
            _entityNode = null;
            _orderBys = null;
        }

        public void AddAttributeToFetch(string att)
        {
            var newAttribute = Doc.CreateElement(Definition.Attribute);
            newAttribute.SetAttribute("name", att);
            EntityNode.AppendChild(newAttribute);
            Clear();
        }
    }
}
