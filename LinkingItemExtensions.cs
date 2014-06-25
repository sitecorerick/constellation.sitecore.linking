namespace Constellation.Sitecore.Links
{
	using global::Sitecore;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using System;

	/// <summary>
	/// The shared content links.
	/// </summary>
	public static class LinkingItemExtensions
	{
		#region Methods
		/// <summary>
		/// Gets the parent page for shared content item in the context of
		/// the context site. Note that this method won't work in a background
		/// process.
		/// </summary>
		/// <param name="item">
		/// The item.
		/// </param>
		/// <returns>
		/// The <see cref="Item"/>.
		/// </returns>
		public static Item GetContextParentForSharedContentItem(this Item item)
		{
			if (Context.Site == null)
			{
				return null;
			}

			if (item.Paths.FullPath.StartsWith(Context.Site.RootPath, StringComparison.InvariantCultureIgnoreCase))
			{
				return item.Parent;
			}

			var siteName = Context.Site.Name;
			Item parent = null;
			var settings = SharedContentLinkProviderConfiguration.Settings.Rules;

			foreach (SharedContentLinkProviderRule setting in settings)
			{
				if (!item.Paths.FullPath.StartsWith(setting.RootPath, StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}

				// Get the parent item
				var parentQuery = setting.ParentPageQuery.Replace("$site", siteName);

				try
				{
					parent = item.Database.SelectSingleItem(parentQuery);
				}
				catch (Exception ex)
				{
					var message = "Error attempting to get configured parent item from query: " + parentQuery;
					Log.Error(message, ex, typeof(LinkingItemExtensions));
					throw new Exception(message, ex);
				}
				break;
			}

			return parent;
		}
		#endregion
	}
}
