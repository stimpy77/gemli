using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Gemli.Reflection;
using Gemli.Serialization;

namespace Gemli.Collections
{
    /// <summary>
    /// A dictionary that can be XML-serialized.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializable, IXmlSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableDictionary{TKey,TValue}"/> class that is
        /// empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public SerializableDictionary()
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class that is
        /// empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity"></param>
        public SerializableDictionary(int capacity)
            : base(capacity)
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class that 
        /// is empty, has the specified initial capacity, and uses the specified 
        /// System.Collections.Generic.IEqualityComparer{T}.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="comparer"></param>
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class that 
        /// contains elements copied from the specified System.Collections.Generic.IDictionary{TKey,TValue} and 
        /// uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary"></param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class that 
        /// contains elements copied from the specified System.Collections.Generic.IDictionary{TKey,TValue} and 
        /// uses the specified System.Collections.Generic.IEqualityComparer{T}.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="comparer"></param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class that is 
        /// empty, has the default initial capacity, and uses the specified 
        /// System.Collections.Generic.IEqualityComparer{T}.
        /// </summary>
        /// <param name="comparer"></param>
        public SerializableDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class with serialized data.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public SerializableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Init();
        }

        private void Init()
        {
            SerializeKeyTypeAsKeyAttribute = false;
            SerializeValueTypeAsValueAttribute = typeof(TValue) == typeof(object);
            SerializedItemElementName = "item";
            SerializedKeyElementName = "key";
            SerializedValueElementName = "value";
        }

        #region IXmlSerializable Members

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty) return;
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement(SerializedItemElementName);
                var key = DeserializeKey(reader);
                reader.MoveToContent();
                var value = DeserializeValue(reader);
                try
                {
                    Add(key, value);
                }
                catch
                {
                    var tk = typeof(TKey);
                    throw;
                }
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            //var valueSerializer = new XmlSerializer(typeof(TValue));
            var cln = this.ToList();
            foreach (var kvp in cln)
            {
                var key = kvp.Key;
                writer.WriteStartElement(SerializedItemElementName);
                SerializeKey(writer, key);
                SerializeValue(writer, this[key]);
                writer.WriteEndElement(); // item
            }
        }

        /// <summary>
        /// Gets or sets whether the &lt;key&gt; element in the XML serialization
        /// of the keys will include an attribute called "type"
        /// that describes the <typeparamref name="TKey">TKey</typeparamref>
        /// type name.
        /// </summary>
        protected bool SerializeKeyTypeAsKeyAttribute { get; set; }

        /// <summary>
        /// Gets or sets whether the &lt;value&gt; element in the XML serialization
        /// of the values will include an attribute called "type"
        /// that describes the <typeparamref name="TValue">TValue</typeparamref>
        /// type name.
        /// </summary>
        protected bool SerializeValueTypeAsValueAttribute { get; set; }

        /// <summary>
        /// Gets or sets the name of the XML element for the key values.
        /// </summary>
        protected string SerializedKeyElementName { get; set; }

        /// <summary>
        /// Gets or sets the name of the XML element for the values.
        /// </summary>
        protected string SerializedValueElementName { get; set; }

        /// <summary>
        /// Gets or sets the name of the XML element for the items.
        /// </summary>
        protected string SerializedItemElementName { get; set; }

        /// <summary>
        /// When overridden, implements a custom serialization of the key value.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="key"></param>
        protected virtual void SerializeKey(XmlWriter writer, TKey key)
        {
            writer.WriteStartElement(SerializedKeyElementName);
            if (SerializeKeyTypeAsKeyAttribute)
            {
                writer.WriteStartAttribute("type");
                writer.WriteValue(typeof(TKey).FullName);
                writer.WriteEndAttribute();
            }
            var keySerializer = new XmlSerializer(typeof(TKey));
            keySerializer.Serialize(writer, key);
            writer.WriteEndElement(); // key
        }
        
        /// <summary>
        /// When overridden, implements a custom deserialization of the key value.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected virtual TKey DeserializeKey(XmlReader reader)
        {
            var keyXml = reader.ReadInnerXml();
            var serializedKey = new XmlSerialized<TKey>(keyXml);
            var key = serializedKey.Deserialize();
            return key;
        }

        /// <summary>
        /// When overridden, implements a custom serialization of the value.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        protected virtual void SerializeValue(XmlWriter writer, TValue value)
        {
            writer.WriteStartElement(SerializedValueElementName);
            ISerialized valueSerializer = new XmlSerialized<TValue>(value);
            if (SerializeValueTypeAsValueAttribute)
            {
                writer.WriteStartAttribute("type");
                writer.WriteValue(value.GetType().FullName);
                writer.WriteEndAttribute();
                Type st = typeof (XmlSerialized<>).MakeGenericType(value.GetType());
                valueSerializer = (ISerialized) Activator.CreateInstance(st, value, true);
            }
            var svdoc = new XmlDocument();
            svdoc.LoadXml(valueSerializer.SerializedStringValue);
            string xml;
            if (svdoc.LastChild.FirstChild == null)
                xml = "<anyType nil=\"true\" />";
            else xml = svdoc.LastChild.OuterXml;
            writer.WriteRaw(xml);
            writer.WriteEndElement(); // value
        }

        /// <summary>
        /// When overridden, implements a custom deserialization of the value.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected virtual TValue DeserializeValue(XmlReader reader)
        {
            var valueDeserializer = new XmlSerialized<TValue>();
            TValue value;
            if (!reader.IsEmptyElement)
            {
                valueDeserializer.SerializedValue = reader.ReadOuterXml();
                var xdoc = new XmlDocument();
                xdoc.LoadXml(valueDeserializer.SerializedValue);
                if (xdoc.LastChild.FirstChild != null && 
                    xdoc.LastChild.FirstChild.Attributes["nil"] != null &&
                    xdoc.LastChild.FirstChild.Attributes["nil"].Value == "true")
                {
                    return default(TValue);
                }
                if (SerializeValueTypeAsValueAttribute)
                {
                    var typeName = xdoc.LastChild.Attributes["type"].Value;
                    var type = Type.GetType(typeName);
                    var x2 = new XmlSerializer(type);
                    var ms = new MemoryStream();
                    var sw = new StreamWriter(ms);
                    sw.Write(xdoc.LastChild.InnerXml);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    object objValue = x2.Deserialize(ms);
                    return (TValue) objValue;
                }
                valueDeserializer.SerializedValue = xdoc.LastChild.InnerXml;
                value = valueDeserializer.Deserialize();
                return value;
            }
            value = default(TValue);
            return value;
        }

        #endregion

        #region ISerializable Members

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
