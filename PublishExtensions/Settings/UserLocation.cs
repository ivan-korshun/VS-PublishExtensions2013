using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PHZH.PublishExtensions.Settings
{
    [Serializable]
    [XmlRoot("UserLocation")]
    public class UserLocation
    {
        public UserLocation()
        {
            UserName = null;
            Location = null;
        }
        
        [XmlAttribute("UserName")]
        public string UserName { get; set; }
        
        [XmlAttribute("Location")]
        public string Location { get; set; }
    }
}
