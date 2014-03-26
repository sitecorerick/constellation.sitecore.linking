namespace Constellation.Sitecore.Links
{
	using global::Sitecore;
	using global::Sitecore.Configuration;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.Links;
	using global::Sitecore.Web;
	using System;
	using System.Collections.Specialized;

	/// <summary>
	/// Provides much needed customization abilities to Sitecore's 
	/// LinkManager technology by providing the ability to set a 
	/// default LinkProvider for each site defined in the Sites collection.
	/// </summary>
	public class SwitchingLinkProvider : LinkProvider
	{
		#region Fields
		/// <summary>
		/// The URL options.
		/// </summary>
		private readonly UrlOptions defaultUrlOptions;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="SwitchingLinkProvider"/> class.
		/// </summary>
		public SwitchingLinkProvider()
		{
			this.defaultUrlOptions = new UrlOptions();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Given an Item's full path and a language code, find the best matching site.
		/// </summary>
		/// <param name="itemFullPath">
		/// The item full path.
		/// </param>
		/// <param name="itemLanguage">
		/// The item language.
		/// </param>
		/// <returns>
		/// The <see cref="SiteInfo"/>. Can be null if no matches are found.
		/// </returns>
		public static SiteInfo FindMatchingSite(string itemFullPath, string itemLanguage)
		{
			SiteInfo match = null;

			var sites = Factory.GetSiteInfoList();

			foreach (var site in sites)
			{
				var startPath = site.RootPath + site.StartItem;

				if (itemFullPath.StartsWith(startPath, StringComparison.InvariantCultureIgnoreCase))
				{
					if (site.Language.Equals(itemLanguage, StringComparison.InvariantCultureIgnoreCase))
					{
						match = site;
						break;
					}

					match = site;
				}
			}

			return match;
		}

		/// <summary>
		/// Expands the dynamic links.
		/// </summary>
		/// <param name="text">The text.</param><param name="resolveSites">Set this to <c>true</c> to resolve site information when expanding dynamic links.</param>
		/// <returns>The expanded dynamic links.</returns>
		public override string ExpandDynamicLinks(string text, bool resolveSites)
		{
			Assert.ArgumentNotNull(text, "text");
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			var options = this.GetDefaultUrlOptions();
			options.SiteResolving = resolveSites;

			var contentExpander = new ContentLinkExpander();
			contentExpander.ExpandLinks(ref text, this, options);

			var mediaExpander = new MediaLinkExpander();
			mediaExpander.Expand(ref text, options);

			return text;
		}

		/// <summary>
		/// Gets (a clone of) the default URL options.
		/// </summary>
		/// <returns>The default URL options.</returns>
		public override UrlOptions GetDefaultUrlOptions()
		{
			return (UrlOptions)this.defaultUrlOptions.Clone();
		}

		/// <summary>
		/// Gets the (friendly) URL of an item.
		/// </summary>
		/// <param name="item">The item.</param><param name="options">The options.</param>
		/// <returns>The item URL.</returns>
		public override string GetItemUrl(Item item, UrlOptions options)
		{
			var provider = ResolveProvider(item, options);

			return provider.GetItemUrl(item, options);
		}

		/// <summary>
		/// Initializes the provider.
		/// </summary>
		/// <param name="name">The friendly name of the provider.</param>
		/// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
		/// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
		/// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
		/// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.</exception>
		public override void Initialize(string name, NameValueCollection config)
		{
			Assert.ArgumentNotNullOrEmpty(name, "name");
			Assert.ArgumentNotNull(config, "config");

			base.Initialize(name, config);
			this.defaultUrlOptions.AddAspxExtension = MainUtil.GetBool(config["addAspxExtension"], true);
			this.defaultUrlOptions.AlwaysIncludeServerUrl = MainUtil.GetBool(config["alwaysIncludeServerUrl"], false);
			this.defaultUrlOptions.EncodeNames = MainUtil.GetBool(config["encodeNames"], true);
			this.defaultUrlOptions.LanguageEmbedding = GetLanguageEmbedding(config["languageEmbedding"], LanguageEmbedding.AsNeeded);
			this.defaultUrlOptions.LanguageLocation = GetLanguageLocation(config["languageLocation"], LanguageLocation.FilePath);
			this.defaultUrlOptions.LowercaseUrls = MainUtil.GetBool(config["lowercaseUrls"], false);
			this.defaultUrlOptions.UseDisplayName = MainUtil.GetBool(config["useDisplayName"], false);
			this.defaultUrlOptions.ShortenUrls = MainUtil.GetBool(config["shortenUrls"], true);
			this.defaultUrlOptions.SiteResolving = MainUtil.GetBool(config["siteResolving"], true); // not in the default functionality, but very useful.
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// The resolve provider.
		/// </summary>
		/// <param name="item">
		/// The item to use for site/provider resolution.
		/// </param>
		/// <param name="options">
		/// The options.
		/// </param>
		/// <returns>
		/// The <see cref="LinkProvider"/>.
		/// </returns>
		private static LinkProvider ResolveProvider(Item item, UrlOptions options)
		{
			var siteName = string.Empty;

			if (options.Site != null)
			{
				siteName = options.Site.Name;
			}
			else
			{
				var site = FindMatchingSite(item.Paths.FullPath, item.Language.Name);

				if (site != null)
				{
					siteName = site.Name;
				}
			}

			var provider = LinkProviderResolver.Resolve(siteName);

			return provider;
		}

		/// <summary>
		/// Gets language embedding.
		/// </summary>
		/// <param name="configValue">The configured value.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The language embedding.</returns>
		private static LanguageEmbedding GetLanguageEmbedding(string configValue, LanguageEmbedding defaultValue)
		{
			if (string.IsNullOrEmpty(configValue))
			{
				return defaultValue;
			}

			switch (configValue.ToLowerInvariant())
			{
				case "always":
					return LanguageEmbedding.Always;
				case "asneeded":
					return LanguageEmbedding.AsNeeded;
				case "never":
					return LanguageEmbedding.Never;
				default:
					return defaultValue;
			}
		}

		/// <summary>
		/// Gets the language location.
		/// </summary>
		/// <param name="configValue">The configurative value.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The language location.</returns>
		private static LanguageLocation GetLanguageLocation(string configValue, LanguageLocation defaultValue)
		{
			if (string.IsNullOrEmpty(configValue))
			{
				return defaultValue;
			}

			switch (configValue.ToLowerInvariant())
			{
				case "filepath":
					return LanguageLocation.FilePath;
				case "querystring":
					return LanguageLocation.QueryString;
				default:
					return defaultValue;
			}
		}
		#endregion
	}
}
