using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Sitecore;
using Sitecore.Data.Fields;
using Sitecore.FakeDb;
using SitecoreCommon.Helpers;

namespace SitecoreCommon.Tests.Helpers
{
    /// <summary>
    /// Tests for ItemsHelper
    /// </summary>
    [TestFixture]
    public class FieldsHelperTests
    {
        /// <summary>
        /// Field names used to test
        /// </summary>
        private static class FieldNames
        {
            public const String TextField = "Text Field";
            public const String EmptyField = "Empty Field";
            public const String CheckboxField = "Checkbox Field";
            public const String CheckboxFieldUnchecked = "Checkbox Field Unchecked";

            public const String DateFieldFrom = "Date Field From";
            public const String DateFieldTo = "Date Field To";
            public const String DateFieldEmpty = "Date Field Empty";

            public const String LinkFieldExternal = "Link Field External";
            public const String LinkFieldInternal = "Link Field Internal";

            public const String IntegerField = "Integer Field";
            public const String FieldWithDoubleValue = "Double Field";
            public const String FieldWithAnotherDoubleValue = "Another Double Field";

            public const String NotExistingField = "Field that does not exist";
        }

        /// <summary>
        /// Generates Sitecore FakeDb
        /// </summary>
        /// <returns></returns>
        private Sitecore.FakeDb.Db GenerateFakeDb()
        {
            Sitecore.Data.ID templateId = new Sitecore.Data.ID("{3AD0E3AB-0CC7-41F1-B9FD-5FD9DDEF50B4}");
            Sitecore.Data.ID referencedItemId = new Sitecore.Data.ID();

            return new Sitecore.FakeDb.Db("master")
            {
                new Sitecore.FakeDb.DbTemplate("TestTemplate", templateId)
                {
                    FieldNames.TextField,
                    FieldNames.EmptyField,
                    new Sitecore.FakeDb.DbField(FieldNames.CheckboxField)
                    {
                        Type = "Checkbox"
                    },
                    new Sitecore.FakeDb.DbField(FieldNames.CheckboxFieldUnchecked)
                    {
                        Type = "Checkbox"
                    },
                    new Sitecore.FakeDb.DbField(FieldNames.DateFieldFrom)
                    {
                        Type = "DateTime"
                    },
                    new Sitecore.FakeDb.DbField(FieldNames.DateFieldTo)
                    {
                        Type = "DateTime"
                    },
                    new Sitecore.FakeDb.DbField(FieldNames.DateFieldEmpty)
                    {
                        Type = "DateTime"
                    },
                    FieldNames.LinkFieldExternal,
                    new Sitecore.FakeDb.DbLinkField(FieldNames.LinkFieldInternal)
                    {
                        LinkType = "internal", Url = "/sitecore/content/TestItemReferenced"
                    },
                    FieldNames.IntegerField,
                    FieldNames.FieldWithDoubleValue,
                    FieldNames.FieldWithAnotherDoubleValue
                },
                new Sitecore.FakeDb.DbItem("TestItemReferenced", referencedItemId)
                {
                    TemplateID = templateId,
                    Fields =
                    {
                        { FieldNames.TextField, "Value 1" },
                        { FieldNames.CheckboxField, "1" },
                        { FieldNames.CheckboxFieldUnchecked, "" },
                        { FieldNames.DateFieldFrom, DateUtil.ToIsoDate(DateTime.Now.AddDays(-1)) },
                        { FieldNames.DateFieldTo, DateUtil.ToIsoDate(DateTime.Now.AddDays(1)) },
                        { FieldNames.DateFieldEmpty, "" },
                        { FieldNames.LinkFieldExternal, "<link linktype=\"external\" url=\"http://facebook.com\" />" }
                    }
                },
                new Sitecore.FakeDb.DbItem("TestItem")
                {
                    TemplateID = templateId,
                    Fields =
                    {
                        { FieldNames.TextField, "Value 1" },
                        { FieldNames.CheckboxField, "1" },
                        { FieldNames.CheckboxFieldUnchecked, "" },
                        { FieldNames.DateFieldFrom, DateUtil.ToIsoDate(DateTime.Now.AddDays(-1)) },
                        { FieldNames.DateFieldTo, DateUtil.ToIsoDate(DateTime.Now.AddDays(1)) },
                        { FieldNames.DateFieldEmpty, "" },
                        { FieldNames.LinkFieldExternal, "<link linktype=\"external\" url=\"http://google.com\" />" },
                        new Sitecore.FakeDb.DbLinkField(FieldNames.LinkFieldInternal)
                        {
                            LinkType = "internal", Url = "/sitecore/content/TestItemReferenced", TargetID = referencedItemId
                        },
                        { FieldNames.IntegerField, "123" },
                        { FieldNames.FieldWithDoubleValue, "3.14159265358979" },
                        { FieldNames.FieldWithAnotherDoubleValue, "100,000.001" }
                    }

                    //String.Format("<link linktype=\"internal\" id=\"{0}\" url=\"{1}\" />", referencedItemId.Guid.ToString(), "/sitecore/content/TestItemReferenced")
                }
            };
        }

