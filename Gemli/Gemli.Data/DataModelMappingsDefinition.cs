using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Gemli.Collections;
using Gemli.Reflection;
using Gemli.Serialization;
using Gemli.Xml;

namespace Gemli.Data
{
    /// <summary>
    /// An XML serializable <see cref="DataModelMap"/> definition dictionary,
    /// typed by classes of inherited <see cref="DataModel"/> and by 
    /// <see cref="DataModel{TEntity}"/>-wrapped classes.
    /// </summary>
    [XmlRoot("mappings")]
    [Serializable]
    public class DataModelMappingsDefinition : SerializableDictionary<Type, DataModelMap>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableDictionary{TKey,TValue}"/> class that is
        /// empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public DataModelMappingsDefinition()
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class that 
        /// contains elements copied from the specified System.Collections.Generic.IDictionary{TKey,TValue} and 
        /// uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary"></param>
        public DataModelMappingsDefinition(IDictionary<Type, DataModelMap> dictionary)
            : base(dictionary)
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class with serialized data.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public DataModelMappingsDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Init();
        }

        private void Init()
        {
            SerializedItemElementName = "mappingItem";
            SerializedKeyElementName = "class";
            //SerializedValueElementName = "mappingValue";
            SerializeKeyTypeAsKeyAttribute = false;
            SerializeValueTypeAsValueAttribute = false;
        }

        /// <summary>
        /// Implements a custom serialization of the key, using the type name as a specifier.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="key"></param>
        protected override void SerializeKey(System.Xml.XmlWriter writer, Type key)
        {
            writer.WriteStartElement(SerializedKeyElementName);
            if (SerializeKeyTypeAsKeyAttribute)
            {
                writer.WriteStartAttribute("type");
                writer.WriteValue("System.String");
                writer.WriteEndAttribute();
            }
            var typeFullName = key.FullName;
            if (!typeFullName.Contains(","))
                typeFullName += ", " + key.Assembly.GetName().Name;
            writer.WriteRaw(XmlUtility.EncodeText(typeFullName));
            writer.WriteEndElement();
        }

        /// <summary>
        /// Implements a custom serialization of the <see cref="DataModelMap"/> value.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        protected override void SerializeValue(System.Xml.XmlWriter writer, DataModelMap value)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml((new XmlSerialized<DataModelMap>(value)).SerializedValue);
            while (xmlDoc.LastChild.Attributes.Count > 0) xmlDoc.LastChild.Attributes.RemoveAt(0);
            writer.WriteRaw(xmlDoc.LastChild.OuterXml);
        }

        /// <summary>
        /// Implements a custom deserialization of the key, using the type name as a type specifier.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected override Type DeserializeKey(System.Xml.XmlReader reader)
        {
            var keyXml = reader.ReadInnerXml();
            var key = Type.GetType(keyXml);
            return key;
        }

        /// <summary>
        /// Implements a custom deserialization of the <see cref="DataModelMap"/> value.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected override DataModelMap DeserializeValue(System.Xml.XmlReader reader)
        {
            var xml = reader.ReadOuterXml();
            var xs = new XmlSerialized<DataModelMap>(xml);
            return xs.Deserialize();
        }
    }
}
