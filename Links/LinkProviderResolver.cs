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
			var providerType = SwitchingLinkProviderConfiguration.Settings.GetDefaultLinkProviderType();

			if (rule != null)
			{
				providerType = rule.GetProviderType();
			}

			return Activator.CreateInstance(providerType) as LinkProvider; // We want to force an exception for incompatible types.
		}
	}
}
