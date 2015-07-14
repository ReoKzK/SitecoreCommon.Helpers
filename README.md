# SitecoreCommon.Helpers
Sitecore helpers

###Installation
By nuget:
```
Install-Package SitecoreCommon.Helpers 
```

In code file add using:
```C#
using SitecoreCommon.Helpers;
```

##PublishingHelper
Helps set publishing directly from code

Usage:

```C#

// - First we need to set up master database (internal reference) -
PublishingHelper.SetUpMasterDatabase();

// - Then we need to set up publication databases by their names -
PublishingHelper.SetUpPublicationDatabases(new List<String> { "web", "pub" });

// - Now we can publish items. First parameter can be sitecore Item...
PublishingHelper.PublishItem(SitecoreItem, Sitecore.Publishing.PublishMode.Full);

// - ...or Sitecore Item ID string. Second parameter is PublishMode -
PublishingHelper.PublishItem("{9C59A610-537C-48A7-97B8-AB4B19DFBB32}", Sitecore.Publishing.PublishMode.Full);
```

##FieldsHelper
Helps get fields values

Usage:

```C#
// - Gets value from field 'Key' in Sitecore item 'item' -
String testValue = FieldsHelper.GetValue(item, "Key");

// - Gets value from field 'Key' in Sitecore item 'item' - if field does not exists, returns value specified in third parameter -
String testValue2 = FieldsHelper.GetValue(item, "Key that not exists", "Default value");

// - Gets value from field 'Key' in Sitecore item 'item' - if field is empty returns value specified in third parameter -
String testValue3 = FieldsHelper.GetValueOrDefaultIfEmpty(item, "Key empty", "Default value");

// - Cheks if specified field "Key" exists in Sitecore Item 'item' -
bool testIfFieldExists = FieldsHelper.HasField(item, "Key");

// - Cheks if specified field "Key" exists in Sitecore Item 'item' and is no empty -
bool testIfFieldExistsAndIsNotEmpty = FieldsHelper.HasNotEmptyField(item, "Key");

// - Checks if checkbox field exists and is checked -
bool testIfCheckboxFieldExistsAndIsChecked = FieldsHelper.HasCheckedCheckboxField(item, "Key checkbox");

// - Gets checkbox field -
var checkboxField = FieldsHelper.GetCheckboxField(item, "Key checkbox");

// - Now let's check it -
bool isCheckboxFieldChecked = checkboxField != null && checkboxField.Checked;

// - Get integer value from Sitecore -
int integerValue = FieldsHelper.GetInteger(item, "Integer Field");

// - Get double value from Sitecore -
double doubleValue = FieldsHelper.GetDouble(item, "Field With Double Value");
```
