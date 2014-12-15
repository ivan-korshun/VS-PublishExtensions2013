using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace PHZH.PublishExtensions.Xml
{
    /// <summary>
    /// <see cref="XmlSerializationException"/> represents an exception if the serialization or
    /// deserialization of an object failed.
    /// </summary>
    [Serializable]
    public class XmlSerializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSerializationException" /> class.
        /// </summary>
        public XmlSerializationException()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSerializationException"/> class 
        /// with a specified error message.  
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public XmlSerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSerializationException"/> class with a 
        /// specified error message and a reference to the inner exception 
        /// that is the cause of this exception
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. 
        /// If the innerException parameter is not a null reference, the current exception 
        /// is raised in a catch block that handles the inner exception.</param>
        public XmlSerializationException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSerializationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <remarks>
        /// This constructor is called during deserialization to reconstitute the exception object transmitted over a stream. For more information, see XML and SOAP Serialization.
        /// </remarks>
        protected XmlSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
