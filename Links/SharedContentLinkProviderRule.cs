namespace Constellation.Sitecore.Links
{
	using System.Configuration;

	/// <summary>
	/// The shared content link provider rule.
	/// </summary>
	public class SharedContentLinkProviderRule : ConfigurationElement
	{
		/// <summary>
		/// Gets or sets the template name.
		/// </summary>
		[ConfigurationProperty("templateName", IsKey = true, IsRequired = true)]
		public string TemplateName
		{
			get { return (string)base["templateName"]; }
			set { base["templateName"] = value; }
		}

		/// <summary>
		/// Gets or sets the root path.
		/// </summary>
		[ConfigurationProperty("rootPath", IsRequired = true)]
		public string RootPath
		{
			get { return (string)base["rootPath"]; }
			set { base["rootPath"] = value; }
		}

		/// <summary>
		/// Gets or sets the folder name.
		/// </summary>
		[ConfigurationProperty("folderName", IsRequired = true)]
		public string FolderName
		{
			get { return (string)base["folderName"]; }
			set { base["folderName"] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether categorized by site folder.
		/// </summary>
		[ConfigurationProperty("categorizedBySiteFolder")]
		public bool CategorizedBySiteFolder
		{
			get { return (bool)base["categorizedBySiteFolder"]; }
			set { base["categorizedBySiteFolder"] = value; }
		}

		/// <summary>
		/// Gets or sets the path to site folder.
		/// </summary>
		[ConfigurationProperty("pathToSiteFolder")]
		public string PathToSiteFolder
		{
			get { return (string)base["pathToSiteFolder"]; }
			set { base["pathToSiteFolder"] = value; }
		}

		/// <summary>
		/// Gets or sets the XPATH statement to query for the parent page of a shared content item.
		/// </summary>
		[ConfigurationProperty("parentPageQuery")]
		public string ParentPageQuery
		{
			get { return (string)base["parentPageQuery"]; }
			set { base["parentPageQuery"] = value; }
		}
	}
}
