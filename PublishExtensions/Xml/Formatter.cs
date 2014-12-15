using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PHZH.PublishExtensions.Xml
{
    /// <summary>
    /// Provides functions to format XML documents or strings.
    /// </summary>
    public class Formatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Formatter"/> class.
        /// </summary>
        public Formatter()
        {
            // create settings
            settings = new XmlWriterSettings();

            // set default values
            settings.Indent = true;
            settings.IndentChars = "    "; // 4 spaces
            settings.OmitXmlDeclaration = true;
        }

        /// <summary>
        /// Formats the specified XML string.
        /// </summary>
        /// <param name="xmlString">The XML string to format.</param>
        /// <returns>The formatted XML string.</returns>
        /// <exception cref="ArgumentNullException">
        /// <c>xmlString</c> is a null reference.
        /// </exception>
        public string Format(string xmlString)
        {
            // null?
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            // create an xml document out of the string
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);

            // call the other function
            return Format(doc);
        }

        /// <summary>
        /// Formats the specified XML document.
        /// </summary>
        /// <param name="xmlDocument">The XML document to format.</param>
        /// <returns>The formatted XML document as string.</returns>
        /// <exception cref="ArgumentNullException">
        /// <c>xmlDocument</c> is a null reference.
        /// </exception>
        public string Format(XmlDocument xmlDocument)
        {
            // null?
            if (xmlDocument == null)
                throw new ArgumentNullException("xmlDocument");

            // create a xml writer
            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                // write the document
                xmlDocument.WriteTo(writer);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets or sets a value indicating whether to indent elements.
        /// </summary>
        /// <value>
        /// true to write individual elements on new lines and indent; otherwise false.
        /// The default is true.
        /// </value>
        public bool Indent
        {
            get { return settings.Indent; }
            set { settings.Indent = value; }
        }

        /// <summary>
        /// Gets or sets the character string to use when indenting. This setting is used when the <see cref="Indent"/> property is set to true.
        /// </summary>
        /// <value>
        /// The character string to use when indenting. This can be set to any string value. 
        /// However, to ensure valid XML, you should specify only valid white space characters, 
        /// such as space characters, tabs, carriage returns, or line feeds.
        /// The default is four spaces.
        /// </value>
        public string IndentChars
        {
            get { return settings.IndentChars; }
            set { settings.IndentChars = value; }
        }

        /// <summary>
        /// Gets or sets the character string to use for line breaks.
        /// </summary>
        /// <value>
        /// The character string to use for line breaks. This can be set to any string value. 
        /// However, to ensure valid XML, you should specify only valid white space characters, 
        /// such as space characters, tabs, carriage returns, or line feeds. 
        /// The default is <code>\r\n</code> (carriage return, new line).
        /// </value>
        public string NewLineChars
        {
            get { return settings.NewLineChars; }
            set { settings.NewLineChars = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to normalize line breaks in the output.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Xml.NewLineHandling"/> values.
        /// The default is <see cref="System.Xml.NewLineHandling.Replace"/>.
        /// </value>
        /// <remarks>See the remarks of the <see cref="XmlWriterSettings.NewLineHandling"/> property for more information.</remarks>
        public NewLineHandling NewLineHandling
        {
            get { return settings.NewLineHandling; }
            set { settings.NewLineHandling = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to write attributes on a new line.
        /// </summary>
        /// <value>
        /// true to write attributes on individual lines; otherwise false. The default is false.
        /// This setting has no effect when the <see cref="Indent"/> property value is false.
        /// When <see cref="NewLineOnAttributes"/> is set to true, each attribute is pre-pended 
        /// with a new line and one extra level of indentation.
        /// </value>
        public bool NewLineOnAttributes
        {
            get { return settings.NewLineOnAttributes; }
            set { settings.NewLineOnAttributes = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to write an XML declaration.
        /// </summary>
        /// <value>
        /// true to omit the XML declaration; otherwise false. 
        /// The default is true, the XML declaration is omitted.
        /// </value>
        public bool OmitXmlDeclaration
        {
            get { return settings.OmitXmlDeclaration; }
            set { settings.OmitXmlDeclaration = value; }
        }

        // private variables
        private XmlWriterSettings settings = null;
    }
}
