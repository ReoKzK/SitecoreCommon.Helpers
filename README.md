# SitecoreCommon.Helpers
Sitecore helpers

##PublishingHelper
Helps set publishing directly from code

Usage:

```C#

// - First we need to set up master database (internal reference) -
PublishingHelper.SetUpMasterDatabase();

// - Then we need to set up publication databases by their names -
PublishingHelper.SetUpPublicationDatabases(new List<String> { "web", "pub" });

// - Now we can publish items. First parameter can be sitecore Item...
PublishingHelper.PublishItem(Astra, Sitecore.Publishing.PublishMode.Full);

// - ...or Sitecore Item ID string. Second parameter is PublishMode -
PublishingHelper.PublishItem("{9C59A610-537C-48A7-97B8-AB4B19DFBB32}", Sitecore.Publishing.PublishMode.Full);
