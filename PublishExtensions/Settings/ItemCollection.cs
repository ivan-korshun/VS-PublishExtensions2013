using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PHZH.PublishExtensions.Xml;

namespace PHZH.PublishExtensions.Settings
{
    /// <summary>
    /// Represents a collection of project items.
    /// </summary>
    [Serializable]
    [XmlRoot("Items")]
    public class ItemCollection : SortedList<string, ItemSettings>, IXmlSerializable
    {
        private static Serializer<ItemSettings> serializer = new Serializer<ItemSettings>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCollection"/> class.
        /// </summary>
        public ItemCollection()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {
        }

        public bool Add(ItemSettings item)
        {
            if (!Contains(item))
            {
                Add(item.Path, item);
                return true;
            }

            return false;
        }

        public bool Contains(ItemSettings item)
        {
            return ContainsKey(item.Path);
        }

        public bool Remove(ItemSettings item)
        {
            return Remove(item.Path);
        }

        public void Rename(ItemSettings item, string newPath)
        {
            Remove(item);
            Add(new ItemSettings(newPath, item));
        }

        public ItemSettings TryGet(string key)
        {
            return ContainsKey(key) ? this[key] : null;
        }

        public List<ItemSettings> GetItemsStartingWith(string path)
        {
            return this.Values.Where(item => item.Path.StartsWith(path, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
        /// </returns>
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is deserialized.</param>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            // clear collection
            Clear();

            bool isEmpty = reader.IsEmptyElement;
            string elementName = reader.Name;

            // advance to next element
            reader.Read();

            // was empty? nothing to load
            if (isEmpty)
                return;

            // read items
            while (reader.Read())
            {
                // end tag of our collection?
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
                    break;

                // is item element
                if (reader.NodeType == XmlNodeType.Element)
                {
                    ItemSettings item = serializer.Deserialize(reader);
                    Add(item);
                }
            }

            // read end element
            reader.ReadEndElement();
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is serialized.</param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<string, ItemSettings> entry in this)
            {
                if (entry.Value != null)
                    serializer.Serialize(writer, entry.Value);
            }
        }
    }
}
