﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Shell.Framework.Commands;
using System.Collections.Specialized;
using Sitecore.Data.Items;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Configuration;
using Sitecore.Modules.Eviblog.Utilities;
using Sitecore.Globalization;

namespace Sitecore.Modules.Eviblog.Commands
{
    public class NewCategory : Command
    {
        public override void Execute(CommandContext context)
        {
            if (context.Items.Length == 1)
            {
                Item item = context.Items[0];
                NameValueCollection parameters = new NameValueCollection();
                parameters["currentid"] = item.ID.ToString();
                parameters["tid"] = item.TemplateID.ToString();
                parameters["database"] = item.Database.Name;
                Context.ClientPage.Start(this, "Run", parameters);
            }
        }

        protected void Run(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    string itemTitle = args.Result;

                    Database current = Factory.GetDatabase("master");
                    TemplateID template = new TemplateID(new ID(Settings.Default.CategoryTemplateID));

                    Item currentItem = current.GetItem(args.Parameters["currentid"]);
                    Item categories = currentItem.Axes.GetChild("Categories");

                    Item newItem = ItemManager.AddFromTemplate(itemTitle, template, categories);

                    Publish.PublishItem(newItem);

                    SheerResponse.Eval("scForm.browser.getParentWindow(scForm.browser.getFrameElement(window).ownerDocument).location.reload(true)");

                    args.WaitForPostBack(true);
                }
            }
            else
            {
                string currentTID = args.Parameters["tid"];

                if (currentTID != Settings.Default.BlogTemplateID && currentTID != Settings.Default.EntryTemplateID)
                {
                    Context.ClientPage.ClientResponse.Alert("Please create or select a blog first");
                }
                else
                {
                    SheerResponse.Input("Enter the name of your new category:", "", Configuration.Settings.ItemNameValidation, Translate.Text("'$Input' is not a valid name."), 100);
                    args.WaitForPostBack(true);
                }
            }
        }
    }
}