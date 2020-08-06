using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using DLaB.Xrm.FilterPlugin.Entities;
using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Source.DLaB.Xrm;

namespace DLaB.Xrm.Filter.Plugins.Poco
{
    [Serializable]
    [DataContract]
    public class AttributeFormatConfig
    {
        [DataMember(IsRequired = true, Name = "attributes")]
        public List<AttributeFormat> Attributes { get; set; }
        [DataMember(IsRequired = true, Name = "format")]
        public string Format { get; set; }
        [DataMember(IsRequired = true, Name = "nameAttribute")]
        public string NameAttribute { get; set; }


        public void InitializeFromLookupView(IOrganizationService service, string entityLogicalName)
        {
            var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Entity,
                LogicalName = entityLogicalName,
                RetrieveAsIfPublished = true
            });
            NameAttribute = response.EntityMetadata.PrimaryNameAttribute;

            SetAttributesAndFormat(service, entityLogicalName);
        }

        private void SetAttributesAndFormat(IOrganizationService service, string entityLogicalName)
        {
            var layout = GetLookupViewLayoutXml(service, entityLogicalName);
            PopulateAttributes(layout);
            SetFormat();
        }

        private static string GetLookupViewLayoutXml(IOrganizationService service, string entityLogicalName)
        {
            var fetch = service.GetFirstOrDefault<SavedQuery>(q => new
                {
                    q.LayoutXml
            },
                SavedQuery.Fields.QueryType, UserQueryQueryType.LookupView,
                new ConditionExpression(SavedQuery.Fields.LayoutXml, ConditionOperator.NotNull),
                SavedQuery.Fields.ReturnedTypeCode, entityLogicalName)?.LayoutXml;
            if (string.IsNullOrWhiteSpace(fetch))
            {
                throw new Exception($"No Configuration set, and no Lookup View found for entity '{entityLogicalName}'!");
            }

            return fetch;
        }

        private void PopulateAttributes(string layout)
        {
            var doc = new XmlDocument();
            doc.LoadXml(layout);
            var attributeNames = doc.SelectNodes("grid/row/cell")?.Cast<XmlNode>()
                                    .Take(3)
                                    .Select(n => n.Attributes
                                                  ?.Cast<XmlAttribute>()
                                                  .FirstOrDefault(a => a.Name == "name")?.Value) ?? new string[0];
            foreach (var att in attributeNames)
            {
                var format = new AttributeFormat
                {
                    Attribute = att
                };
                if (Attributes.Count > 0)
                {
                    format.Prefix = " - ";
                }

                Attributes.Add(format);
            }
        }

        private void SetFormat()
        {
            for (var i = 0; i < Attributes.Count; i++)
            {
                Format += "{" + i + "}";
            }
        }
    }
}
