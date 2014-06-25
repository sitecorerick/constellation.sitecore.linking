namespace Constellation.Sitecore.Links
{
	using System.Configuration;
	using System.Diagnostics.CodeAnalysis;

	public class SharedContentLinkProviderConfiguration : ConfigurationSection
	{
		/// <summary>
		/// The settings.
		/// </summary>
		private static SharedContentLinkProviderConfiguration settings;

		/// <summary>
		/// Gets the settings.
		/// </summary>
		public static SharedContentLinkProviderConfiguration Settings
		{
			get
			{
				if (settings == null)
				{
					var config = ConfigurationManager.GetSection("sharedContentLinkProvider");
					settings = config as SharedContentLinkProviderConfiguration;
				}

				return settings;
			}
		}

		/// <summary>
		/// Gets the rules used to locate and link to shared content items
		/// across all sites.
		/// </summary>
		[ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
		public SharedContentLinkProviderRuleCollection Rules
		{
			[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1122:UseStringEmptyForEmptyStrings", Justification = "Reviewed. Suppression is OK here.")]
			get
			{
				return (SharedContentLinkProviderRuleCollection)base[""];
			}
		}
	}
}
