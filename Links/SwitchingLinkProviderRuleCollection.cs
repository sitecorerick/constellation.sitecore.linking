namespace Constellation.Sitecore.Links
{
	using System.Configuration;

	[ConfigurationCollection(typeof(SwitchingLinkProviderRule), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class SwitchingLinkProviderRuleCollection : ConfigurationElementCollection
	{
		/// <summary>
		/// The this.
		/// </summary>
		/// <param name="index">
		/// The index.
		/// </param>
		/// <returns>
		/// The <see cref="SwitchingLinkProviderRule"/>.
		/// </returns>
		public SwitchingLinkProviderRule this[int index]
		{
			get
			{
				return (SwitchingLinkProviderRule)BaseGet(index);
			}

			set
			{
				if (this.BaseGet(index) != null)
				{
					this.BaseRemoveAt(index);
				}

				this.BaseAdd(index, value);
			}
		}

		/// <summary>
		/// The this.
		/// </summary>
		/// <param name="key">
		/// The key.
		/// </param>
		/// <returns>
		/// The <see cref="FolderConfigurationElement"/>.
		/// </returns>
		public new SwitchingLinkProviderRule this[string key]
		{
			get
			{
				return (SwitchingLinkProviderRule)BaseGet(key);
			}
		}

		/// <summary>
		/// The create new element.
		/// </summary>
		/// <returns>
		/// The <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new SwitchingLinkProviderRule();
		}

		/// <summary>
		/// The get element key.
		/// </summary>
		/// <param name="element">
		/// The element.
		/// </param>
		/// <returns>
		/// The <see cref="object"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((SwitchingLinkProviderRule)element).Site;
		}
	}
}
