namespace Constellation.Sitecore.Links
{
	using System;
	using System.Configuration;

	/// <summary>
	/// The switching link provider rule.
	/// </summary>
	public class SwitchingLinkProviderRule : ConfigurationElement
	{
		/// <summary>
		/// Gets or sets the site name for the LinkProviderRule.
		/// </summary>
		[ConfigurationProperty("site", IsKey = true, IsRequired = true)]
		public string Site
		{
			get { return (string)base["site"]; }
			set { base["site"] = value; }
		}

		/// <summary>
		/// Gets or sets the Link Provider Name. If this value is not empty, this value trumps
		/// the value of the ProviderType and uses LinkManager to load the provider. This is
		/// the preferred method of getting providers.
		/// </summary>
		[ConfigurationProperty("name")]
		public string Name
		{
			get { return (string)base["name"]; }
			set { base["name"] = value; }
		}

		/// <summary>
		/// Gets or sets the provider type.
		/// </summary>
		[ConfigurationProperty("providerType")]
		public string ProviderType
		{

			get { return (string)base["providerType"]; }
			set { base["providerType"] = value; }
		}

		/// <summary>
		/// Returns a Type object representing the providerType specified in the config.
		/// </summary>
		/// <returns>
		/// The <see cref="Type"/>.
		/// </returns>
		public Type GetProviderType()
		{
			return Type.GetType(this.ProviderType);
		}

	}
}
