using System;
using System.Linq;
using System.Xml;
using Microsoft.Xrm.Sdk.Query;
using LookupFilterResult = DLaB.Xrm.Filter.Plugins.Common.QueryRequestParserResult.LookupFilterResult;

namespace DLaB.Xrm.Filter.Plugins.Common
{
    public class QueryRequestParser
    {
        public struct FetchXmlDef
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

        public QueryRequestParserResult Parse(QueryBase qb, string primaryNameAttributeName)
        {
            
            var doc = new XmlDocument();
            var result =
                GetFetchQuery(qb, out var query)
                ?? ParseXml(query, doc)
                ?? ValidateIsLookupFilter(doc, primaryNameAttributeName);

            result.Fetch = query;
            return result;
        }

        private QueryRequestParserResult ValidateIsLookupFilter(XmlDocument doc, string primaryNameAttributeName)
        {
            return InvalidFetchXml(doc, out var entity)
                   ?? MissingAttributes(doc, entity, out var attributes)
                   ?? MissingPrimaryNameAttribute(doc, attributes, primaryNameAttributeName)
                   ?? MissingOrderBy(doc, entity)
                   ?? MissingFilterConditions(doc, out var filterConditions)
                   ?? MissingLikeCondition(doc, filterConditions)
                   ?? new QueryRequestParserResult(LookupFilterResult.Success, doc);
        }


        private static QueryRequestParserResult InvalidFetchXml(XmlDocument doc, out XmlNode entity)
        {
            entity = doc.FirstChild?.FirstChild;
            return entity == null || entity.Name != FetchXmlDef.Entity
                ? new QueryRequestParserResult(LookupFilterResult.MissingFetchXmlStructure, doc)
                : null;
        }

        private static QueryRequestParserResult MissingAttributes(XmlDocument doc, XmlNode entity, out XmlNodeList attributes)
        {
            attributes = entity.SelectNodes(FetchXmlDef.Attribute);
            return attributes?.Count == 0
                ? new QueryRequestParserResult(LookupFilterResult.MissingAttributes, doc)
                : null;
        }

        private static QueryRequestParserResult MissingPrimaryNameAttribute(XmlDocument doc, XmlNodeList attributes, string primaryNameAttribute)
        {
            return attributes.Cast<XmlNode>()
                             .SelectMany(a => a.Attributes?.Cast<XmlAttribute>())
                             .Any(a => a.Name == "name" 
                                       && a.Value == primaryNameAttribute)
                ? null
                : new QueryRequestParserResult(LookupFilterResult.MissingPrimaryNameAttribute, doc);
        }

        private static QueryRequestParserResult MissingOrderBy(XmlDocument doc, XmlNode entity)
        {
            return entity.SelectNodes(FetchXmlDef.Order)?.Count != 1
                ? new QueryRequestParserResult(LookupFilterResult.MissingOrderBy, doc)
                : null;
        }

        private static QueryRequestParserResult MissingFilterConditions(XmlDocument doc, out XmlNodeList filterConditions)
        {
            filterConditions = doc.FirstChild.FirstChild.SelectNodes(FetchXmlDef.EntityFilterConditionPath);
            return filterConditions == null || filterConditions.Count == 0
                ? new QueryRequestParserResult(LookupFilterResult.MissingLikeFilterCondition, doc)
                : null;
        }

        private QueryRequestParserResult MissingLikeCondition(XmlDocument doc, XmlNodeList filterConditions)
        {
            var likeCondition = GetLikeCondition(filterConditions);
            return likeCondition == null 
                ? new QueryRequestParserResult(LookupFilterResult.MissingLikeFilterCondition, doc)
                : null;
        }

        private XmlNode GetLikeCondition(XmlNodeList filterConditions)
        {
            return filterConditions.Cast<XmlNode>()
                                   .SelectMany(c => c.Attributes?.Cast<XmlAttribute>())
                                   .FirstOrDefault(a => a.Name == FetchXmlDef.ConditionOperatorAttribute
                                                        && a.Value == FetchXmlDef.LikeOperator);
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

        private static QueryRequestParserResult ParseXml(FetchExpression query, XmlDocument doc)
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
