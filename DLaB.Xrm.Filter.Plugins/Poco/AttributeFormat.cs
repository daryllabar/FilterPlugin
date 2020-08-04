using System.Runtime.Serialization;

namespace DLaB.Xrm.Filter.Plugins.Poco
{
    [DataContract]
    public class AttributeFormat
    {
        [DataMember(IsRequired = false, Name = "prefix", EmitDefaultValue = false)]
        public string Prefix { get; set; }
        [DataMember(IsRequired = true, Name = "attribute")]
        public string Attribute { get; set; }
        [DataMember(IsRequired = false, Name = "maxLength", EmitDefaultValue = false)]
        public int MaxLength { get; set; }
    }
}
