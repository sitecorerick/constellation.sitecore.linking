namespace Constellation.Sitecore.Links
{
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
			var rule = LinkProviderSwitching.Instance.LinkProviderRules[site];
			var providerType = LinkProviderSwitching.Instance.DefaultLinkProviderType;

			if (rule != null)
			{
				providerType = rule.LinkProviderType;
			}

			return (LinkProvider)(Activator.CreateInstance(providerType)); // We want to force an exception for incompatible types.
		}
	}
}