        /// <summary>
        /// Tests methods that check if field exists or is empty
        /// </summary>
        [Test]
        public void TestHasFieldAndHasNotEmptyField()
        {
            using (Sitecore.FakeDb.Db masterDb = GenerateFakeDb())
            {
                var item = masterDb.GetItem("/sitecore/content/TestItem");

                Assert.IsTrue(FieldsHelper.HasField(item, FieldNames.TextField));
                Assert.IsTrue(FieldsHelper.HasField(item, FieldNames.EmptyField));
                Assert.IsFalse(FieldsHelper.HasField(item, FieldNames.NotExistingField));

                Assert.IsTrue(FieldsHelper.HasNotEmptyField(item, FieldNames.TextField));
                Assert.IsFalse(FieldsHelper.HasNotEmptyField(item, FieldNames.EmptyField));
                Assert.IsFalse(FieldsHelper.HasNotEmptyField(item, FieldNames.NotExistingField));
            }
        }

        [Test]
        public void TestGetValue()
        {
            using (Sitecore.FakeDb.Db masterDb = GenerateFakeDb())
            {
                var item = masterDb.GetItem("/sitecore/content/TestItem");

                Assert.IsTrue(FieldsHelper.GetValue(item, FieldNames.TextField) == "Value 1");
                Assert.IsTrue(FieldsHelper.GetValue(item, FieldNames.EmptyField) == "");

                Assert.IsTrue(FieldsHelper.GetValue(item, FieldNames.NotExistingField, "Default value") == "Default value");
                Assert.IsTrue(FieldsHelper.GetValue(item, FieldNames.TextField, "Default value") == "Value 1");
                Assert.IsTrue(FieldsHelper.GetValue(item, FieldNames.EmptyField, "Default value") == "");

                Assert.IsTrue(FieldsHelper.GetValueOrDefaultIfEmpty(item, FieldNames.EmptyField, "Default value") == "Default value");
                Assert.IsTrue(FieldsHelper.GetValueOrDefaultIfEmpty(item, FieldNames.TextField, "Default value") == "Value 1");
                Assert.IsTrue(FieldsHelper.GetValueOrDefaultIfEmpty(item, FieldNames.NotExistingField, "Default value") == "Default value");
            }
        }

        [Test]
        public void TestGetCheckboxValue()
        {
            using (Sitecore.FakeDb.Db masterDb = GenerateFakeDb())
            {
                var item = masterDb.GetItem("/sitecore/content/TestItem");

                Assert.IsTrue(FieldsHelper.HasCheckedCheckboxField(item, FieldNames.CheckboxField));
                Assert.IsFalse(FieldsHelper.HasCheckedCheckboxField(item, FieldNames.CheckboxFieldUnchecked));
                Assert.IsFalse(FieldsHelper.HasCheckedCheckboxField(item, FieldNames.NotExistingField));

                var checkbox = FieldsHelper.GetCheckboxField(item, FieldNames.CheckboxField);

                Assert.IsNotNull(checkbox);
                Assert.IsTrue(checkbox.Checked);

                var checkboxUnchecked = FieldsHelper.GetCheckboxField(item, FieldNames.CheckboxFieldUnchecked);

                Assert.IsNotNull(checkboxUnchecked);
                Assert.IsFalse(checkboxUnchecked.Checked);

                var checkboxNotExisting = FieldsHelper.GetCheckboxField(item, FieldNames.NotExistingField);

                Assert.IsNull(checkboxNotExisting);
            }
        }

