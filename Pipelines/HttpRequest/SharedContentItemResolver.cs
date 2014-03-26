namespace Constellation.Sitecore.Pipelines.HttpRequest
{
	using Constellation.Sitecore;
	using Constellation.Sitecore.Links;
	using global::Sitecore;
	using global::Sitecore.Configuration;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.Pipelines.HttpRequest;
	using global::Sitecore.Web;
	using System;
	using System.Linq;

	/// <summary>
	/// Drop-in replacement for the default Sitecore ItemResolver that handles
	/// locating content that is site-specific but is not stored below
	/// the site's start node.
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
			Item item;

			if (Context.PageMode.IsNormal)
			{
				item = this.ResolveItem(args);

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
				// Page Editor and Preview mode support
				this.RunStandardItemResolver(args);
				item = Context.Item;
				if (item == null)
				{
					return; // no valid item found.
				}

				Tracer.Info("Current item is \"" + item.Paths.Path + "\".");

				/////* 
				//// * When we're in Preview or Page Editor, 
				//// * we need to manually adjust the site
				//// */

				// Figure out if the Item is a decendant of the current site.
				var itemPath = item.Paths.FullPath;
				var siteRoot = Context.Site.SiteInfo.RootPath;

				if (itemPath.StartsWith(siteRoot, StringComparison.InvariantCultureIgnoreCase))
				{
					// Yep, it's an Item under the root of the site.
					return;
				}

				// We need to switch to the correct site.
				var sites = Factory.GetSiteInfoList();
				SiteInfo correctedSite = null;

				foreach (var site in sites)
				{
					if (itemPath.StartsWith(site.RootPath, StringComparison.InvariantCultureIgnoreCase))
					{
						correctedSite = site;
						break;
					}
				}

				// Fix the site
				if (correctedSite == null)
				{
					// stay where we are
					return;
				}

				string prefix = "http://" + correctedSite.TargetHostName;

				// Kill the request and redirect to the appropriate site prefix.
				if (correctedSite.VirtualFolder.Length > 1)
				{
					// The Virtual folder wasn't "root" ( = "/")
					prefix += correctedSite.VirtualFolder.Substring(0, correctedSite.VirtualFolder.Length - 1);
				}

				var adjustedUri = new Uri(prefix + args.Context.Request.Url.PathAndQuery);
				args.AbortPipeline();
				args.Context.Response.Redirect(adjustedUri.OriginalString, true);
			}
		}

		/// <summary>
		/// Runs the default Sitecore Item resolver when operating within the
		/// context of a Sitecore Client.
		/// </summary>
		/// <param name="args">The parameters for this request.</param>
		protected override void Defer(HttpRequestArgs args)
		{
			this.RunStandardItemResolver(args);
		}

		/// <summary>
		/// Runs the default Sitecore Item resolver.
		/// </summary>
		/// <param name="args">The parameters for this request.</param>
		protected void RunStandardItemResolver(HttpRequestArgs args)
		{
			var resolver = new SharedContentItemResolver();
			resolver.Process(args);
		}

		/// <summary>
		/// Executes the standard Item Resolver, and if no result is found,
		/// attempts to look for the Item within shared content folders.
		/// </summary>
		/// <param name="args">The request args.</param>
		/// <returns>The context item or null.</returns>
		private Item ResolveItem(HttpRequestArgs args)
		{
			// Check for the Item in the usual spot.
			this.RunStandardItemResolver(args);

			if (Context.Item != null)
			{
				return Context.Item; // we found the item.
			}

			Item item = null;
			var folder = PathUtility.GetFirstFolderFromPath(args.Url.ItemPath);

			var settings = SharedContentConfiguration.Instance.SharedContentFolders;
			var rootPath = (from SharedContentFolder setting in settings where setting.FolderName.Equals(folder, StringComparison.InvariantCultureIgnoreCase) select setting.RootPath).FirstOrDefault();

			if (!string.IsNullOrEmpty(rootPath))
			{
				var relativePath = this.GetRelativePathToItem(this.RemoveFlagFolderFromPath(args.Url.ItemPath, folder));
				item = this.GetItem(rootPath, relativePath);
			}

			if (item != null)
			{
				item = item.GetBestFitLanguageVersion(Context.Language);
			}

			return item;
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

			return path.Replace(flagFolderName, string.Empty);
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

			if (!path.Contains("/"))
			{
				return path;
			}

			var slash = path.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);
			var length = path.Length - (slash + 1);

			return path.Substring(slash + 1, length);

			/*
			 * A little math
			 *  
			 * "foo/bar"
			 * 
			 * index of slash = 3
			 * length of string = 7
			 * length of remaining string = 7 - 3 = 4
			 * 
			 * substring(3, 4) = "/bar"
			 * 
			 */
		}

		/// <summary>
		/// Queries for an Item based upon a root path, the context site, and the suffix path.
		/// </summary>
		/// <param name="prefix">The root path to the folder where Items are kept.</param>
		/// <param name="suffix">The date, alpha, or name path to the Item.</param>
		/// <returns>The Item or null.</returns>
		private Item GetItem(string prefix, string suffix)
		{
			var siteName = Context.Site.Name;
			var item = DatasourceResolver.Resolve(prefix + "/" + siteName + "/" + suffix, Context.Site.Database);

			return item ?? DatasourceResolver.Resolve(prefix + "/*/" + suffix, Context.Site.Database);
		}
		#endregion
	}
}