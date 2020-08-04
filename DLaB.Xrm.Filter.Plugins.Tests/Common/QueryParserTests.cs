using DLaB.Xrm.FilterPlugin.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using LookupFilterResult = DLaB.Xrm.Filter.Plugins.Common.QueryRequestParserResult.LookupFilterResult;

namespace DLaB.Xrm.Filter.Plugins.Common.Tests
{
    [TestClass]
    public class QueryParserTests
    {
        public QueryRequestParser Parser { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Parser = new QueryRequestParser();
        }

        [TestMethod]
        public void QueryParser_QueryExpression_Should_FlagNotFetchExpression()
        {
            Assert.AreEqual(LookupFilterResult.NotFetchExpression, Parse(new QueryExpression()).Result, "Lookup Filters are always FetchExpressions");
        }

        [TestMethod]
        public void QueryParser_InvalidXml_Should_FlagException()
        {
            Assert.AreEqual(LookupFilterResult.Exception, Parse(new FetchExpression("ABC")).Result, "Invalid Xml should return exception.");
        }

        [TestMethod]
        public void QueryParser_NoFetchXml_Should_FlagMissingFetchXmlStructure()
        {
            Assert.AreEqual(LookupFilterResult.MissingFetchXmlStructure, Parse(new FetchExpression("<a/>")).Result, "FetchXml should always have a fetch and entity node.");
            Assert.AreEqual(LookupFilterResult.MissingFetchXmlStructure, Parse(new FetchExpression("<fetch/>")).Result, "FetchXml should always have a fetch and entity node.");
        }

        [TestMethod]
        public void QueryParser_NoAttributesFilterExpression_Should_FlagMissingAttributes()
        {
            Assert.AreEqual(LookupFilterResult.MissingAttributes, Parse(GenerateQuery(skipAttributes: true)).Result, "Lookup Filters always has attributes defined.");
        }

        [TestMethod]
        public void QueryParser_NoPrimaryNameAttributeFilterExpression_Should_FlagMissingPrimaryNameAttribute()
        {
            Assert.AreEqual(LookupFilterResult.MissingPrimaryNameAttribute, Parse(GenerateQuery(skipPrimaryNameAttribute: true)).Result, "Lookup Filters always has the primary name attribute defined.");
        }

        [TestMethod]
        public void QueryParser_NoOrderByFilterExpression_Should_FlagMissingOrderBy()
        {
            Assert.AreEqual(LookupFilterResult.MissingOrderBy, Parse(GenerateQuery(skipOrderBy: true)).Result, "Lookup Filters always have order by.");
        }

        [TestMethod]
        public void QueryParser_NoLikeFilterConditionFilterExpression_Should_FlagMissingLikeFilterCondition()
        {
            Assert.AreEqual(LookupFilterResult.MissingLikeFilterCondition, Parse(GenerateQuery(skipFilter: true)).Result, "Lookup Filters always have filter.");
            Assert.AreEqual(LookupFilterResult.MissingLikeFilterCondition, Parse(GenerateQuery(skipLikeFilter: true)).Result, "Lookup Filters always have a like filter.");
        }

        [TestMethod]
        public void QueryParser_WithLookupQuery_Should_ParseAttributes()
        {
            Assert.AreEqual($"{Contact.Fields.FullName},{Contact.Fields.Id}", string.Join(",", Parse().Attributes), "The parser should have parsed the attributes in the fetch xml.");
        }

        [TestMethod]
        public void QueryParser_WithLookupQuery_Should_ExposeEntity()
        {
            Assert.IsNotNull(Parse().Entity, "The parser should expose the Entity Element.");
        }

        [TestMethod]
        public void QueryParser_WithLookupQuery_Should_FlagSuccess()
        {
            Assert.AreEqual(LookupFilterResult.Success, Parse().Result, "Lookup Filters should be successful.");
        }

        public QueryRequestParserResult Parse(QueryBase query = null)
        {
            return Parser.Parse(query ?? GenerateQuery(), "fullname");
        }

        private FetchExpression GenerateQuery(
            bool skipAttributes = false, 
            bool skipPrimaryNameAttribute = false, 
            bool skipOrderBy = false, 
            bool skipFilter = false, 
            bool skipLikeFilter = false)
        {
            const string header  = @"<fetch version=""1.0"" mapping=""logical"" no-lock=""false""><entity name=""contact"">";
            var attributes       = (skipPrimaryNameAttribute ? string.Empty : @"<attribute name=""fullname""/>") + @"<attribute name=""contactid""/>";
            const string orderBy = @"<order attribute=""fullname"" descending=""false"" />";
            var filter           = $@"<filter type=""or""><condition attribute=""fullname"" operator=""{(skipLikeFilter ? "equal" : "like")}"" value="" %% "" /></filter>";
            const string footer  = "</entity></fetch>";

            return new FetchExpression(header
                   + (skipAttributes ? string.Empty : attributes)
                   + (skipOrderBy ? string.Empty : orderBy)
                   + (skipFilter  ? string.Empty : filter )
                   + footer);
        }
    }
}
