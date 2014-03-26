namespace Constellation.Sitecore.Links
{
	using global::Sitecore;
	using global::Sitecore.Configuration;
	using global::Sitecore.Data;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Data.Managers;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.Globalization;
	using global::Sitecore.IO;
	using global::Sitecore.Links;
	using global::Sitecore.SecurityModel;
	using global::Sitecore.Web;
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	/// <summary>
	/// Constructs HTML-ready URLs based on the supplied Item and Site context.
	/// </summary>
	public class SharedContentLinkBuilder
	{
		#region Fields
		/// <summary>
		/// The mandatory options for building the URL.
		/// </summary>
		private readonly UrlOptions options;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="SharedContentLinkBuilder"/> class.
		/// </summary>
		/// <param name="options">The URL options.</param>
		public SharedContentLinkBuilder(UrlOptions options)
		{
			Assert.ArgumentNotNull(options, "options");
			this.options = options;
		}
		#endregion

		/// <summary>
		/// Gets a value indicating whether to add aspx extension.
		/// </summary>
		/// <value>
		/// <c>true</c> if add aspx extension; otherwise, <c>false</c>.
		/// </value>
		[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
		protected bool AddAspxExtension
		{
			get
			{
				return this.options.AddAspxExtension;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to always include the server URL.
		/// </summary>
		/// <value>
		/// <c>true</c> if always include the server URL; otherwise, <c>false</c>.
		/// </value>
		protected bool AlwaysIncludeServerUrl
		{
			get
			{
				return this.options.AlwaysIncludeServerUrl;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the language should be embedded in the link.
		/// </summary>
		/// <value>
		/// <c>true</c> if LanguageEmbedding is Always, if the context language is different
		/// from the Item language, if the Site is null, or the site's language cookie is null.
		/// </value>
		protected bool EmbedLanguage
		{
			get
			{
				if (this.options.LanguageEmbedding == LanguageEmbedding.Always)
				{
					return true;
				}

				if (this.options.LanguageEmbedding == LanguageEmbedding.Never)
				{
					return false;
				}

				var site = Context.Site;
				if (site == null || WebUtil.GetOriginalCookieValue(site.GetCookieKey("lang")) == null)
				{
					return true;
				}

				return this.options.EmbedLanguage(Context.Language);
			}
		}

		/// <summary>
		/// Gets a value indicating whether to use display name.
		/// </summary>
		/// <value>
		/// <c>true</c> if use display name; otherwise, <c>false</c>.
		/// </value>
		protected bool UseDisplayName
		{
			get
			{
				return this.options.UseDisplayName;
			}
		}

		/// <summary>
		/// Gets the URL.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>An HTML-ready URL.</returns>
		public string GetItemUrl(Item item)
		{
			Assert.ArgumentNotNull(item, "item");

			var siteInfo = this.ResolveTargetSite(item);
			var itemUrlPath = this.GetItemUrlPath(item, siteInfo);
			return this.BuildItemUrl(this.GetUrlAuthorityPart(siteInfo), itemUrlPath);
		}

		/// <summary>
		/// Resolves the target site.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>The siteInfo to use for the URL, or null.</returns>
		private SiteInfo ResolveTargetSite(Item item)
		{
			// return null if the Item DB is core.
			if (item.Database.Name == "core")
			{
				return null;
			}

			// if the options have a Site to use, use it.
			if (this.options.Site != null)
			{
				return this.options.Site.SiteInfo;
			}

			var fullPath = item.Paths.FullPath;

			// If we have a context site, see if the item belongs to it.
			if (Context.Site != null)
			{
				if (fullPath.Contains("media library"))
				{
					return Context.Site.SiteInfo; // media library items can be used by all sites.
				}

				// If the Item is a descendant of the context site item, use the context site.
				if (fullPath.StartsWith(Context.Site.RootPath, StringComparison.OrdinalIgnoreCase))
				{
					return Context.Site.SiteInfo; // direct descendant.
				}

				// if the Item's language matches the context language use the context site.
				if (item.Language.Equals(Context.Language))
				{
					return Context.Site.SiteInfo; // language match.
				}

				// if the Item's language is supported by the context site, use the context site.
				var supportedLanguages = Context.Site.SiteInfo.SupportedLanguages();
				if (supportedLanguages.Any(language => language.Equals(item.Language.Name, StringComparison.OrdinalIgnoreCase)))
				{
					return Context.Site.SiteInfo; // compatible language.
				}
			}

			return this.GetInferredItemSite(item);
		}

		/// <summary>
		/// Looks at the Item's ancestors to find the Site that the Item officially belongs to.
		/// </summary>
		/// <param name="item">The Item to parse.</param>
		/// <returns>A siteInfo or null.</returns>
		private SiteInfo GetInferredItemSite(Item item)
		{
			var sharedContentFolderSetting = SharedContentConfiguration.Instance.SharedContentFolders[item.TemplateName];

			if (sharedContentFolderSetting == null)
			{
				return Context.Site.SiteInfo; // Item has no special rules.
			}

			var siteName = PathUtility.GetFirstFolderFromPath(item.Paths.FullPath, sharedContentFolderSetting.PathToSiteFolder);

			if (string.IsNullOrEmpty(siteName))
			{
				return Context.Site.SiteInfo; // No reasonable site folder name found.
			}

			var siteInfo = Factory.GetSiteInfo(siteName);

			return siteInfo ?? Context.Site.SiteInfo;
		}

		#region URL Construction
		/// <summary>
		/// Builds an item URL for use in HTML markup.
		/// </summary>
		/// <param name="partialHost">The leftmost side of the URL through the Host part.</param>
		/// <param name="relativeItemPath">The item path excluding any Site nodes.</param>
		/// <returns>A browser-ready URL.</returns>
		private string BuildItemUrl(string partialHost, string relativeItemPath)
		{
			Assert.ArgumentNotNull(partialHost, "partialHost");
			Assert.ArgumentNotNull(relativeItemPath, "relativeItemPath");

			var part1 = partialHost.EndsWith("/") ? string.Empty : "/";

			var embedLanguage = this.EmbedLanguage;
			if (embedLanguage && this.options.LanguageLocation == LanguageLocation.FilePath)
			{
				part1 = FileUtil.MakePath(part1, this.options.Language.Name, '/');
			}

			var path = FileUtil.MakePath(part1, relativeItemPath, '/');
			if (path.Length > 1)
			{
				path = StringUtil.RemovePostfix('/', path);
			}

			if (this.options.EncodeNames)
			{
				path = MainUtil.EncodePath(path, '/');
			}

			if (this.AddAspxExtension && path != "/" && path != partialHost)
			{
				path = path + '.' + "aspx";
			}

			if (embedLanguage && this.options.LanguageLocation == LanguageLocation.QueryString)
			{
				path = path + "?sc_lang=" + this.options.Language.Name;
			}

			return partialHost + path;
		}

		/// <summary>
		/// Gets the item path element.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="site">The site.</param>
		/// <returns>The root-relative URL path to the Item as processed by a browser.</returns>
		private string GetItemUrlPath(Item item, SiteInfo site)
		{
			ItemPathType pathType = this.UseDisplayName ? ItemPathType.DisplayName : ItemPathType.Name;

			var itemPath = item.Paths.GetPath(pathType);

			if (site == null)
			{
				return itemPath;
			}

			var pathToHomePage = this.GetRootPath(site, item.Language, item.Database, true);
			if (pathToHomePage.Length > 0 && itemPath.StartsWith(pathToHomePage, StringComparison.OrdinalIgnoreCase))
			{
				itemPath = itemPath.Substring(pathToHomePage.Length); // Truncate all nodes from home page and up.
			}
			else
			{
				// item must be a shared type, so its path is explicit.
				var setting = SharedContentConfiguration.Instance.SharedContentFolders[item.TemplateName];

				if (setting != null)
				{
					itemPath = GetSharedContentItemRelativePath(item, setting.RootPath);
					itemPath = FileUtil.MakePath(setting.FolderName, itemPath);
				}
			}

			if (!string.IsNullOrEmpty(site.VirtualFolder))
			{
				return FileUtil.MakePath(site.VirtualFolder, itemPath);
			}

			return itemPath;
		}

		/// <summary>
		/// Removes all components of an Item's path above and including the supplied ContentFolderName.
		/// </summary>
		/// <param name="item">The item to parse.</param>
		/// <param name="contentFolderName">The folder name to remove.</param>
		/// <returns>A truncated version of the Item's full path.</returns>
		private string GetSharedContentItemRelativePath(Item item, string contentFolderName)
		{
			var path = item.Paths.FullPath.Replace("/sitecore/content/" + contentFolderName + "/", string.Empty);

			var firstSlash = path.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);

			// remove the site name from the path
			return firstSlash > 0 ? path.Substring(firstSlash, path.Length - firstSlash) : path;

			/*
			 * foo/bar
			 * 
			 * firstslash = 3
			 * path.Length = 7
			 * 
			 * remainder = 7 - 3 = 4
			 * 
			 * substring(firstslash, 4) = /bar
			 */
		}

		/// <summary>
		/// Gets the root path of a site.
		/// </summary>
		/// <param name="site">The site.</param>
		/// <param name="language">The language.</param>
		/// <param name="database">The database.</param>
		/// <param name="useStartItem">Defines whether to add start item to the resulting path.</param>
		/// <returns>The site portion of a Sitecore Item path.</returns>
		private string GetRootPath(SiteInfo site, Language language, Database database, bool useStartItem)
		{
			var itemPath = useStartItem ? FileUtil.MakePath(site.RootPath, site.StartItem) : site.RootPath;

			if (!this.UseDisplayName)
			{
				return itemPath;
			}

			if (itemPath.Length == 0)
			{
				return string.Empty;
			}

			var item = ItemManager.GetItem(itemPath, language, global::Sitecore.Data.Version.Latest, database, SecurityCheck.Disable);
			return item == null ? string.Empty : item.Paths.GetPath(ItemPathType.DisplayName);
		}

		/// <summary>
		/// Gets the server URL.
		/// </summary>
		/// <param name="siteInfo">The site to use for the server portion of the URL.</param>
		/// <returns>The schema, hostname and port portion of a URL.</returns>
		private string GetUrlAuthorityPart(SiteInfo siteInfo)
		{
			var contextAuthority = this.AlwaysIncludeServerUrl ? WebUtil.GetServerUrl() : string.Empty;

			var contextSite = Context.Site;
			var siteName = contextSite != null ? contextSite.Name : string.Empty;
			if (siteInfo == null || siteInfo.Name.Equals(siteName, StringComparison.OrdinalIgnoreCase))
			{
				return contextAuthority; // The URL to build is on the same hostname/site as the context.
			}

			var candidateHostName = this.GetTargetHostName(siteInfo) ?? WebUtil.GetHostName();

			if (candidateHostName == string.Empty || candidateHostName.IndexOf('*') >= 0)
			{
				return contextAuthority; // siteInfo's targetHostName isn't defined. Use the current hostname.
			}

			var candidateScheme = !string.IsNullOrEmpty(siteInfo.Scheme) ? siteInfo.Scheme : WebUtil.GetScheme(); // http or https

			var candidatePort = siteInfo.Port > 0 ? siteInfo.Port : WebUtil.GetPort();

			var includePort = (candidateScheme.Equals("https", StringComparison.OrdinalIgnoreCase) && candidatePort != 433)
							|| candidatePort > 80;

			var serverUrl = candidateScheme + "://" + candidateHostName;
			if (includePort)
			{
				return serverUrl + ":" + candidatePort;
			}

			return serverUrl;
		}

		/// <summary>
		/// Gets the name of the target host.
		/// </summary>
		/// <param name="siteInfo">The site info.</param>
		/// <returns>The site's TargetHostName value if available, the Site's hostname value if it is not a wildcard, or an empty string.</returns>
		private string GetTargetHostName(SiteInfo siteInfo)
		{
			Assert.ArgumentNotNull(siteInfo, "siteInfo");

			if (!string.IsNullOrEmpty(siteInfo.TargetHostName))
			{
				return siteInfo.TargetHostName;
			}

			var hostName = siteInfo.HostName;
			return hostName.IndexOfAny(new[] { '*', '|' }) < 0 ? hostName : string.Empty;
		}
		#endregion
	}
}