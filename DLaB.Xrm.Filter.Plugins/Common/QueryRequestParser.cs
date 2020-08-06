using System;
using System.Linq;
using System.Xml;
using Microsoft.Xrm.Sdk.Query;
using LookupFilterResult = DLaB.Xrm.Filter.Plugins.Common.QueryRequestParserResult.LookupFilterResult;
using FetchXmlDef = DLaB.Xrm.Filter.Plugins.Common.FetchXmlDocument.Definition;

namespace DLaB.Xrm.Filter.Plugins.Common
{
    public class QueryRequestParser
    {
        public QueryRequestParserResult Parse(QueryBase qb, string primaryNameAttributeName)
        {
            
            var doc = new FetchXmlDocument();
            var result =
                GetFetchQuery(qb, out var query)
                ?? ParseXml(query, doc)
                ?? ValidateIsLookupFilter(doc, primaryNameAttributeName);

            result.Fetch = query;
            return result;
        }

        private QueryRequestParserResult ValidateIsLookupFilter(FetchXmlDocument doc, string primaryNameAttributeName)
        {
            return InvalidFetchXml(doc)
                   ?? MissingAttributes(doc)
                   ?? MissingPrimaryNameAttribute(doc, primaryNameAttributeName)
                   ?? MissingOrderBy(doc)
                   ?? MissingFilterConditions(doc)
                   ?? MissingLikeCondition(doc)
                   ?? new QueryRequestParserResult(LookupFilterResult.Success, doc);
        }


        private static QueryRequestParserResult InvalidFetchXml(FetchXmlDocument doc)
        {
            return doc.EntityNode == null || doc.EntityNode.Name != FetchXmlDef.Entity
                ? new QueryRequestParserResult(LookupFilterResult.MissingFetchXmlStructure, doc)
                : null;
        }

        private static QueryRequestParserResult MissingAttributes(FetchXmlDocument doc)
        {
            return doc.Attributes.Count == 0
                ? new QueryRequestParserResult(LookupFilterResult.MissingAttributes, doc)
                : null;
        }

        private static QueryRequestParserResult MissingPrimaryNameAttribute(FetchXmlDocument doc, string primaryNameAttribute)
        {
            return doc.AttributeNames.Contains(primaryNameAttribute)
                ? null
                : new QueryRequestParserResult(LookupFilterResult.MissingPrimaryNameAttribute, doc);
        }

        private static QueryRequestParserResult MissingOrderBy(FetchXmlDocument doc)
        {
            return doc.OrderBys.Count == 0
                ? new QueryRequestParserResult(LookupFilterResult.MissingOrderBy, doc)
                : null;
        }

        private static QueryRequestParserResult MissingFilterConditions(FetchXmlDocument doc)
        {
            return doc.FilterConditions.Count == 0
                ? new QueryRequestParserResult(LookupFilterResult.MissingLikeFilterCondition, doc)
                : null;
        }

        private QueryRequestParserResult MissingLikeCondition(FetchXmlDocument doc)
        {
            return doc.LikeCondition == null 
                ? new QueryRequestParserResult(LookupFilterResult.MissingLikeFilterCondition, doc)
                : null;
        }

        private static QueryRequestParserResult GetFetchQuery(QueryBase qb, out FetchExpression fetchExpression)
        {
            if (qb is FetchExpression query)
            {
                fetchExpression = query;
                return null;
            }

            fetchExpression = null;
            return new QueryRequestParserResult(LookupFilterResult.NotFetchExpression, null);
        }

        private static QueryRequestParserResult ParseXml(FetchExpression query, FetchXmlDocument doc)
        {
            try
            {
                doc.LoadXml(query.Query);
            }
            catch (Exception ex)
            {
                return new QueryRequestParserResult(ex);
            }

            return null;
        }
    }
}
