﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Links;

namespace Sitecore.Modules.WeBlog.Extensions
{
    /// <summary>
    /// Provides utilities for working with Sitecore items
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Determine if an item is based on a given template or if the item's template is based on the given template
        /// </summary>
        /// <param name="item">The item to test the template of</param>
        /// <param name="template">The template which the item's template should be or inherit from</param>
        /// <returns>True if the item's template is based on the given template, otherwise false</returns>
        [Obsolete("Use TemplateIsOrBasedOn(Item, ID) instead.")] // deprecated 3.0
        public static bool TemplateIsOrBasedOn(this Item item, TemplateItem template)
        {
            if (template != null)
                return TemplateIsOrBasedOn(item, template.ID);
            else
                return false;
        }

        /// <summary>
        /// Determine if an item is based on a given template or if the item's template is based on the given template
        /// </summary>
        /// <param name="item">The item to test the template of</param>
        /// <param name="templateId">The ID of the template which the item's template should be or inherit from</param>
        /// <returns>True if the item's template is based on the given template, otherwise false</returns>
        public static bool TemplateIsOrBasedOn(this Item item, ID templateId)
        {
            return TemplateIsOrBasedOn(item, new[] { templateId });

        }

        /// <summary>
        /// Determine if an item is based on a given template or if the item's template is based on the given template
        /// </summary>
        /// <param name="item">The item to test the template of</param>
        /// <param name="templateIds">The IDs of the templates which the item's template should be or inherit from</param>
        /// <returns>True if the item's template is based on the given templates, otherwise false</returns>
        public static bool TemplateIsOrBasedOn(this Item item, IEnumerable<ID> templateIds)
        {
            if (item == null || templateIds == null || !templateIds.Any())
                return false;

            var template = TemplateManager.GetTemplate(item.TemplateID, item.Database);
            if (template == null)
                return false;

            var match = from templateId in templateIds
                where template.DescendsFromOrEquals(templateId)
                select templateId;

            return match.Any();
        }

        /// <summary>
        /// Determine if an item is based on a given template or if the item's template is based on the given template
        /// </summary>
        /// <param name="item">The item to test the template of</param>
        /// <param name="template">The template which the item's template should be or inherit from</param>
        /// <returns>True if the item's template is based on the given template, otherwise false</returns>
        [Obsolete("Use TemplateManager.DescendsFromOrEquals instead.")] // deprecated 3.0
        public static bool TemplateIsOrBasedOn(TemplateItem itemTemplate, TemplateItem baseTemplate)
        {
            return TemplateIsOrBasedOn(itemTemplate, baseTemplate.ID);
        }

        /// <summary>
        /// Determine if an item is based on a given template or if the item's template is based on the given template
        /// </summary>
        /// <param name="item">The item to test the template of</param>
        /// <param name="template">The ID of the template which the item's template should be or inherit from</param>
        /// <returns>True if the item's template is based on the given template, otherise false</returns>
        [Obsolete("Use TemplateIsOrBasedOn(Item, ID) instead.")] // deprecated 3.0
        public static bool TemplateIsOrBasedOn(TemplateItem itemTemplate, ID baseTemplate)
        {
            if (itemTemplate == null || baseTemplate == ID.Null)
                return false;

            if (itemTemplate.ID == baseTemplate)
                return true;

            else
            {
                bool result = false;
                foreach (var template in itemTemplate.BaseTemplates)
                {
                    result = TemplateIsOrBasedOn(template, baseTemplate);
                    if (result)
                        return result;
                }
            }

            return false;
        }

