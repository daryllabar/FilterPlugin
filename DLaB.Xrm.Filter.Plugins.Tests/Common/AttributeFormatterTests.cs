using System;
using System.Collections.Generic;
using DLaB.Xrm.Filter.Plugins.Common;
using DLaB.Xrm.Filter.Plugins.Poco;
using DLaB.Xrm.FilterPlugin.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Filter.Plugin.Common.Tests
{
    [TestClass]
    public class AttributeFormatterTests
    {
        public AttributeFormatter Formatter { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Formatter = new AttributeFormatter();
        }

        [TestMethod]
        public void AttributeFormatter_AllAttributeTypes_Should_Format()
        {
            var contact = new Contact
            {
                AccountId = new EntityReference(Account.EntityLogicalName, Guid.NewGuid())
                {
                    Name = "AccountName"
                },
                Address1_AddressTypeCode = Contact_Address1_AddressTypeCode.BillTo,
                Aging30 = new Money(10m),
                Address1_City = "City"
            };
            contact.FormattedValues.Add(Contact.Fields.Address1_AddressTypeCode, "BillTo");
            contact.FormattedValues.Add(Contact.Fields.AccountId, "AccountName");

            var format = Formatter.Format(contact, new AttributeFormatConfig
            {
                Attributes = new List<AttributeFormat>
                {
                    new AttributeFormat {Attribute = Contact.Fields.AccountId},
                    new AttributeFormat {Attribute = Contact.Fields.Address1_AddressTypeCode},
                    new AttributeFormat {Attribute = Contact.Fields.Aging30},
                    new AttributeFormat {Attribute = Contact.Fields.Address1_City},
                },
                Format = "Account:{0} AddressTypeCode:{1} Aging30:{2} Address1_City:{3}",
            });

            Assert.AreEqual($"Account:{contact.AccountId.Name} AddressTypeCode:{contact.Address1_AddressTypeCode} Aging30:{contact.Aging30.Value} Address1_City:{contact.Address1_City}", format);
        }

        [TestMethod]
        public void AttributeFormatter_PrefixOutput_Should_BeAttributeDependent()
        {
            var contact = new Contact
            {
                Address1_City = " "
            };
            var format = Formatter.Format(contact, new AttributeFormatConfig
            {
                Attributes = new List<AttributeFormat>
                {
                    new AttributeFormat { Attribute = Contact.Fields.Address1_City, Prefix = "|" }
                },
                Format = "{0}"
            });

            Assert.AreEqual("", format, "Since the value was empty, the prefix should not have been applied.");
        }

        [TestMethod]
        public void AttributeFormatter_MaxLength_Should_CapLength()
        {
            var contact = new Contact
            {
                Address1_City = "1234567890"
            };
            var format = Formatter.Format(contact, new AttributeFormatConfig
            {
                Attributes = new List<AttributeFormat>
                {
                    new AttributeFormat { Attribute = Contact.Fields.Address1_City, MaxLength = 5}
                },
                Format = "{0}"

            });

            Assert.AreEqual("12345", format, "The length of the output should have been limited to 5.");
        }
    }
}
