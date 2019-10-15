using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Xml.Serialization;

namespace Gemli.Collections
{
    /// <summary>
    /// A serializable dictionary with case-insensitive string 
    /// keys.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class CaseInsensitiveDictionary<TValue> : SerializableDictionary<string, TValue>
    {
        /// <summary>
        /// Empty constructor to create a CaseInsensitiveDictionary&lt;TValue&gt;.
        /// </summary>
        public CaseInsensitiveDictionary() {}

        /// <summary>
        /// Used for deserializing the dictionary.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected CaseInsensitiveDictionary(SerializationInfo info, StreamingContext context)
        {
            foreach (var item in info)
            {
                if (item.Name.StartsWith("KeyValuePairs"))
                {
                    var val = (KeyValuePair<string, TValue>[]) item.Value;
                    foreach (var kvp in val)
                    {
                        Add(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Basic constructor to create a CaseInsensitiveDictionary&lt;TValue&gt;
        /// while pre-populating it with the provided dictionary data.
        /// </summary>
        /// <param name="startData"></param>
        public CaseInsensitiveDictionary(IDictionary<string, TValue> startData)
            : base(startData)
        {
        }

        /// <summary>
        /// Determins whether the dictionary contains the specified
        /// key (case insensitive).
        /// </summary>
        /// <param name="key">They key to search for (case insensitive).</param>
        /// <returns></returns>
        public new bool ContainsKey(string key)
        {
            if (base.ContainsKey(key)) return true;
            string lckey = key.ToLower();
            foreach (var kvp in this)
            {
                if (kvp.Key.ToLower() == lckey) 
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets or sets the value with the specified key (case insensitive).
        /// </summary>
        /// <param name="key">The key (case insensitive) that identifies the value.</param>
        /// <returns></returns>
        public new TValue this[string key]
        {
            get
            {
                if (base.ContainsKey(key)) return base[key];
                string lckey = key.ToLower();
                foreach (var kvp in this)
                {
                    if (kvp.Key.ToLower() == lckey) 
                        return kvp.Value;
                }
                throw new ArgumentException("Key not found.");
            }
            set
            {
                if (base.ContainsKey(key))
                {
                    base[key] = value;
                    return;
                }
                string lckey = key.ToLower();
                foreach (var kvp in this)
                {
                    if (kvp.Key.ToLower() == lckey)
                    {
                        base[kvp.Key] = value;
                        return;
                    }
                }
                base[key] = value;
            }
        }

        /// <summary>
        /// Removes the value with the specified key (case insensitive)
        /// from the dictionary.
        /// </summary>
        /// <param name="key"></param>
        public new void Remove(string key)
        {
            if (base.ContainsKey(key))
            {
                base.Remove(key);
                return;
            }
            string lckey = key.ToLower();
            foreach (var kvp in this)
            {
                if (kvp.Key.ToLower() == lckey)
                {
                    base.Remove(kvp.Key);
                    return;
                }
            }
            throw new ArgumentException("Key not found.");
        }

        
    }
}