        /// <summary>
        /// Find items below another based on the specified template or a derived template
        /// </summary>
        /// <param name="rootItem">The item the item must be below</param>
        /// <param name="template">The template to find item based on, or based on derivaties of the template</param>
        /// <returns>All items which match</returns>
        public static Item[] FindItemsByTemplateOrDerivedTemplate(this Item rootItem, TemplateItem template)
        {
            if (rootItem == null || template == null)
                return new Item[0];

            var foundItems = new List<Item>();
            var derivedTemplates = new List<TemplateItem>();

            var references = Sitecore.Globals.LinkDatabase.GetReferrers(template);
            foreach (var reference in references)
            {
                var source = reference.GetSourceItem();
                if (source.Template.Key == "template")
                    derivedTemplates.Add((TemplateItem)source);
                else
                {
                    if (source.Axes.IsDescendantOf(rootItem))
                        foundItems.Add(source);
                }
            }

            foreach (var derivedTemplate in derivedTemplates)
            {
                foundItems.AddRange(FindItemsByTemplateOrDerivedTemplate(rootItem, derivedTemplate));
            }

            return foundItems.ToArray();
        }

        /// <summary>
        /// Finds the current type of item for the given item
        /// </summary>
        /// <param name="item">The item to search from</param>
        /// <param name="template">The template the target item must be based on or derived from</param>
        /// <returns>The target item if found, otherwise null</returns>
        [Obsolete("Use FindAncestorByTemplate instead.")]
        public static Item GetCurrentItem(this Item item, string template)
        {
            return FindAncestorByTemplate(item, template);
        }

        /// <summary>
        /// Finds the current type of item for the given item
        /// </summary>
        /// <param name="item">The item to search from</param>
        /// <param name="template">The template the target item must be based on or derived from</param>
        /// <returns>The target item if found, otherwise null</returns>
        public static Item FindAncestorByTemplate(this Item item, string template)
        {
            if (item == null)
                return null;

            var templateItem = item.Database.GetTemplate(template);
            return FindAncestorByTemplate(item, templateItem.ID);
        }

        [Obsolete("Use FindAncestorByTemplate instead.")] // deprecated 3.0
        public static Item GetCurrentItem(this Item item, ID templateId)
        {
            return FindAncestorByTemplate(item, templateId);
        }

        /// <summary>
        /// Finds the current type of item for the given item
        /// </summary>
        /// <param name="item">The item to search from</param>
        /// <param name="templateId">The template the target item must be based on or derived from</param>
        /// <returns>The target item if found, otherwise null</returns>
        public static Item FindAncestorByTemplate(this Item item, ID templateId)
        {
            if (item == null)
                return null;

            var currentItem = item;

            while (currentItem != null && !TemplateIsOrBasedOn(currentItem, templateId))
            {
                currentItem = currentItem.Parent;
            }

            return currentItem;
        }

        /// <summary>
        /// Finds the current type of item for the given item
        /// </summary>
        /// <param name="item">The item to search from</param>
        /// <param name="templateIds">The template the target item must be based on or derived from</param>
        /// <returns>The target item if found, otherwise null</returns>
        public static Item FindAncestorByAnyTemplate(this Item item, IEnumerable<ID> templateIds)
        {
            if (item == null)
                return null;

            var currentItem = item;

            while (currentItem != null && !TemplateIsOrBasedOn(currentItem, templateIds))
            {
                currentItem = currentItem.Parent;
            }

            return currentItem;
        }

        /// <summary>
        /// Gets the content of the title field if not empty, otherwise the item name
        /// </summary>
        /// <param name="item">The item to get the title for</param>
        /// <returns>The title if not empty, otherwise the item's name</returns>
        public static string GetItemTitle(this Item item)
        {
            if (item != null)
            {
                var title = item["title"];
                return string.IsNullOrEmpty(title) ? item.Name : title;
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the URL for an item
        /// </summary>
        /// <param name="item">The item to get the URL for</param>
        /// <returns>The URL for the item if valid, otherwise an empty string</returns>
        public static string GetUrl(this Item item)
        {
            return LinkManager.GetItemUrl(item);
        }

        /// <summary>
        /// Determines if the field given by name needs to have it's output wrapped in an additional tag
        /// </summary>
        /// <param name="item">The item to check field on</param>
        /// <param name="fieldName">The name of the field to check</param>
        /// <returns>True if wrapping is required, otherwsie false  </returns>
        public static bool DoesFieldRequireWrapping(this Item item, string fieldName)
        {
            return !item[fieldName].StartsWith("<p>");
        }
    }
}
