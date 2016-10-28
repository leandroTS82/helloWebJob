using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using System.Security;

namespace helloWebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = (NameValueCollection)ConfigurationManager.GetSection("Sites");
            foreach (var key in config.Keys)
            {
                Uri siteUri = new Uri(config.GetValues(key as string)[0]);
                string listname = "listWebJob";
                string realm = TokenHelper.GetRealmFromTargetUrl(siteUri);
                string accessToken = TokenHelper.GetAppOnlyAccessToken(
                    TokenHelper.SharePointPrincipal,
                    siteUri.Authority, realm).AccessToken;

                using (var clientContext =
                    TokenHelper.GetClientContextWithAccessToken(
                        siteUri.ToString(), accessToken))
                {
                    CheckListExists(clientContext, listname);
                    AddListItem(clientContext, listname);
                }
            }
        }

        private static void CheckListExists(ClientContext clientContext, string listName)
        {
            ListCollection listCollection = clientContext.Web.Lists;
            clientContext.Load(listCollection, lists => lists.Include(list => list.Title).Where(list => list.Title == listName));
            clientContext.ExecuteQuery();
            if (listCollection.Count <= 0)
            {
                CreateList(clientContext, listName);
            }

        }
        private static void CreateList(ClientContext clientContext, string listName)
        {
            Web currentWeb = clientContext.Web;
            ListCreationInformation creationInfo = new ListCreationInformation();
            creationInfo.Title = listName;
            creationInfo.TemplateType = (int)ListTemplateType.GenericList;
            List list = currentWeb.Lists.Add(creationInfo);
            list.Description = "My custom list";
            list.Update();
            clientContext.ExecuteQuery();
        }
        private static void AddListItem(ClientContext clientContext, string listName)
        {
            Web currentWeb = clientContext.Web;
            var myList = clientContext.Web.Lists.GetByTitle(listName);
            ListItemCreationInformation listItemCreate = new ListItemCreationInformation();
            Microsoft.SharePoint.Client.ListItem newItem = myList.AddItem(listItemCreate);
            newItem["Title"] = "Item added by Job at " + DateTime.Now;
            newItem.Update();
            clientContext.ExecuteQuery();
        }
    }
}