        [Test]
        public void TestGetDateValue()
        {
            using (Sitecore.FakeDb.Db masterDb = GenerateFakeDb())
            {
                var item = masterDb.GetItem("/sitecore/content/TestItem");

                Assert.IsTrue(FieldsHelper.IsActual(item, FieldNames.DateFieldFrom, FieldNames.DateFieldTo));
                Assert.IsTrue(FieldsHelper.IsActual(item, FieldNames.DateFieldFrom, FieldNames.DateFieldEmpty));
                Assert.IsTrue(FieldsHelper.IsActual(item, FieldNames.DateFieldEmpty, FieldNames.DateFieldTo));

                Assert.IsFalse(FieldsHelper.IsActual(item, FieldNames.DateFieldTo, FieldNames.DateFieldEmpty));
                Assert.IsFalse(FieldsHelper.IsActual(item, FieldNames.DateFieldEmpty, FieldNames.DateFieldFrom));

                var dateFieldFrom = FieldsHelper.GetDateField(item, FieldNames.DateFieldFrom);
                var dateFieldTo = FieldsHelper.GetDateField(item, FieldNames.DateFieldTo);

                Assert.IsTrue(dateFieldFrom.DateTime < dateFieldTo.DateTime);

                var dateFrom = FieldsHelper.GetDateFieldValue(item, FieldNames.DateFieldFrom);
                var dateTo = FieldsHelper.GetDateFieldValue(item, FieldNames.DateFieldTo);

                Assert.IsTrue(dateFrom < dateTo);
            }
        }

        [Test]
        public void TestGetUrl()
        {
            using (Sitecore.FakeDb.Db masterDb = GenerateFakeDb())
            {
                var item = masterDb.GetItem("/sitecore/content/TestItem");

                Assert.AreEqual(FieldsHelper.GetUrl(item, FieldNames.LinkFieldExternal), "http://google.com");
                Assert.AreEqual(FieldsHelper.GetUrl(item, FieldNames.EmptyField), String.Empty);

                //Assert.AreEqual(FieldsHelper.GetReferenceFieldTargetItemFieldValue(item, FieldNames.LinkFieldInternal, FieldNames.LinkFieldExternal), "http://facebook.com");
            }
        }

        /// <summary>
        /// Tests getting integer values from Sitecore
        /// </summary>
        [Test]
        public void TestGetIntegerValue()
        {
            using (Sitecore.FakeDb.Db masterDb = GenerateFakeDb())
            {
                var item = masterDb.GetItem("/sitecore/content/TestItem");

                int integerValue = FieldsHelper.GetInteger(item, FieldNames.IntegerField);
                
                Assert.AreEqual(integerValue, 123);
            }
        }

        /// <summary>
        /// Tests getting double values from Sitecore
        /// </summary>
        [Test]
        public void TestGetDoubleValue()
        {
            using (Sitecore.FakeDb.Db masterDb = GenerateFakeDb())
            {
                var item = masterDb.GetItem("/sitecore/content/TestItem");
                var expectedValue = 3.14159265358979;
                var expectedAnotherValue = 100000.001;

                // - Get value at default culture -
                double doubleValueDefaultCulture = FieldsHelper.GetDouble(item, FieldNames.FieldWithDoubleValue);

                var defaultCulture = Thread.CurrentThread.CurrentCulture;

                // - Set pl-PL culture -
                Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
                double doubleValuePLCulture = FieldsHelper.GetDouble(item, FieldNames.FieldWithDoubleValue);

                // - Set en-US culture -
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-Us");
                double doubleValueEnUsCulture = FieldsHelper.GetDouble(item, FieldNames.FieldWithDoubleValue);

                Assert.AreEqual(doubleValueDefaultCulture, expectedValue);
                Assert.AreEqual(doubleValuePLCulture, expectedValue);
                Assert.AreEqual(doubleValueEnUsCulture, expectedValue);

                // - Special number format info -
                var format = new NumberFormatInfo
                {
                    NumberDecimalSeparator = ".",
                    NumberGroupSeparator = ",",
                };

                // - Reset default culture -
                Thread.CurrentThread.CurrentCulture = defaultCulture;

                double doubleAnotherValueDefaultCulture = FieldsHelper.GetDouble(item, FieldNames.FieldWithAnotherDoubleValue, format);

                // - Set pl-PL culture -
                Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
                double doubleAnotherValuePLCulture = FieldsHelper.GetDouble(item, FieldNames.FieldWithAnotherDoubleValue);

                // - Set en-US culture -
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-Us");
                double doubleAnotherValueEnUsCulture = FieldsHelper.GetDouble(item, FieldNames.FieldWithAnotherDoubleValue);

                Assert.AreEqual(doubleAnotherValueDefaultCulture, expectedAnotherValue);
                Assert.AreEqual(doubleAnotherValuePLCulture, expectedAnotherValue);
                Assert.AreEqual(doubleAnotherValueEnUsCulture, expectedAnotherValue);
            }
        }
    }
}
