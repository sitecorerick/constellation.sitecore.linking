namespace Constellation.Sitecore.Links
{
	using global::Sitecore.Links;
	using System;

	/// <summary>
	/// Reads the configuration file and determines the best-match LinkProvider.
	/// </summary>
	public static class LinkProviderResolver
	{
		/// <summary>
		/// Resolves the correct link provider based on the system configuration.
		/// </summary>
		/// <param name="site">
		/// The site.
		/// </param>
		/// <returns>
		/// The <see cref="LinkProvider"/>.
		/// </returns>
		public static LinkProvider Resolve(string site)
		{
			var rule = SwitchingLinkProviderConfiguration.Settings.Rules[site];
			var providerName = SwitchingLinkProviderConfiguration.Settings.DefaultLinkProviderName;
			var providerType = SwitchingLinkProviderConfiguration.Settings.GetDefaultLinkProviderType();

			if (rule != null)
			{
				providerName = rule.Name;

				if (!string.IsNullOrEmpty(providerName))
				{
					return LinkManager.Providers[providerName];
				}

				// This is the old style, which is included for backwards compatibility.
				providerType = rule.GetProviderType();

				if (providerType != null)
				{
					return Activator.CreateInstance(providerType) as LinkProvider;
				}
			}

			if (!string.IsNullOrEmpty(providerName))
			{
				return LinkManager.Providers[providerName];
			}

			// This is the old style, which is provided for backwards compatibility.
			if (providerType != null)
			{
				return Activator.CreateInstance(providerType) as LinkProvider;
			}

			return LinkManager.Providers["sitecore"];
		}
	}
}
