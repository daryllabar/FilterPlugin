using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Filter.Plugins.Common;
using DLaB.Xrm.Filter.Plugins.Poco;
using DLaB.Xrm.FilterPlugin.Entities;
using DLaB.Xrm.FilterPlugin.Plugin;
using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Source.DLaB.Common;
using Source.DLaB.Xrm;
using Source.DLaB.Xrm.Plugin;

namespace DLaB.Xrm.Filter.Plugins
{
    public class FormatLookup: ConfigDataPlugin<AttributeFormatConfig>, IPlugin
    {
        public struct Variables
        {
            public static readonly string IsLookupFilterRequest = "IsLookupFilterRequest";
            public static readonly string AttributesAdded = "AttributesAdded";
        }

        #region Constructors

        public FormatLookup(string unsecureConfig = null, string secureConfig = null) : base(unsecureConfig, secureConfig)
        {
        }

        #endregion Constructors

        protected override IEnumerable<RegisteredEvent> CreateEvents()
        {
            return new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.RetrieveMultiple)
                   .WithExecuteAction(OnPreExecute)
                   .And(PipelineStage.PostOperation, MessageType.RetrieveMultiple)
                   .WithExecuteAction(OnPostExecute).Build();
        }

        protected override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);
            if (string.IsNullOrWhiteSpace(SecureConfig)
                && string.IsNullOrWhiteSpace(UnsecureConfig)
                && SecureConfigData == null
                && UnsecureConfigData == null)
            {
                InitializeDefaultConfigData(serviceProvider);
            }
        }

        private void InitializeDefaultConfigData(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.CreateOrganizationService();
            var context = serviceProvider.GetService<IPluginExecutionContext>();
            var entityLogicalName = context.PrimaryEntityName;
            ConfigData.InitializeFromLookupView(service, entityLogicalName);
        }

        public override AttributeFormatConfig CreateDefaultConfigData()
        {
            return new AttributeFormatConfig
            {
                Attributes = new List<AttributeFormat>()
            };
        }

        protected override void ExecuteInternal(ExtendedPluginContext context) { throw new NotImplementedException("Invalid registration Event!"); }

        private void OnPreExecute(ExtendedPluginContext context)
        {
            var request = new RetrieveMultipleRequest
            {
                Parameters = context.InputParameters
            };
            var result = new QueryRequestParser().Parse(request.Query, ConfigData.NameAttribute);
            switch (result.Result)
            {
                case QueryRequestParserResult.LookupFilterResult.Exception:
                    context.Trace("An exception occured attempting to parse");
                    context.Trace(result.Exception.ToString());
                    return;
                case QueryRequestParserResult.LookupFilterResult.NotFetchExpression:
                    context.Trace("Not a Fetch Expression.  Exiting... ");
                    return;
                case QueryRequestParserResult.LookupFilterResult.Success:
                    ProcessLookupFilterOnPreOp(context, result);
                    context.Trace("Fetch Xml:");
                    context.Trace(result.FetchXml);
                    break;
                default:
                    context.Trace("Fetch Xml:");
                    context.Trace(result.FetchXml);
                    context.Trace($"Not a Filter Lookup Fetch Expression because {result.Result}.  Exiting... ");
                    return;
            }

            // <fetch version="1.0" mapping="logical" no-lock="false"><entity name="contact"><attribute name="fullname"/><attribute name="contactid"/><order attribute="fullname" descending="false" /><filter type="or"><condition attribute="fullname" operator="like" value="%Tom%" /></filter></entity></fetch>
        }

        private void ProcessLookupFilterOnPreOp(ExtendedPluginContext context, QueryRequestParserResult result)
        {
            context.Trace("Processing Lookup Filter");
            context.SharedVariables.Add(Variables.IsLookupFilterRequest, true.ToString());
            UpdateFetchQueryToIncludeMissingSettingAttributes(context, result);
        }

        private void UpdateFetchQueryToIncludeMissingSettingAttributes(ExtendedPluginContext context, QueryRequestParserResult result)
        {
            // Update the select to include attributes.
            var includedAttributes = new HashSet<string>(result.Attributes);
            var attributesAdded = new List<string>();
            foreach (var att in ConfigData.Attributes
                                          .Select(a => a.Attribute)
                                          .Where(a => !includedAttributes.Contains(a)))
            {
                result.XmlDoc.AddAttributeToFetch(att);
                attributesAdded.Add(att);
            }

            context.Trace(attributesAdded.Count == 0
                ? "All required attributes already exist in the query."
                : $"Attribute {string.Join(", ", attributesAdded)} have been added to the query.");
            context.SharedVariables.Add(Variables.AttributesAdded, attributesAdded.ToCsv());
            result.Fetch.Query = result.FetchXml;
        }

        private void OnPostExecute(ExtendedPluginContext context)
        {
            if (context.GetFirstSharedVariable<string>(Variables.IsLookupFilterRequest) != true.ToString())
            {
                context.Trace("IsLookupFilter was not set.  Exiting...");
                return;
            }

            var results = context.GetOutputParameterValue<EntityCollection>("BusinessEntityCollection");
            var formatter = new AttributeFormatter();
            var addedAttributesString = context.GetFirstSharedVariable<string>(Variables.AttributesAdded);
            var addedAttributes = addedAttributesString.Split(',').Select(a => a.Trim()).ToArray();
            context.Trace($"Applying format for {results.Entities.Count} entities.");
            foreach (var entity in results.Entities)
            {
                entity[ConfigData.NameAttribute] = formatter.Format(entity, ConfigData);
                foreach (var addedAtt in addedAttributes)
                {
                    entity.Attributes.Remove(addedAtt);
                }
            }
            context.Trace($"Added Attributes {addedAttributesString} removed");
        }
    }
}
