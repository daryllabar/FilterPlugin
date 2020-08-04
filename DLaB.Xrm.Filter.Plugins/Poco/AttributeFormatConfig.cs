using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DLaB.Xrm.Filter.Plugins.Poco
{
    [Serializable]
    [DataContract]
    public class AttributeFormatConfig
    {
        [DataMember(IsRequired = true, Name = "attributes")]
        public List<AttributeFormat> Attributes { get; set; }
        [DataMember(IsRequired = true, Name = "format")]
        public string Format { get; set; }
        [DataMember(IsRequired = true, Name = "nameAttribute")]
        public string NameAttribute { get; set; }

    }
}
