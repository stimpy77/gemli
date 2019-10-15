using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Gemli.Serialization
{
    ///<summary>
    /// Serializes and deserializes a serializable object to and from XML.
    ///</summary>
    ///<typeparam name="T">The type of object to be serialized and/or deserialized</typeparam>
    [Serializable]
    [DataContract]
    public class XmlSerialized<T> : ISerialized<T>
    {
        private static XmlSerializer _Serializer;
        [XmlIgnore] private string _serializedValue;

        /// <summary>
        /// Constructs the serializer without any initialization.
        /// </summary>
        public XmlSerialized()
        {
        }

        /// <summary>
        /// Constructs the serializer with a specified serialized object.
        /// </summary>
        /// <param name="serializedValue"></param>
        public XmlSerialized(string serializedValue)
        {
            SerializedValue = serializedValue;
        }

        /// <summary>
        /// Constructs the serializer and serializes the referenced object.
        /// </summary>
        /// <param name="value"></param>
        public XmlSerialized(T value) : this(value, true) {}

        /// <summary>
        /// Constructs the serializer and, if so specified, serializes the referenced object.
        /// This is only used internally or by reflection-based invocations.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="autoSerialize"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public XmlSerialized(T value, bool autoSerialize)
        {
            if (!autoSerialize) return;
            if ((object)value == DBNull.Value)
            {
                if (typeof(T).IsValueType) throw new NullReferenceException("DBNull was referenced while attempting to serialize " + typeof(T).Name);
                value = default(T);
            }
            Stream stream = new MemoryStream();
            try
            {
                Serializer.Serialize(stream, value);
            } catch
            {
                throw; // try...catch is for breakpoint
            }
            stream.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(stream);
            SerializedValue = sr.ReadToEnd();
            sr.Close();
            stream.Close();
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (_Serializer == null) _Serializer = new XmlSerializer(typeof (T));
                return _Serializer;
            }
        }

        /// <summary>
        /// The serialized version of the object.
        /// </summary>
        [XmlElement]
        [DataMember]
        public string SerializedValue
        {
            get { return _serializedValue; }
            set { _serializedValue = value; }
        }

        #region ISerialized<T> Members

        string ISerialized.SerializedStringValue
        {
            get { return SerializedValue; }
            set { SerializedValue = value; }
        }

        byte[] ISerialized.SerializedBinaryValue
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        object ISerialized.Deserialize(Type type)
        {
            if (type != typeof(T)) 
                throw new ArgumentException(
                    "Specified type is not type " 
                    + typeof(T).FullName, "type");
            return Deserialize();
        }

        /// <summary>
        /// Deserializes the underlying serialized object 
        /// as a <typeparamref name="T">T</typeparamref>.
        /// </summary>
        /// <returns></returns>
        public T Deserialize()
        {
            if (string.IsNullOrEmpty(SerializedValue))
                throw new InvalidOperationException("SerializedValue property is null or empty string.");
            Stream s = new MemoryStream();
            var sw = new StreamWriter(s);
            sw.Write(SerializedValue);
            sw.Flush();
            s.Seek(0, SeekOrigin.Begin);
            T value;
            try
            {
                value = (T) Serializer.Deserialize(s);
            } catch
            {
                throw; // try...catch is for breakpoint
            }
            return value;
        }

        #endregion

        /// <summary>
        /// Loads the <see cref="SerializedValue"/>
        /// into a <see cref="XmlDocument"/> object
        /// and returns the object.
        /// </summary>
        /// <returns></returns>
        public XmlDocument ToXmlDocument()
        {
            if (string.IsNullOrEmpty(SerializedValue))
                throw new InvalidOperationException("SerializedValue property is null or empty string.");
            var xdoc = new XmlDocument();
            xdoc.LoadXml(this.SerializedValue);
            return xdoc;
        }

        /// <summary>
        /// Converts the serialized value 
        /// of type <typeparamref name="T">T</typeparamref>
        /// to the specified compatible type.
        /// </summary>
        /// <typeparam name="To_T"></typeparam>
        /// <returns></returns>
        public virtual To_T ConvertTo<To_T>()
        {
            if (string.IsNullOrEmpty(SerializedValue))
                throw new InvalidOperationException("SerializedValue property is null or empty string.");
            var toObj = new XmlSerialized<To_T>(SerializedValue);
            return toObj.Deserialize();
        }

        /// <summary>
        /// Converts the specified deserialized object
        /// to type <typeparamref name="T">T</typeparamref>.
        /// </summary>
        /// <typeparam name="FromT"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual T ConvertFrom<FromT>(FromT obj) where FromT : class
        {
            if (!obj.GetType().IsValueType && obj == null)
                throw new ArgumentException("Parameter 'obj' is null.", "obj");
            var fromObj = new XmlSerialized<FromT>(obj);
            SerializedValue = fromObj.SerializedValue;
            return Deserialize();
        }
    }
}