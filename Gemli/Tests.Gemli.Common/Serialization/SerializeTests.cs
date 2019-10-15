using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gemli.Serialization;

namespace Tests.Gemli.Common.Serialization
{
    /// <summary>
    /// Summary description for XmlSerializedBinarySerializedTest
    /// </summary>
    [TestClass]
    public class SerializeTests
    {
        public SerializeTests()
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

        [Serializable]
        public class XYZ
        {
            public byte X = 2;
            public string Y = "7";
            public byte[] Z = {23, 17};
        }

        [TestMethod]
        public void ConvertToJsonTest()
        {
            var xyz = new XYZ();
            var jsonSerialized = new JsonSerialized<XYZ>(xyz);
            var deserializer = new JavaScriptSerializer();
            var xyz2 = deserializer.Deserialize<XYZ>(jsonSerialized.SerializedValue);
            Assert.AreEqual(xyz2.X, 2);
            Assert.AreEqual(xyz2.Y, "7");
            Assert.IsTrue(xyz2.Z.Length == 2);
            Assert.AreEqual(xyz2.Z[0], 23);
            Assert.AreEqual(xyz2.Z[1], 17);
        }

        [TestMethod]
        public void ConvertToBinaryCompressedToStringBackToBinaryDeserializedBackToOriginalTest()
        {
            var xyz = new XYZ();
            var binarySerialized = new BinarySerialized<XYZ>(xyz, false);
            var serializedValue = binarySerialized.SerializedValue;
            binarySerialized.Compress();
            Assert.AreNotEqual(serializedValue.Length, binarySerialized.SerializedValue.Length);
            var xmlSerialized = new XmlSerialized<BinarySerialized<XYZ>>(binarySerialized);
            binarySerialized = xmlSerialized.Deserialize();
            binarySerialized.Decompress();
            xyz = binarySerialized.Deserialize();
            Assert.AreEqual(xyz.X, 2);
            Assert.AreEqual(xyz.Y, "7");
            Assert.IsTrue(xyz.Z.Length == 2);
            Assert.AreEqual(xyz.Z[0], 23);
            Assert.AreEqual(xyz.Z[1], 17);
        }
    }
}
