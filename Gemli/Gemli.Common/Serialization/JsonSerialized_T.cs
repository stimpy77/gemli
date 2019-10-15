using System;
using System.Web.Script.Serialization;

namespace Gemli.Serialization
{
    /// <summary>
    /// Serializes and deserializes an object graph to/from JSON.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonSerialized<T> : ISerialized<T>
    {
        /// <summary>
        /// Constructs the serializer without initializing it
        /// with any object graph or JSON value.
        /// </summary>
        public JsonSerialized()
        {
            Serializer = new JavaScriptSerializer();
        }

        /// <summary>
        /// Constructs the serializer while initializing it
        /// with the specified JSON value to be deserialized.
        /// </summary>
        /// <param name="json"></param>
        public JsonSerialized(string json) : this()
        {
            SerializedValue = json;
        }

        /// <summary>
        /// Constructs the serializer and serializes the referenced 
        /// <typeparamref name="T">T</typeparamref> <paramref>value</paramref>. 
        /// </summary>
        /// <param name="value"></param>
        public JsonSerialized(T value) : this()
        {
            //try
            //{
                SerializedValue = Serializer.Serialize(value);
            //} catch (Exception e)
            //{
            //    if (e.Message.StartsWith("A circular reference was detected"))
            //    {
            //        var xml = new XmlSerialized<T>(value).SerializedValue;
            //        SerializedValue = JsonConverter.XmlToJson(xml);
            //    }
            //    else throw;
            //}
        }

        private JavaScriptSerializer Serializer { get; set; }

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
        /// Deserializes the JSON and returns
        /// the <typeparamref name="T">T</typeparamref> value.
        /// </summary>
        /// <returns></returns>
        public T Deserialize()
        {
            return Serializer.Deserialize<T>(SerializedValue);
        }

        /// <summary>
        /// Returns the value of serialized object.
        /// </summary>
        public string SerializedValue { get; set; }

        #endregion
    }
}