using Gemli.Web.Content;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Gemli.WebCore.Content
{
    /// <summary>
    ///This is a test class for MimeTypeMapTest and is intended
    ///to contain all MimeTypeMapTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MimeTypeMapTest
    {
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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetFileTypeFromContentType
        ///</summary>
        [TestMethod()]
        public void GetFileTypeFromContentTypeTest()
        {
            string mimeType = "application/msword";
            string expected = ".doc";
            string actual;
            actual = MimeTypeMap.GetFileTypeFromContentType(mimeType);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetContentTypeFromFileType
        ///</summary>
        [TestMethod()]
        public void GetContentTypeFromFileTypeTest()
        {
            string filename = "My Girl.doc";
            string expected = "application/msword";
            string actual;
            actual = MimeTypeMap.GetContentTypeFromFileType(filename);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetContentTypeDescriptionFromFileType
        ///</summary>
        [TestMethod()]
        public void GetContentTypeDescriptionFromFileTypeTest()
        {
            string fileType = "My Girl.doc";
            string expected = "Microsoft Word binary document";
            string actual;
            actual = MimeTypeMap.GetContentTypeDescriptionFromFileType(fileType);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetContentTypeDescriptionFromContentType
        ///</summary>
        [TestMethod()]
        public void GetContentTypeDescriptionFromContentTypeTest()
        {
            string mimeType = "application/msword";
            string expected = "Microsoft Word binary document";
            string actual;
            actual = MimeTypeMap.GetContentTypeDescriptionFromContentType(mimeType);
            Assert.AreEqual(expected, actual);
        }
    }
}