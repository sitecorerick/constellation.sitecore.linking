namespace Constellation.Sitecore.Links
{
	using System.Configuration;

	/// <summary>
	/// The shared content link provider rule collection.
	/// </summary>
	[ConfigurationCollection(typeof(SharedContentLinkProviderRule), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class SharedContentLinkProviderRuleCollection : ConfigurationElementCollection
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
		public SharedContentLinkProviderRule this[int index]
		{
			get
			{
				return (SharedContentLinkProviderRule)BaseGet(index);
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
		public new SharedContentLinkProviderRule this[string key]
		{
			get
			{
				return (SharedContentLinkProviderRule)BaseGet(key);
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
			return new SharedContentLinkProviderRule();
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
			return ((SharedContentLinkProviderRule)element).TemplateName;
		}
	}
}
