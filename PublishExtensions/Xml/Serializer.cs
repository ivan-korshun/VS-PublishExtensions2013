using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PHZH.PublishExtensions.Xml
{
    /// <summary>
    /// Serializes and deserializes objects into and from XML documents. 
    /// The <see cref="T:PHZH.Xml.Serializer`1"/> enables you to control how objects are encoded into XML.
    /// </summary>
    /// <typeparam name="T">The type of the object that this <see cref="T:PHZH.Xml.Serializer`1"/> can serialize.</typeparam>
    public class Serializer<T>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PHZH.Xml.Serializer`1"/> class that can serialize 
        /// objects of the specified type into XML documents, and deserialize XML documents 
        /// into objects of the specified type.
        /// </summary>
        public Serializer()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PHZH.Xml.Serializer`1"/> class that can serialize 
        /// objects of the specified type into XML documents, and deserialize XML documents 
        /// into object of a specified type. If a property or field returns an array, the 
        /// extraTypes parameter specifies objects that can be inserted into the array.
        /// </summary>
        /// <param name="extraTypes">A <see cref="Type"/> array of additional object types to serialize.</param>
        public Serializer(Type[] extraTypes)
            : this(null, extraTypes, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PHZH.Xml.Serializer`1"/> class that can serialize 
        /// objects of the specified type into XML documents, and deserialize XML documents into 
        /// objects of the specified type. Each object to be serialized can itself contain instances 
        /// of classes, which this overload can override with other classes.
        /// </summary>
        /// <param name="overrides">An <see cref="XmlAttributeOverrides"/>.</param>
        public Serializer(XmlAttributeOverrides overrides)
            : this(overrides, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PHZH.Xml.Serializer`1"/> class that can serialize 
        /// objects of the specified type into XML documents, and deserialize an XML document into 
        /// object of the specified type. It also specifies the class to use as the XML root element.
        /// </summary>
        /// <param name="root">An <see cref="XmlRootAttribute"/> that represents the XML root element.</param>
        public Serializer(XmlRootAttribute root)
            : this(null, null, root)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PHZH.Xml.Serializer`1"/> class.
        /// </summary>
        /// <param name="overrides">An <see cref="XmlAttributeOverrides"/>.</param>
        /// <param name="extraTypes">A <see cref="Type"/> array of additional object types to serialize.</param>
        /// <param name="root">An <see cref="XmlRootAttribute"/> that represents the XML root element.</param>
        private Serializer(XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root)
        {
            // extra types not set?
            if (extraTypes == null)
                extraTypes = new Type[0];
            
            // create the serializer
            serializer = new XmlSerializer(typeof(T), overrides, extraTypes, root, null);

            // settings
            writerSettings = new XmlWriterSettings();
            readerSettings = new XmlReaderSettings();

            // namespace
            xmlNamespace = new XmlSerializerNamespaces();
            xmlNamespace.Add(string.Empty, string.Empty);

            // set the default values
            SetDefaultSettings();
        }

        #endregion // Constructors

        #region Static functions

        /// <summary>
        /// Serializes the specified object into its XML representation.
        /// </summary>
        /// <param name="obj">The object to serialze.</param>
        /// <param name="formatXml">true, to format the XML output; false, for no formatting.</param>
        /// <returns>The XML representation of the specified object.</returns>
        public static string ToXml(T obj, bool formatXml)
        {
            var serializer = new Serializer<T>();
            serializer.WriterSettings.Indent = formatXml;
            return serializer.SerializeToString(obj);
        }

        /// <summary>
        /// Deserializes the specified XML string into its object representation.
        /// </summary>
        /// <param name="xmlString">The XML string to deserialize.</param>
        /// <returns>The object deserialized from the specified XML string.</returns>
        public static T FromXml(string xmlString)
        {
            var serializer = new Serializer<T>();
            return serializer.DeserializeFromString(xmlString);
        }

        /// <summary>
        /// Deserializes the specified stream into its object representation.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The object deserialized from the specified stream.</returns>
        public static T FromXml(Stream stream)
        {
            var serializer = new Serializer<T>();
            return serializer.Deserialize(stream);
        }

        /// <summary>
        /// Deserializes the specified XML File into its object representation.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The object deserialized from the specified XML File.</returns>
        public static T FromXmlFile(string filePath)
        {
            var serializer = new Serializer<T>();
            return serializer.Deserialize(filePath);
        }

        #endregion // Static functions

        #region Serialize

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and returns the XML document as <see cref="String"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to serialize.</param>
        /// <returns>A <see cref="String"/> representing the serialized object.</returns>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during serialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>obj</c> is a null reference
        /// </exception>
        public string SerializeToString(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            StringBuilder sb = new StringBuilder();
            Serialize(sb, obj);
            return sb.ToString();
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the XML document to a file.
        /// </summary>
        /// <param name="outputFileName">The file to which you want to write. The outputFileName must be a file system path.</param>
        /// <param name="obj">The <see cref="Object"/> to serialize.</param>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during serialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>obj</c> is a null reference
        /// <para>- or -</para>
        /// <c>outputFileName</c> is a null reference
        /// </exception>
        public void Serialize(string outputFileName, T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (outputFileName == null)
                throw new ArgumentNullException("outputFileName");

            using (XmlWriter writer = XmlWriter.Create(outputFileName, this.WriterSettings))
            {
                Serialize(writer, obj);
            }
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the XML document 
        /// using the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="output">The StringBuilder to which to write to. Content written by the <see cref="T:PHZH.Xml.Serializer`1"/> is appended to the <see cref="StringBuilder"/>.</param>
        /// <param name="obj">The <see cref="Object"/> to serialize.</param>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during serialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>obj</c> is a null reference
        /// <para>- or -</para>
        /// <c>output</c> is a null reference
        /// </exception>
        public void Serialize(StringBuilder output, T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (output == null)
                throw new ArgumentNullException("output");

            using (XmlWriter writer = XmlWriter.Create(output, this.WriterSettings))
            {
                Serialize(writer, obj);
            }
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the XML document to a file 
        /// using the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> used to write the XML document.</param>
        /// <param name="obj">The <see cref="Object"/> to serialize.</param>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during serialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>obj</c> is a null reference
        /// <para>- or -</para>
        /// <c>stream</c> is a null reference
        /// </exception>
        public void Serialize(Stream stream, T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (stream == null)
                throw new ArgumentNullException("stream");

            XmlWriter writer = XmlWriter.Create(stream, this.WriterSettings);
            Serialize(writer, obj);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the XML document to a file 
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the XML document.</param>
        /// <param name="obj">The <see cref="Object"/> to serialize.</param>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during serialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>obj</c> is a null reference
        /// <para>- or -</para>
        /// <c>textWriter</c> is a null reference
        /// </exception>
        public void Serialize(TextWriter textWriter, T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (textWriter == null)
                throw new ArgumentNullException("textWriter");

            XmlWriter writer = XmlWriter.Create(textWriter, this.WriterSettings);
            Serialize(writer, obj);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the XML document to a file 
        /// using the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="xmlWriter">The <see cref="XmlWriter"/> used to write the XML document.</param>
        /// <param name="obj">The <see cref="Object"/> to serialize.</param>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during serialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>obj</c> is a null reference
        /// <para>- or -</para>
        /// <c>xmlWriter</c> is a null reference
        /// </exception>
        public void Serialize(XmlWriter xmlWriter, T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");

            try
            {
                serializer.Serialize(xmlWriter, obj, xmlNamespace);
            }
            catch (InvalidOperationException ex)
            {
                // get the exception
                Exception innerEx = ex.InnerException ?? ex;
                
                throw new XmlSerializationException(
                    "There was an error generating the XML document: " + innerEx.Message,
                    innerEx);
            }
        }

        #endregion // Serialize

        #region Deserialize

        /// <summary>
        /// Deserializes the XML document contained in specified XML string.
        /// </summary>
        /// <param name="xmlString">The string containing the XML data.</param>
        /// <returns>The Object being deserialized.</returns>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during deserialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>xmlString</c> is a null reference
        /// </exception>
        public T DeserializeFromString(string xmlString)
        {
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            using (StringReader reader = new StringReader(xmlString))
            {
                return Deserialize(reader);
            }
        }

        /// <summary>
        /// Deserializes the XML document contained in specified file.
        /// </summary>
        /// <param name="inputFileName">The file to which you want to read. The inputFileName must be a file system path.</param>
        /// <returns>The Object being deserialized.</returns>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during deserialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>inputFileName</c> is a null reference
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The file identified by the inputFileName does not exist.
        /// </exception>
        public T Deserialize(string inputFileName)
        {
            if (inputFileName == null)
                throw new ArgumentNullException("inputFileName");

            using (XmlReader reader = XmlReader.Create(inputFileName, this.ReaderSettings))
            {
                return Deserialize(reader);
            }
        }

        /// <summary>
        /// Deserializes the XML document contained by the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="input">The StringBuilder to use</param>
        /// <returns>The Object being deserialized.</returns>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during deserialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>input</c> is a null reference
        /// </exception>
        public T Deserialize(StringBuilder input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            return DeserializeFromString(input.ToString());
        }

        /// <summary>
        /// Deserializes the XML document contained by the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> that contains the XML document to deserialize.</param>
        /// <returns>The Object being deserialized.</returns>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during deserialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>stream</c> is a null reference
        /// </exception>
        public T Deserialize(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            XmlReader reader = XmlReader.Create(stream, this.ReaderSettings);
            return Deserialize(reader);
        }

        /// <summary>
        /// Deserializes the XML document contained by the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> that contains the XML document to deserialize.</param>
        /// <returns>The Object being deserialized.</returns>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during deserialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>textReader</c> is a null reference
        /// </exception>
        public T Deserialize(TextReader textReader)
        {
            if (textReader == null)
                throw new ArgumentNullException("textReader");

            XmlReader reader = XmlReader.Create(textReader, this.ReaderSettings);
            return Deserialize(reader);
        }

        /// <summary>
        /// Deserializes the XML document contained by the specified <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="xmlReader">The <see cref="XmlReader"/> that contains the XML document to deserialize.</param>
        /// <returns>The Object being deserialized.</returns>
        /// <exception cref="XmlSerializationException">
        /// An error occurred during deserialization. The original exception is available using 
        /// the <see cref="Exception.InnerException"/> property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>xmlReader</c> is a null reference
        /// </exception>
        public T Deserialize(XmlReader xmlReader)
        {
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");

            try
            {
                object o = serializer.Deserialize(xmlReader);
                return (T)o;
            }
            catch (InvalidOperationException ex)
            {
                // get the exception
                Exception innerEx = ex.InnerException ?? ex;

                throw new XmlSerializationException(
                    "There was an error reading the XML document: " + innerEx.Message,
                    innerEx);
            }
        }

        #endregion // Deserialize

        #region Properties

        /// <summary>
        /// Gets the internally used <see cref="XmlSerializer"/>.
        /// </summary>
        public XmlSerializer XmlSerializer
        {
            get { return serializer; }
        }

        /// <summary>
        /// Gets the settings used for writing objects.
        /// </summary>
        public XmlWriterSettings WriterSettings
        {
            get { return writerSettings; }
        }

        /// <summary>
        /// Gets the settings used for reading objects.
        /// </summary>
        public XmlReaderSettings ReaderSettings
        {
            get { return readerSettings; }
        }

        #endregion // Properties

        /// <summary>
        /// Sets the default settings for writing and reading objects.
        /// </summary>
        protected void SetDefaultSettings()
        {
            // writter
            writerSettings.Indent = true;
            writerSettings.IndentChars = "    "; // 4 spaces
            writerSettings.OmitXmlDeclaration = true;
            writerSettings.Encoding = Encoding.UTF8;

            // reader needs no special settings
        }

        // private variables
        private XmlSerializer serializer = null;
        private XmlWriterSettings writerSettings = null;
        private XmlReaderSettings readerSettings = null;
        private XmlSerializerNamespaces xmlNamespace = null;
    }
}
