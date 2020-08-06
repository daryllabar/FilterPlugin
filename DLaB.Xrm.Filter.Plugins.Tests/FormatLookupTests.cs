using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Filter.Plugins.Poco;
using DLaB.Xrm.FilterPlugin.Entities;
using DLaB.Xrm.FilterPlugin.Test;
using DLaB.Xrm.FilterPlugin.Test.Assumptions;
using DLaB.Xrm.FilterPlugin.Test.Builders;
using DLaB.Xrm.Plugin;
using DLaB.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Filter.Plugins.Tests
{
    [TestClass]
    public class FormatLookupTests
    {
        [TestMethod]
        public void FormatLookup_InvalidConfig_Should_SkipExecution()
        {
            //
            // Arrange
            //
            TestInitializer.InitializeTestSettings();
            var plugin = new FormatLookup(@"Invalid Config");
            var context = new PluginExecutionContextBuilder()
                          .WithFirstRegisteredEvent(plugin)
                          .WithPrimaryEntityName(Contact.EntityLogicalName).Build();
            var serviceProvider = new ServiceProviderBuilder(TestBase.GetOrganizationService(), context, new FakeTraceService(new DebugLogger())).Build();

            //
            // Act
            //
            plugin.Execute(serviceProvider);


            //
            // Assert
            //
            var logger = (FakeTraceService) serviceProvider.GetService<ITracingService>();
            Assert.IsTrue(logger.Traces.Any(t => t.Trace == "Execution Has Been Skipped!"));
        }

        [TestMethod]
        public void FormatLookup_ValidConfig_Should_Parse()
        {
            //
            // Arrange / Act
            //
            var plugin = new FormatLookup($@"{{
    ""attributes"":[
        {{
            ""attribute"":""{Contact.Fields.FullName}"",
            ""maxLength"":40
        }}, {{
            ""attribute"":""{Contact.Fields.AccountId}"",
            ""maxLength"":30,
            ""prefix"":"" - ""
        }}, {{
            ""attribute"":""{Contact.Fields.Address1_StateOrProvince}"",
            ""prefix"":"" ""
        }}],
    ""format"":""{{0}}{{1}}{{2}}"",
    ""nameAttribute"":""{Contact.Fields.FullName}""
}}");

            //
            // Assert
            //
            Assert.AreEqual("{0}{1}{2}", plugin.ConfigData.Format);
            Assert.AreEqual(Contact.Fields.FullName, plugin.ConfigData.NameAttribute);
            var attributes = plugin.ConfigData.Attributes;
            Assert.AreEqual(3, attributes.Count, "3 attributes should have been parsed.");

            var fullName = attributes[0];
            Assert.AreEqual(Contact.Fields.FullName, fullName.Attribute);
            Assert.AreEqual(40, fullName.MaxLength);
            Assert.AreEqual(null, fullName.Prefix);

            var account = attributes[1];
            Assert.AreEqual(Contact.Fields.AccountId, account.Attribute);
            Assert.AreEqual(30, account.MaxLength);
            Assert.AreEqual(" - ", account.Prefix);

            var state = attributes[2];
            Assert.AreEqual(Contact.Fields.Address1_StateOrProvince, state.Attribute);
            Assert.AreEqual(0, state.MaxLength);
            Assert.AreEqual(" ", state.Prefix);
        }

        [TestMethod]
        public void FormatLookup_ContactFormatLookupPre_Should_AddAttributesAndVariables()
        {
            //
            // Arrange
            //
            var plugin = new FormatLookup
            {
                UnsecureConfigData = GetContactSettings()
            };
            var fetchExpression = GetFetchExpression();
            var context = new PluginExecutionContextBuilder().WithFirstPreOpEvent(plugin)
                                                             .WithInputParameter("Query", fetchExpression).Build();
            var serviceProvider = new ServiceProviderBuilder(null, context, new DebugLogger()).Build();
        
            //
            // Act
            //
            plugin.Execute(serviceProvider);
        
            //
            // Assert
            //
            context = serviceProvider.GetService<IPluginExecutionContext>();
            Assert.AreEqual(true.ToString(), context.GetFirstSharedVariable(FormatLookup.Variables.IsLookupFilterRequest), $"The {FormatLookup.Variables.IsLookupFilterRequest} flag should have been set!");
            Assert.AreEqual(ContactAttributesAdded, context.GetFirstSharedVariable(FormatLookup.Variables.AttributesAdded), $"The {FormatLookup.Variables.AttributesAdded} variable should have been set!");
            Assert.IsTrue(fetchExpression.Query.Contains($@"<attribute name=""{Contact.Fields.AccountId}"" />"), "The Account Id Attribute should have been added to the query.");
            Assert.IsTrue(fetchExpression.Query.Contains($@"<attribute name=""{Contact.Fields.Address1_StateOrProvince}"" />"), "The State Attribute should have been added to the query.");
        }

        [TestMethod]
        public void FormatLookup_MissingIsFormatLookupFlag_Should_Exit()
        {
            //
            // Arrange
            //
            var plugin = new FormatLookup
            {
                UnsecureConfigData = GetContactSettings()
            };
            var context = new PluginExecutionContextBuilder().WithFirstPostOpEvent(plugin).Build();
            var serviceProvider = new ServiceProviderBuilder(null, context, new DebugLogger()).Build();

            //
            // Act
            //
            plugin.Execute(serviceProvider);

            //
            // Assert
            //
            var logger = (FakeTraceService)serviceProvider.GetService<ITracingService>();
            Assert.IsTrue(logger.Traces.Any(t => t.Trace == "IsLookupFilter was not set.  Exiting..."));
        }

        [TestMethod]
        public void FormatLookup_ContactFormatLookupPost_Should_Format()
        {
            //
            // Arrange
            //
            var plugin = new FormatLookup
            {
                UnsecureConfigData = GetContactSettings()
            };
            var contact = new Contact
            {
                FullName = "John Doe",
                AccountId = new EntityReference(),
                Address1_StateOrProvince = "Alaska"
            };
            var entities = new EntityCollection(new List<Entity> {contact});
            contact.FormattedValues.Add(Contact.Fields.AccountId, "Acme");
            var context = new PluginExecutionContextBuilder()
                          .WithFirstPostOpEvent(plugin)
                          .WithOutputParameter("BusinessEntityCollection", entities)
                          .WithSharedVariables(
                              FormatLookup.Variables.IsLookupFilterRequest, true.ToString(),
                              FormatLookup.Variables.AttributesAdded, ContactAttributesAdded).Build();
            var serviceProvider = new ServiceProviderBuilder(null, context, new DebugLogger()).Build();
        
            //
            // Act
            //
            plugin.Execute(serviceProvider);
        
            //
            // Assert
            //
            var result = entities.Entities.First().ToEntity<Contact>();
            Assert.AreEqual("John Doe - Acme, Alaska", result.FullName, "Full name should have been formatted based on config.");
            Assert.IsFalse(result.Attributes.ContainsKey(Contact.Fields.AccountId), "Added account id should have been removed.");
            Assert.IsFalse(result.Attributes.ContainsKey(Contact.Fields.Address1_StateOrProvince), "Added state should have been removed.");
        }

        #region MissingConfigPreOp_Should_PopulateConfigFromLookup

        [TestMethod]
        public void FormatLookup_MissingConfigPreOp_Should_PopulateConfigFromLookup()
        {
            new MissingConfigPreOp_Should_PopulateConfigFromLookup().Test();
        }

        [SavedQuery_ContactLookup]
        // ReSharper disable once InconsistentNaming
        private class MissingConfigPreOp_Should_PopulateConfigFromLookup : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var plugin = new FormatLookup();
                var context = new PluginExecutionContextBuilder()
                              .WithFirstPreOpEvent(plugin)
                              .WithPrimaryEntityName(Contact.EntityLogicalName).Build();
                var serviceProvider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(serviceProvider);

                //
                // Assert
                //
                Assert.AreEqual(plugin.ConfigData.NameAttribute, Contact.Fields.FullName);
                Assert.AreEqual("{0}{1}{2}", plugin.ConfigData.Format);
                var attributes = plugin.ConfigData.Attributes.ToArray();
                var att = attributes[0];
                Assert.AreEqual(att.Attribute, Contact.Fields.FullName, "The fullname attribute should have been found in the lookup query fetch xml, and added to the config Attributes.");
                Assert.IsTrue(string.IsNullOrWhiteSpace(att.Prefix));
                att = attributes[1];
                Assert.AreEqual(att.Attribute, Contact.Fields.EMailAddress1, "The email address attribute should have been found in the lookup query fetch xml, and added to the config Attributes.");
                Assert.AreEqual(att.Prefix, " - ");
                att = attributes[2];
                Assert.AreEqual(att.Attribute, Contact.Fields.Telephone1, "The telephone attribute should have been found in the lookup query fetch xml, and added to the config Attributes.");
                Assert.AreEqual(att.Prefix, " - ");
            }
        }

        #endregion MissingConfigPreOp_Should_PopulateConfigFromLookup

        private static string ContactAttributesAdded => $"{Contact.Fields.AccountId}, {Contact.Fields.Address1_StateOrProvince}";

        private static FetchExpression GetFetchExpression()
        {
            const string xml = @"<fetch version=""1.0"" mapping=""logical"" no-lock=""false""><entity name=""contact""><attribute name=""fullname""/><attribute name=""contactid""/><order attribute=""fullname"" descending=""false"" /><filter type=""or""><condition attribute=""fullname"" operator=""like"" value=""%%"" /></filter></entity></fetch>";
            return new FetchExpression(xml);
        }

        private static AttributeFormatConfig GetContactSettings()
        {
            return new AttributeFormatConfig
            {
                Attributes = new List<AttributeFormat> {
                    new AttributeFormat { Attribute = Contact.Fields.FullName, MaxLength = 30, },
                    new AttributeFormat { Attribute = Contact.Fields.AccountId, Prefix = " - ", MaxLength = 40, },
                    new AttributeFormat { Attribute = Contact.Fields.Address1_StateOrProvince, Prefix = ", " },
                },
                Format = "{0}{1}{2}",
                NameAttribute = Contact.Fields.FullName
            };
        }
    }
}
