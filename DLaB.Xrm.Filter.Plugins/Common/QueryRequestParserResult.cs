using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Filter.Plugins.Common
{
    public class QueryRequestParserResult
    {
        public enum LookupFilterResult
        {
            /// <summary>
            /// An exception occured.
            /// </summary>
            Exception,
            /// <summary>
            /// Lookup Filters are always FilterExpressions.
            /// </summary>
            NotFetchExpression,
            /// <summary>
            /// Lookup Filters always have attributes.
            /// </summary>
            MissingAttributes,
            /// <summary>
            /// Lookup Filters always have the primary name as an attribute.
            /// </summary>
            MissingPrimaryNameAttribute,
            /// <summary>
            /// Fetch Xml always has a fetch and entity node.
            /// </summary>
            MissingFetchXmlStructure,
            /// <summary>
            /// Lookup Filters always have an OrderBy clause.
            /// </summary>
            MissingOrderBy,
            /// <summary>
            /// Lookup Filters always have a like Filter Condition.
            /// </summary>
            MissingLikeFilterCondition,
            /// <summary>
            /// The query request is a lookup request and was successfully parsed
            /// </summary>
            Success,
        }

        public IEnumerable<string> Attributes => XmlDoc.SelectNodes(QueryRequestParser.FetchXmlDef.AttributePath)?
                                                       .Cast<XmlNode>()
                                                       .SelectMany(n => n.Attributes?
                                                                         .Cast<XmlAttribute>()
                                                                         .Where(a => a.Name == "name")
                                                                         .Select(a => a.Value));

        public XmlNode Entity => XmlDoc?.SelectSingleNode(QueryRequestParser.FetchXmlDef.EntityPath);

        public Exception Exception { get; set; }

        public string FetchXml => XmlDoc?.OuterXml;

        public FetchExpression Fetch { get; set; }

        public LookupFilterResult Result { get; set; }

        public XmlDocument XmlDoc { get; set; }

        public QueryRequestParserResult(LookupFilterResult result, XmlDocument doc)
        {
            Result = result;
            XmlDoc = doc;
        }

        public QueryRequestParserResult(Exception exception, XmlDocument doc = null) : this(LookupFilterResult.Exception, doc)
        {
            Exception = exception;
        }
    }

}
