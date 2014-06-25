namespace Constellation.Sitecore.Links
{
	using global::Sitecore;
	using global::Sitecore.Configuration;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.IO;
	using global::Sitecore.Resources;
	using global::Sitecore.Resources.Media;
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Replaces the Sitecore Media (link) Provider. Adds support for a dedicated
	/// media site URL. To use, add a new property to your site definitions named
	/// "mediahostname" supply it with the host name value for all media requests.
	/// </summary>
	/// <remarks>
	/// <para>
	/// In order to use this solution one of two things must be true:
	/// 1. All media hostnames for a given Sitecore solution should be pointing at the
	/// same site definition, with a wildcard hostname of "media.*"
	/// 2. All media hostnames for a given Sitecore solution should have their own unique
	/// site definition with unique hostname and targethostname values.
	/// </para>
	/// <para>
	/// To enable this MediaProvider, change the type attribute in 
	/// /configuration/sitecore/mediaLibrary/mediaProvider
	/// in the web config.
	/// </para>
	/// </remarks>
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
	public class MediaLinkProvider : MediaProvider
	{
		/// <summary>
		/// Gets a media URL.
		/// </summary>
		/// <param name="item">The media item.</param><param name="options">The query string.</param>
		/// <returns>
		/// The media URL.
		/// </returns>
		public override string GetMediaUrl(MediaItem item, MediaUrlOptions options)
		{
			Assert.ArgumentNotNull(item, "item");
			Assert.ArgumentNotNull(options, "options");

			if (!options.Thumbnail && !this.HasMediaContent(item))
			{
				if (item.InnerItem["path"].Length > 0)
				{
					return item.InnerItem["path"];
				}

				if (options.UseDefaultIcon)
				{
					return Themes.MapTheme(Settings.DefaultIcon);
				}
			}

			Assert.IsTrue(this.Config.MediaPrefixes[0].Length > 0, "media prefixes are not configured properly.");

			var rootPath = this.MediaLinkPrefix;

			if (rootPath.StartsWith("/", StringComparison.InvariantCulture))
			{
				rootPath = StringUtil.Mid(rootPath, 1);
			}

			if (options.AlwaysIncludeServerUrl && Context.PageMode.IsNormal)
			{
				var hostname = this.GetSiteSpecificMediaHostname();

				// prefix = FileUtil.MakePath(WebUtil.GetServerUrl(), prefix, '/'); - Original from Sitecore
				rootPath = FileUtil.MakePath("http://" + hostname, rootPath, '/');
			}

			var extension = StringUtil.EnsurePrefix('.', StringUtil.GetString(options.RequestExtension, item.Extension, "ashx"));
			var querystring = options.ToString();
			if (querystring.Length > 0)
			{
				extension = extension + "?" + querystring;
			}

			const string MediaLibraryPath = "/sitecore/media library/";
			var itempath = item.InnerItem.Paths.Path;
			string path;
			if (!options.UseItemPath || !itempath.StartsWith(MediaLibraryPath, StringComparison.OrdinalIgnoreCase))
			{
				path = item.ID.ToShortID().ToString();
			}
			else
			{
				path = StringUtil.Mid(itempath, MediaLibraryPath.Length);
			}

			return rootPath + path + (options.IncludeExtension ? extension : string.Empty);
		}

		/// <summary>
		/// Returns either the default target host name value for the "media"
		/// site entry or returns a customized hostname based upon the media host name
		/// of the Context Site's properties.
		/// </summary>
		/// <returns>
		/// The <see cref="string"/>.
		/// </returns>
		private string GetSiteSpecificMediaHostname()
		{
			var hostname = string.Empty;
			var currentSite = Context.Site;

			if (currentSite != null)
			{
				var mediahostname = currentSite.Properties["mediaHostName"];

				if (!string.IsNullOrEmpty(mediahostname))
				{
					return mediahostname;
				}

				hostname = currentSite.TargetHostName;
			}

			var mediaSiteContext = Factory.GetSite("media");

			if (mediaSiteContext == null)
			{
				Log.Warn("There is no site definition for \"media\".", this);
			}
			else
			{
				hostname = mediaSiteContext.TargetHostName;
			}

			return hostname;
		}
	}
}