using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Filter.Plugins.Poco;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Filter.Plugins.Common
{
    public class AttributeFormatter
    {
        public string Format(Entity entity, AttributeFormatConfig config)
        {
            var values = new List<string>(config.Attributes.Count);
            foreach (var att in config.Attributes)
            {
                var value = GetAttributeValue(entity, att);
                value = ConstrainLength(att, value);
                value = AddPrefix(att, value);
                values.Add(value);
            }

            return string.Format(config.Format, values.Cast<object>().ToArray());
        }

        private static string GetAttributeValue(Entity entity, AttributeFormat att)
        {
            if (entity.FormattedValues.TryGetValue(att.Attribute, out var value))
            {
                return value;
            }

            return entity.TryGetAttributeValue<object>(att.Attribute, out var result) 
                ? GetStringForValueBasedOnType(result) 
                : null;
        }

        private static string GetStringForValueBasedOnType(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case Money money:
                    return money.Value.ToString();
                default:
                    return value.ToString();
            }
        }

        private static string ConstrainLength(AttributeFormat att, string value)
        {
            if (att.MaxLength > 0
                && value != null
                && value.Length > att.MaxLength)
            {
                value = value.Substring(0, att.MaxLength);
            }

            return value;
        }

        private static string AddPrefix(AttributeFormat att, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(att.Prefix))
            {
                return value;
            }

            return att.Prefix + value;
        }
    }
}
