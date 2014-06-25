namespace Constellation.Sitecore.Links
{
	using System;
	using System.Configuration;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// The switching link provider configuration.
	/// </summary>
	public class SwitchingLinkProviderConfiguration : ConfigurationSection
	{
		/// <summary>
		/// The settings.
		/// </summary>
		private static SwitchingLinkProviderConfiguration settings;

		/// <summary>
		/// Gets the settings.
		/// </summary>
		public static SwitchingLinkProviderConfiguration Settings
		{
			get
			{
				if (settings == null)
				{
					var config = ConfigurationManager.GetSection("switchingLinkProvider");
					settings = config as SwitchingLinkProviderConfiguration;
				}

				return settings;
			}
		}

		/// <summary>
		/// Gets or sets the provider type.
		/// </summary>
		[ConfigurationProperty("defaultLinkProviderType", IsRequired = true)]
		public string DefaultLinkProviderType
		{

			get { return (string)base["defaultLinkProviderType"]; }
			set { base["defaultLinkProviderType"] = value; }
		}

		/// <summary>
		/// Gets the rules specifying which LinkProvider to use for a given Sitecore site definition.
		/// across all sites.
		/// </summary>
		[ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
		public SwitchingLinkProviderRuleCollection Rules
		{
			[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1122:UseStringEmptyForEmptyStrings", Justification = "Reviewed. Suppression is OK here.")]
			get
			{
				return (SwitchingLinkProviderRuleCollection)base[""];
			}
		}

		/// <summary>
		/// The get default link provider type.
		/// </summary>
		/// <returns>
		/// The <see cref="Type"/>.
		/// </returns>
		public Type GetDefaultLinkProviderType()
		{
			return Type.GetType(this.DefaultLinkProviderType);
		}
	}
}
