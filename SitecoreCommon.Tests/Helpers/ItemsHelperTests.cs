using System;
using System.Collections.Generic;
using NUnit.Framework;
using SitecoreCommon.Helpers;

namespace SitecoreCommon.Tests.Helpers
{
    /// <summary>
    /// Tests for ItemsHelper
    /// </summary>
    [TestFixture]
    public class ItemsHelperTests
    {
        /// <summary>
        /// Test latin to ASCII string conversion
        /// </summary>
        [Test]
        public void TestLatinToAsciiConversion()
        {
            String[] latinStrings = {
                                       "Łabądek",
                                       "Żółć",
                                       "Gęślą",
                                       "Exupéry"
                                    };

            String[] converionExpected = {
                                            "Labadek",
                                            "Zolc",
                                            "Gesla",
                                            "Exupery"
                                         };

            for (int i = 0; i < latinStrings.Length; i++)
            {
                String converted = ItemsHelper.LatinToAscii(latinStrings[i]);

                Assert.AreEqual(converted, converionExpected[i]);
            }
        }

        /// <summary>
        /// Tests hex string to unicode char conversion
        /// </summary>
        [Test]
        public void TestHexToUnicodeCharConversion()
        {
            String hexString = "0044";
            String conversionExpected = "D";

            String converted = ItemsHelper.ToUnichar(hexString);

            Assert.AreEqual(converted, conversionExpected);
        }

        /// <summary>
        /// Tests item name conversion
        /// </summary>
        [Test]
        public void TestItemValidNameConversion()
        {
            Dictionary<String, String> conversionExpectations = new Dictionary<String, String>
            {
                { "Testowy element nr. 123",        "Testowy-element-nr-123" },
                { "ul. Łąkowa 89",                  "ul-Lakowa-89" },
                { "Świeże bułeczki",                "Swieze-buleczki" },
                { "Oferta",                         "Oferta" },
                { "nc+",                            "nc-" },
                { "Ça me plaît",                    "Ca-me-plait" }
                //{ "slashes/test\\new",              "slashes-test-new" },
            };

            foreach (KeyValuePair<String, String> entry in conversionExpectations)
            {
                var converted = ItemsHelper.GetValidItemName(entry.Key);

                Assert.AreEqual(converted, entry.Value);
            }
        }
    }
}
