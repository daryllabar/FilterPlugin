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

        public IEnumerable<string> Attributes => XmlDoc.AttributeNames;

        public XmlNode Entity => XmlDoc?.EntityNode;

        public Exception Exception { get; set; }

        public string FetchXml => XmlDoc?.Doc?.OuterXml;

        public FetchExpression Fetch { get; set; }

        public LookupFilterResult Result { get; set; }

        public FetchXmlDocument XmlDoc { get; set; }

        public QueryRequestParserResult(LookupFilterResult result, FetchXmlDocument doc)
        {
            Result = result;
            XmlDoc = doc;
        }

        public QueryRequestParserResult(Exception exception, FetchXmlDocument doc = null) : this(LookupFilterResult.Exception, doc)
        {
            Exception = exception;
        }
    }

}
