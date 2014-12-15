using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using PHZH.PublishExtensions.Xml;

namespace PHZH.PublishExtensions.Settings
{
    /// <summary>
    /// Base class for settings classes.
    /// </summary>
    /// <typeparam name="T">The type that should be serialized.</typeparam>
    [Serializable]
    public abstract class ConfigBase<T>
        where T : ConfigBase<T>, new()
    {
        private static Serializer<T> serializer = null;

        /// <summary>
        /// Initializes the <see cref="ConfigBase&lt;T&gt;"/> class.
        /// </summary>
        static ConfigBase()
        {
            serializer = new Serializer<T>();
            serializer.ReaderSettings.DtdProcessing = DtdProcessing.Ignore;
            serializer.WriterSettings.Indent = true;
            serializer.WriterSettings.OmitXmlDeclaration = false;
        }

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns>true, if the file exists; otherwise, false.</returns>
        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Loads the specified file name.
        /// </summary>
        /// <param name="fileName">The absolute path to the file.</param>
        /// <returns>An instance of the loaded configuration file.</returns>
        public static T Load(string filePath)
        {
            T config;

            // build filepath
            if (!Exists(filePath))
            {
                config = new T();
            }
            else
            {
                config = serializer.Deserialize(filePath);
                config.OnDeserialized();
            }

            config.FileName = Path.GetFileName(filePath);
            config.FilePath = filePath;
            return config;
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        public virtual void Save()
        {
            serializer.Serialize(FilePath, (T)this);
        }

        /// <summary>
        /// Determines whether the file exists.
        /// </summary>
        /// <returns>true, if the file exists; otherwise, false.</returns>
        public bool Exists()
        {
            return File.Exists(FilePath);
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        [XmlIgnore]
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        [XmlIgnore]
        public string FilePath { get; private set; }

        /// <summary>
        /// Called after the configuration was deserialized.
        /// </summary>
        protected virtual void OnDeserialized()
        {
        }
    }
}
