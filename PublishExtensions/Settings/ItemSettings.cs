using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PHZH.PublishExtensions.Settings
{
    [Serializable]
    [XmlRoot("Item")]
    public class ItemSettings
    {
        public const bool IgnoreDefault = false;
        public const string MappingDefault = null;
        
        public ItemSettings()
        {
            Path = null;
            Mapping = null;
            Ignore = false;
        }

        public ItemSettings(string path)
            : this()
        {
            Path = path;
        }

        public ItemSettings(string path, ItemSettings template)
        {
            template.ThrowIfNull("template");
            
            Path = path;
            Mapping = template.Mapping;
            Ignore = template.Ignore;
        }

        public override string ToString()
        {
            return "Item: {0} (Mapping={1}, Ignore={2})".TryFormat(Path, Mapping, Ignore);
        }

        public string GetName()
        {
            string path = Path.TrimEnd('\\');
            if (path.Contains('\\'))
                return path.SubstringAfterLast("\\");
            else
                return path;
        }

        public string ApplyMapping()
        {
            return ApplyMapping(Mapping);
        }

        public string ApplyMapping(string mapping)
        {
            if (mapping.IsNullOrWhiteSpace())
                return this.Path;

            // replace name with mapping
            string name = GetName();
            int index = Path.LastIndexOf(name);
            return Path.Remove(index, name.Length).Insert(index, mapping);
        }

        [XmlAttribute("Path")]
        public string Path { get; set; }

        [XmlAttribute("Mapping")]
        public string Mapping { get; set; }

        [XmlAttribute("Ignore")]
        public bool Ignore { get; set; }
    }
}
