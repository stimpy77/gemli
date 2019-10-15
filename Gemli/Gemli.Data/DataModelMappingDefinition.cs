using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
    [XmlRoot("modelMappings")]
    [Serializable]
    public class DataModelMappingDefinition : SerializableDictionary<Type, DataModelMap>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableDictionary{TKey,TValue}"/> class that is
        /// empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public DataModelMappingDefinition()
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class that 
        /// contains elements copied from the specified System.Collections.Generic.IDictionary{TKey,TValue} and 
        /// uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary"></param>
        public DataModelMappingDefinition(IDictionary<Type, DataModelMap> dictionary)
            : base(dictionary)
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary{TKey,TValue} class with serialized data.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public DataModelMappingDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Init();
        }

        private void Init()
        {
            SerializedItemElementName = "modelMapping";
            SerializedKeyElementName = "class";
            SerializedValueElementName = "mappingValue";
            SerializeKeyTypeAsKeyAttribute = false;
            SerializeValueTypeAsValueAttribute = false;
        }

        /// <summary>
        /// Implements a custom deserialization of the key, using the type name as a type specifier.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected override Type DeserializeKey(System.Xml.XmlReader reader)
        {
            reader.ReadToFollowing(SerializedKeyElementName);
            var keyXml = reader.ReadInnerXml();
            var key = Type.GetType(keyXml);
            return key;
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
    }
}
