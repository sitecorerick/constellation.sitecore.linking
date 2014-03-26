namespace Constellation.Sitecore.Links
{
	using global::Sitecore;
	using global::Sitecore.Configuration;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.Links;
	using global::Sitecore.Resources.Media;
	using global::Sitecore.Web;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;
	using System.Web;

	/// <summary>
	/// Used by LinkManager to resolve item URLs.
	/// </summary>
	/// <remarks>
	/// To use this class, change the Sitecore/LinkManager/Providers section of the web.config.
	/// See http://reasoncodeexample.com/2012/08/09/sitecore-cross-site-links/ for some of the functionality
	/// included here.
	/// </remarks>
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
	public class LinkProvider : global::Sitecore.Links.LinkProvider
	{
		#region Properties
		/// <summary>
		/// The URL options.
		/// </summary>
		private readonly UrlOptions defaultUrlOptions;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="LinkProvider"/> class.
		/// </summary>
		public LinkProvider()
		{
			this.defaultUrlOptions = new UrlOptions();
		}
		#endregion

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
			string url = null;
			Assert.ArgumentNotNull(item, "item");
			Assert.ArgumentNotNull(options, "options");

			if (Context.Site != null && (Context.PageMode.IsPreview || Context.PageMode.IsPageEditor))
			{
				var dynamicOptions = new LinkUrlOptions { Site = Context.Site.Name, Language = item.Language };

				var sharedContentSetting = SharedContentConfiguration.Instance.SharedContentFolders[item.TemplateName];
				if (sharedContentSetting != null)
				{
					// Make sure we load Shared Content Items for editing in the context of their parent site
					var siteName = PathUtility.GetFirstFolderFromPath(item.Paths.FullPath, sharedContentSetting.PathToSiteFolder);

					if (!string.IsNullOrEmpty(siteName))
					{
						dynamicOptions.Site = siteName;
					}
				}

				url = this.GetDynamicUrl(item, dynamicOptions);
			}

			if (url == null)
			{
				var builder = new SharedContentLinkBuilder(options);
				url = builder.GetItemUrl(item);
				if (this.LowercaseUrls)
				{
					url = url.ToLowerInvariant();
				}
			}

			return url;
		}

		/// <summary>
		/// Gets the dynamic URL for an item.
		/// </summary>
		/// <param name="item">The item to create an URL to.</param>
		/// <param name="options">The options.</param>
		/// <returns>The dynamic URL.</returns>
		public override string GetDynamicUrl(Item item, LinkUrlOptions options)
		{
			Assert.ArgumentNotNull(item, "item");
			Assert.ArgumentNotNull(options, "options");
			var builder = new StringBuilder("http://");

			// Get the hostname for the site
			var siteInfo = Factory.GetSiteInfo(options.Site);
			builder.Append(siteInfo.TargetHostName);

			var queryStringSeparator = HttpUtility.HtmlDecode(Settings.Links.QueryStringSeparator);
			builder.Append("/~/link.aspx?_id" + '=' + item.ID.ToShortID());
			if (options.Language != null)
			{
				builder.Append(queryStringSeparator + "_lang" + "=" + options.Language.Name);
			}

			if (!string.IsNullOrEmpty(options.Site))
			{
				builder.Append(queryStringSeparator + "_site" + "=" + options.Site);
			}

			builder.Append(queryStringSeparator + "_z=z");

			return builder.ToString();
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
			this.defaultUrlOptions.LanguageEmbedding = this.GetLanguageEmbedding(config["languageEmbedding"], LanguageEmbedding.AsNeeded);
			this.defaultUrlOptions.LanguageLocation = this.GetLanguageLocation(config["languageLocation"], LanguageLocation.FilePath);
			this.defaultUrlOptions.LowercaseUrls = MainUtil.GetBool(config["lowercaseUrls"], false);
			this.defaultUrlOptions.UseDisplayName = MainUtil.GetBool(config["useDisplayName"], false);
			this.defaultUrlOptions.ShortenUrls = MainUtil.GetBool(config["shortenUrls"], true);
			this.defaultUrlOptions.SiteResolving = MainUtil.GetBool(config["siteResolving"], true); // not in the default functionality, but very useful.
		}

		/// <summary>
		/// Determines whether the specified link text represents a dynamic link.
		/// </summary>
		/// <param name="linkText">The link text.</param>
		/// <returns><c>true</c> if [is dynamic link] [the specified link text]; otherwise, <c>false</c>.</returns>
		public override bool IsDynamicLink(string linkText)
		{
			Assert.ArgumentNotNull(linkText, "linkText");
			if (linkText.Length == 0)
			{
				return false;
			}

			if (linkText.Contains("~/link.aspx?"))
			{
				return true;
			}

			foreach (string str in MediaManager.Provider.Config.MediaPrefixes)
			{
				if (linkText.Contains(str))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Parses a dynamic link.
		/// </summary>
		/// <param name="linkText">The link text.</param>
		/// <returns>The dynamic link.</returns>
		public override DynamicLink ParseDynamicLink(string linkText)
		{
			Assert.ArgumentNotNullOrEmpty(linkText, "linkText");
			var dynamicLink = new DynamicLink();
			dynamicLink.Initialize(linkText);
			return dynamicLink;
		}

		/// <summary>
		/// Parses a request URL.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>The request URL.</returns>
		public override RequestUrl ParseRequestUrl(HttpRequest request)
		{
			Assert.ArgumentNotNull(request, "request");
			return new RequestUrl(request);
		}

		/// <summary>
		/// Gets language embedding.
		/// </summary>
		/// <param name="configValue">The configured value.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The language embedding.</returns>
		private LanguageEmbedding GetLanguageEmbedding(string configValue, LanguageEmbedding defaultValue)
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
		private LanguageLocation GetLanguageLocation(string configValue, LanguageLocation defaultValue)
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
	}
}
