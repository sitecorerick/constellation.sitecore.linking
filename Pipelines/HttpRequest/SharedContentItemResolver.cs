namespace Constellation.Sitecore.Pipelines.HttpRequest
{
	using Constellation.Sitecore;
	using Constellation.Sitecore.Links;
	using global::Sitecore;
	using global::Sitecore.Configuration;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.Pipelines.HttpRequest;
	using System;


	/// <summary>
	/// Follows the default Sitecore ItemResolver that handles
	/// locating content that is site-specific but is not stored below
	/// the site's start node. Also ensures the context item is in 
	/// a useable language.
	/// </summary>
	public class SharedContentItemResolver : HttpRequestProcessor
	{
		#region Methods
		/// <summary>
		/// Given the request arguments, locates the item requested.
		/// </summary>
		/// <param name="args">The parameters for this request.</param>
		protected override void Execute(HttpRequestArgs args)
		{
			if (Context.PageMode.IsNormal)
			{
				if (Context.Item != null)
				{
					return; // The normal Item Resolver has found the Item, no work to do.
				}

				Tracer.Info("SharedContentItemResolver is attempting to resolve the item based on:" + args.Url.ItemPath);
				var item = this.ResolveItem(args);

				if (item == null)
				{
					return; // no valid item found.
				}

				if (item.LanguageVersionIsEmpty())
				{
					// Explicitly clear the context item so the 404 page fires.
					Context.Item = null;
					return;
				}

				Context.Item = item;
				Tracer.Info("Current item is \"" + item.Paths.Path + "\".");
			}
			else
			{
				this.ExecuteInExperienceEditorMode(args);
			}
		}

		/// <summary>
		/// Runs the default Sitecore Item resolver when operating within the
		/// context of a Sitecore Client.
		/// </summary>
		/// <param name="args">The parameters for this request.</param>
		protected override void Defer(HttpRequestArgs args)
		{
			// We don't need to do anything.
		}

		/// <summary>
		/// Assuming there's a Context Item, this routine determines if Sitecore should change
		/// hostnames to allow editing of a page that officially belongs to another site.
		/// </summary>
		/// <param name="args">
		/// The args.
		/// </param>
		private void ExecuteInExperienceEditorMode(HttpRequestArgs args)
		{
			Tracer.Info("SharedContentItemResolver executing in Page Editor mode.");

			// Page Editor and Preview mode support
			var item = Context.Item;
			if (item == null)
			{
				return; // no item found using the stock Item Resolver in Page Edit mode is a 404 condition.
			}

			Tracer.Info("Current item is \"" + item.Paths.Path + "\".");


			// Figure out if the Item is a decendant of the current site.
			var itemPath = item.Paths.FullPath;
			var siteRoot = Context.Site.SiteInfo.RootPath;

			if (itemPath.StartsWith(siteRoot, StringComparison.InvariantCultureIgnoreCase))
			{
				return; // Item is under the site root and is technically not a shared item.
			}

			// We need to switch to the correct site.
			var setting = SharedContentLinkProviderConfiguration.Settings.Rules[item.TemplateName];
			if (!setting.CategorizedBySiteFolder)
			{
				return; // nothing to check.
			}

			var siteName = this.GetOfficialSiteNameFromPath(itemPath, setting.PathToSiteFolder);

			var nativeSite = Factory.GetSite(siteName);
			if (nativeSite == null)
			{
				return; // no site match.
			}

			if (nativeSite.Name.Equals(Context.Site.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				return; // same site.
			}

			Tracer.Info(item.Paths.FullPath + "resolved to a different site for editing. Redirecting to " + nativeSite.TargetHostName);
			args.AbortPipeline();
			args.Context.Response.Redirect("http://" + nativeSite.TargetHostName + "/sitecore", true);
		}

		/// <summary>
		/// Executes the standard Item Resolver, and if no result is found,
		/// attempts to look for the Item within shared content folders.
		/// </summary>
		/// <param name="args">The request args.</param>
		/// <returns>The context item or null.</returns>
		private Item ResolveItem(HttpRequestArgs args)
		{
			Item item = null;

			var folder = PathUtility.GetFirstFolderFromPath(args.Url.ItemPath);

			var settings = SharedContentLinkProviderConfiguration.Settings.Rules;
			SharedContentLinkProviderRule setting = null;

			foreach (SharedContentLinkProviderRule rule in settings)
			{
				if (rule.FolderName.Equals(folder, StringComparison.InvariantCultureIgnoreCase))
				{
					setting = rule;
					Tracer.Info("SharedContentItemResolver found a matching rule for flag folder: " + folder);
					break;
				}
			}

			if (setting == null)
			{
				Tracer.Info("SharedContentItemResolver found no rule for flag folder: " + folder);
				return null;
			}

			if (!string.IsNullOrEmpty(setting.PathToSiteFolder))
			{
				var pathWithoutFlag = this.RemoveFlagFolderFromPath(args.Url.ItemPath, folder);
				var relativePath = this.GetRelativePathToItem(pathWithoutFlag);
				item = this.GetItem(setting.PathToSiteFolder, relativePath, setting.CategorizedBySiteFolder);
			}

			if (item == null)
			{
				return null;
			}

			return item.GetBestFitLanguageVersion(Context.Language);
		}
		#endregion

		#region Helpers
		/// <summary>
		/// Removes flag folder from path.
		/// </summary>
		/// <param name="itemPath">
		/// The item path.
		/// </param>
		/// <param name="flagFolderName">
		/// The flag folder name.
		/// </param>
		/// <returns>
		/// The path without the flag folder<see cref="string"/>.
		/// </returns>
		private string RemoveFlagFolderFromPath(string itemPath, string flagFolderName)
		{
			var path = MainUtil.DecodeName(itemPath).Replace(Context.Site.StartPath, string.Empty).ToLower();

			if (path.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
			{
				path = path.Substring(1, path.Length - 1);
			}

			var flagIndex = path.IndexOf(flagFolderName, StringComparison.InvariantCultureIgnoreCase);

			path = path.Remove(flagIndex, flagFolderName.Length);

			return path.Replace("//", "/");
		}

		/// <summary>
		/// The get official site name from path.
		/// </summary>
		/// <param name="itemPath">
		/// The item path.
		/// </param>
		/// <param name="pathToSiteFolder">
		/// The path to site folder.
		/// </param>
		/// <returns>
		/// The <see cref="string"/>.
		/// </returns>
		private string GetOfficialSiteNameFromPath(string itemPath, string pathToSiteFolder)
		{
			string path = itemPath.Replace(pathToSiteFolder + "/", string.Empty);

			var firstSlash = path.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);
			return path.Substring(0, firstSlash);
		}

		/// <summary>
		/// Provides access to the part of the URL that will remain the same after it is
		/// transplanted to the correct folder.
		/// </summary>
		/// <param name="itemPath">The request's Item Path.</param>
		/// <returns>The URL to transplant.</returns>
		private string GetRelativePathToItem(string itemPath)
		{
			var path = MainUtil.DecodeName(itemPath).Replace(Context.Site.StartPath, string.Empty).ToLower();

			if (path.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
			{
				path = path.Substring(1, path.Length - 1);
			}

			return path;
		}

		/// <summary>
		/// Queries for an Item based upon a root path, the context site, and the suffix path.
		/// </summary>
		/// <param name="prefix">
		/// The root path to the folder where Items are kept.
		/// </param>
		/// <param name="suffix">
		/// The date, alpha, or name path to the Item.
		/// </param>
		/// <param name="categorizedBySiteFolder">
		/// The categorized By Site Folder.
		/// </param>
		/// <returns>
		/// The Item or null.
		/// </returns>
		private Item GetItem(string prefix, string suffix, bool categorizedBySiteFolder)
		{
			Item item;
			string path = prefix + "/" + suffix;

			if (!categorizedBySiteFolder)
			{
				item = DatasourceResolver.Resolve(path, Context.Site.Database);
			}
			else
			{
				var siteName = Context.Site.Name;
				path = prefix + "/" + siteName + "/" + suffix;
				item = DatasourceResolver.Resolve(path, Context.Site.Database);

				if (item == null)
				{
					Tracer.Info("SharedContentItemResolver did not find an item at expected site path: " + path);
					path = prefix + "/*/" + suffix;
					item = DatasourceResolver.Resolve(path, Context.Site.Database);
				}
			}

			if (item == null)
			{
				Tracer.Info("SharedContetnItemResolver did not find an item at expected path: " + path);
			}

			return item;
		}
		#endregion
	}
}