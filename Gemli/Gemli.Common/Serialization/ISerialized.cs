namespace Gemli.Serialization
{
    /// <summary>
    /// Describes a conversion utility that serializes or 
    /// deserializes object graphs.
    /// </summary>
    public interface ISerialized
    {
        /// <summary>
        /// When implemented, returns the serialized value as a string.
        /// </summary>
        string SerializedStringValue { get; set; }

        /// <summary>
        /// When implemented, returns the serialized value as a byte array.
        /// </summary>
        byte[] SerializedBinaryValue { get; set; }

        /// <summary>
        /// When implemented, deserializes the SerializedValue.
        /// </summary>
        /// <returns></returns>
        object Deserialize(System.Type type);
    }

    /// <summary>
    /// Describes a typed conversion utility that serializes or 
    /// deserializes object graphs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISerialized<T> : ISerialized
    {
        /// <summary>
        /// When implemented, deserializes the SerializedValue
        /// and returns it as a <typeparamref name="T">T</typeparamref>.
        /// </summary>
        /// <returns></returns>
        T Deserialize();
    }
}