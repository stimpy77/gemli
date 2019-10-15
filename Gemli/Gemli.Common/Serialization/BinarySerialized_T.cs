using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Gemli.Serialization
{
    /// <summary>
    /// Serializes/deserializes an object graph to/from a byte array.
    /// </summary>
    /// <typeparam name="T">The type of object being serialized/deserialized.</typeparam>
    [Serializable]
    [DataContract]
    public class BinarySerialized<T> : ISerialized<T>
    {
        /// <summary>
        /// Constructs the serializer using the specified object reference
        /// as the serialization subject and specifies whether to compress
        /// the serialized output.
        /// </summary>
        /// <param name="value">The object graph to serialize.</param>
        /// <param name="compressed">If true, the serialization will be compressed.</param>
        public BinarySerialized(T value, bool compressed)
        {
            serialize(value, compressed);
        }

        /// <summary>
        /// Constructs the serializer using the specified object reference
        /// as the serialization subject
        /// and automatically compresses the value or leaves the value
        /// decompressed depending on whether the compression actually
        /// shrinks the length of the byte array.
        /// </summary>
        /// <param name="value"></param>
        public BinarySerialized(T value)
            : this(value, null)
        {
        }

        /// <summary>
        /// Constructs the serializer using the specified object reference
        /// as the serialization subject.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="compress"></param>
        private BinarySerialized(T value, bool? compress)
        {
            serialize(value, compress.HasValue && compress.Value);
            if (!compress.HasValue) // auto-serialize
            {
                var len = SerializedValue.Length;
                Compress();
                if (SerializedValue.Length >= len) Decompress();
            }
        }

        /// <summary>
        /// Constructs the serializer/deserializer using the specified
        /// serialized byte array as the deserialization subject.
        /// </summary>
        /// <param name="serializedValue"></param>
        public BinarySerialized(byte[] serializedValue)
        {
            SerializedValue = serializedValue;
        }

        /// <summary>
        /// Constructs the serializer without initializing with
        /// a serialization/deserialization subject.
        /// </summary>
        public BinarySerialized()
        {
        }

        /// <summary>
        /// Gets or sets whether the byte array should
        /// be compressed when serializing or is already
        /// compressed when deserializing.
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Returns the serialized version of the object graph.
        /// </summary>
        [DataMember]
        [XmlElement]
        public byte[] SerializedValue { get; set; }

        #region ISerialized<T> Members

        object ISerialized.Deserialize(Type type)
        {
            if (type != typeof(T))
                throw new ArgumentException(
                    "Specified type is not type "
                    + typeof(T).FullName, "type");
            return Deserialize();
        }

        byte[] ISerialized.SerializedBinaryValue
        {
            get { return SerializedValue; }
            set
            {
                SerializedValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the Base64 encoding of the <see cref="SerializedValue"/>.
        /// </summary>
        string ISerialized.SerializedStringValue
        {
            get { return ToBase64(); }
            set { ConvertFromBase64(value); }
        }

        /// <summary>
        /// Returns the deserialized version of the object graph
        /// as type <typeparamref name="T">T</typeparamref>.
        /// </summary>
        /// <returns></returns>
        public T Deserialize()
        {
            byte[] bytes = IsCompressed
                               ? Decompress(SerializedValue)
                               : SerializedValue;
            var ms = new MemoryStream(bytes) {Position = 0};
            var bf = new BinaryFormatter();
            var retval = (T) bf.Deserialize(ms);
            return retval;
        }

        #endregion

        private void serialize(T value, bool compressed)
        {
            IsCompressed = compressed;
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, value);
            byte[] bytes = ms.ToArray();
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            SerializedValue = compressed ? Compress(bytes) : bytes;
// ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        /// <summary>
        /// Compresses the <see cref="SerializedValue"/>.
        /// </summary>
        public void Compress()
        {
            if (IsCompressed) throw new InvalidOperationException("Data is already compressed.");
            if (SerializedValue == null) throw new NullReferenceException("SerializedValue is null.");
            SerializedValue = Compress(SerializedValue);
            IsCompressed = true;
        }

        /// <summary>
        /// Decompresses the <see cref="SerializedValue"/>.
        /// </summary>
        public void Decompress()
        {
            if (!IsCompressed) throw new InvalidOperationException("Data is already decompressed.");
            if (SerializedValue == null) throw new NullReferenceException("SerializedValue is null.");
            SerializedValue = Decompress(SerializedValue);
            IsCompressed = false;
        }
        /// <summary>
        /// Returns a Base64-encoded string version of the <see cref="SerializedValue"/>.
        /// </summary>
        /// <returns></returns>
        public string ToBase64()
        {
            return Convert.ToBase64String(SerializedValue);
        }

        /// <summary>
        /// Returns a Base64-encoded string version of the <see cref="SerializedValue"/>.
        /// </summary>
        /// <param name="obj">The object to be converted to Base64 encoding.</param>
        /// <returns></returns>
        public static string ToBase64(T obj)
        {
            var ret = new BinarySerialized<T>(obj);
            return ret.ToBase64();
        }

        /// <summary>
        /// Converts the specified Base64-encoded string value to a byte
        /// array and returns a <see cref="BinarySerialized{T}"/>.
        /// </summary>
        /// <param name="base64Value"></param>
        /// <returns></returns>
        public static BinarySerialized<T> FromBase64(string base64Value)
        {
            var ret = new BinarySerialized<T> {IsCompressed = false};
            ret.ConvertFromBase64(base64Value);
            return ret;
        }


        private void ConvertFromBase64(string base64Value)
        {
            try
            {
                SerializedValue = Convert.FromBase64String(base64Value);
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Decode" + e.Message);
            }
        }

        /// <summary>
        /// Compresses the byte array using gzip.
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        protected virtual byte[] Compress(byte[] byteArray)
        {
            //Prepare for compress
            var ms = new MemoryStream();
            var sw = new GZipStream(ms,
                                    CompressionMode.Compress);

            //Compress
            sw.Write(byteArray, 0, byteArray.Length);
            sw.Flush();
            sw.Close();

            return ms.ToArray();
        }

        /// <summary>
        /// Decompresses the byte array assuming gzip compression.
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        protected virtual byte[] Decompress(byte[] byteArray)
        {
            //Prepare for decompress
            var ms = new MemoryStream(byteArray) {Position = 0};
            var sr = new GZipStream(ms,
                                    CompressionMode.Decompress);

            //Reset variable to collect uncompressed result
            const int buffer_length = 100;
            byteArray = new byte[buffer_length];

            //Decompress
            int offset = 0;
            while (true)
            {
                if (offset + buffer_length > byteArray.Length)
                {
                    var newArray = new byte[offset + buffer_length];
                    Array.Copy(byteArray, newArray, byteArray.Length);
                    byteArray = newArray;
                }
                int rByte = sr.Read(byteArray, offset, buffer_length);
                if (rByte == 0)
                {
                    var retval = new byte[offset];
                    Array.Copy(byteArray, retval, offset);
                    byteArray = retval;
                    break;
                }
                offset += rByte;
            }

            return byteArray;
        }
    }
}