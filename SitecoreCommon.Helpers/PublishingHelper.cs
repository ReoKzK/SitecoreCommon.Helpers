using System;
using System.Collections.Generic;
using Sitecore.Data.Items;
using Sitecore.Publishing;

namespace SitecoreCommon.Helpers
{
    /// <summary>
    /// Publishing Helper
    /// </summary>
    public static class PublishingHelper
    {
        /// <summary>
        /// Sitecore Master Database
        /// </summary>
        private static Sitecore.Data.Database masterDB;

        /// <summary>
        /// Sitecore Publication Databases
        /// </summary>
        private static List<Sitecore.Data.Database> publicationDBs;

        /// <summary>
        /// Set up master database
        /// </summary>
        public static void SetUpMasterDatabase()
        {
            masterDB = masterDB ?? Sitecore.Configuration.Factory.GetDatabase("master");
        }

        /// <summary>
        /// Set up publication databases
        /// </summary>
        /// <param name="databases">List of publication databases names</param>
        public static void SetUpPublicationDatabases(List<String> databases)
        {
            publicationDBs = new List<Sitecore.Data.Database>();

            foreach (String dbName in databases)
            {
                publicationDBs.Add(Sitecore.Configuration.Factory.GetDatabase(dbName));
            }
        }

        /// <summary>
        /// Publishes item to all publication databases
        /// </summary>
        /// <param name="item">Sitecore Item to publish</param>
        /// <param name="publishMode">Sitecore publish mode</param>
        public static void PublishItem(Item item, PublishMode publishMode = PublishMode.SingleItem)
        {
            if (masterDB != null && publicationDBs != null && publicationDBs.Count > 0)
            {
                var itemPublishToPublicationDatabasesOptionsArray = new PublishOptions[publicationDBs.Count][];

                for (int i = 0; i < publicationDBs.Count; i++)
                {
                    itemPublishToPublicationDatabasesOptionsArray[i] = new PublishOptions[masterDB.Languages.Length];

                    for (var j = 0; j < masterDB.Languages.Length; j++)
                    {
                        itemPublishToPublicationDatabasesOptionsArray[i][j] = new PublishOptions(masterDB, publicationDBs[i], publishMode, masterDB.Languages[j], DateTime.Now)
                        {
                            Deep = true,
                            CompareRevisions = true,
                            RootItem = item,
                        };
                    }

                    PublishManager.Publish(itemPublishToPublicationDatabasesOptionsArray[i]);
                }
            }
        }

        /// <summary>
        /// Publishes item to all publication databases
        /// </summary>
        /// <param name="itemID">ID of Sitecore Item to publish</param>
        /// <param name="publishMode">Sitecore publish mode</param>
        public static void PublishItem(String itemID, PublishMode publishMode = PublishMode.SingleItem)
        {
            if (masterDB != null)
            {
                var item = masterDB.GetItem(itemID);

                PublishItem(item, publishMode);
            }
        }
    }
}
