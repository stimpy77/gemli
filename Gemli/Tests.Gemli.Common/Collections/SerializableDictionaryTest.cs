using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Gemli.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gemli.Collections;

namespace Tests.Gemli.Common.Collections
{
    /// <summary>
    /// Summary description for SerializedDictionaryTest
    /// </summary>
    [TestClass]
    public class SerializableDictionaryTest
    {
        public SerializableDictionaryTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SerializeAndDeserializeIntStringDictionaryTest()
        {
            var dic = new SerializableDictionary<string, int>();
            dic.Add("one", 1);
            dic.Add("two", 2);
            var serializer = new XmlSerialized<SerializableDictionary<string, int>>(dic);
            var xml = serializer.SerializedValue;
            dic = serializer.Deserialize();
            Assert.IsTrue(dic.Count == 2);
            Assert.IsTrue(dic["one"] == 1);
            Assert.IsTrue(dic["two"] == 2);
        }

        public class CustomSerializationElementsDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue>
        {
            public CustomSerializationElementsDictionary()
            {
                base.SerializedItemElementName = "anudderOne";
                base.SerializedKeyElementName = "whatItsCalled";
                base.SerializedValueElementName = "whatItIs";
            }
        }

        [TestMethod]
        public void SerializeAndDeserializeIntStringDictionaryWithCustomElementNamesTest()
        {
            var dic = new CustomSerializationElementsDictionary<string, int>();
            dic.Add("one", 1);
            dic.Add("two", 2);
            var serializer = new XmlSerialized<CustomSerializationElementsDictionary<string, int>>(dic);
            var xml = serializer.SerializedValue;
            Assert.IsTrue(xml.Contains("<anudderOne"));
            Assert.IsTrue(xml.Contains("<whatItsCalled"));
            Assert.IsTrue(xml.Contains("<whatItIs"));
            dic = serializer.Deserialize();
            Assert.IsTrue(dic.Count == 2);
            Assert.IsTrue(dic["one"] == 1);
            Assert.IsTrue(dic["two"] == 2);
        }
    }
}
