﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BitHoursApp.Updater.Core;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

namespace BitHoursApp.Tests
{
    /// <summary>
    /// Summary description for ApplicationInfoLoaderTest
    /// </summary>
    [TestClass]
    public class ApplicationInfoLoaderTest
    {
        public ApplicationInfoLoaderTest()
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
        public void TestGetApplicationInfo()
        {
            var loader = new HttpApplicationInfoLoaderMock();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("ApplicationInfo.xml");

            var applicationsInfo = loader.GetApplicationInfo(xmlDocument).ToArray();

            Assert.AreEqual(2, applicationsInfo.Length);
        }

        class HttpApplicationInfoLoaderMock : HttpApplicationInfoLoader
        {
            public IEnumerable<ApplicationInfo> GetApplicationInfo(XmlDocument xmlDocument)
            {                
                return base.GetApplicationInfo(xmlDocument);
            }
        }
    }
}
