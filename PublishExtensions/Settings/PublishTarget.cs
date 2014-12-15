using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PHZH.PublishExtensions.Settings
{
    [Serializable]
    [XmlRoot("PublishTarget")]
    public class PublishTarget
    {
        public PublishTarget()
        {
            IsUserSpecific = false;
            Location = null;
            UserLocations = new List<UserLocation>();
        }

        [XmlAttribute("IsUserSpecific")]
        public bool IsUserSpecific { get; set; }

        [XmlAttribute("Location")]
        public string Location { get; set; }

        [XmlElement("UserLocation")]
        public List<UserLocation> UserLocations { get; set; }
    }
}
