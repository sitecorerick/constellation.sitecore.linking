namespace Constellation.Sitecore
{
	using global::Sitecore;
	using System;

	public static class PathUtility
	{
		/// <summary>
		/// Returns the first "slug" of an Item Path, which represents
		/// a flag folder name indicating that the Item is a shared content item.
		/// </summary>
		/// <param name="itemPath">The path to parse.</param>
		/// <returns>The folder name to test for shared content.</returns>
		public static string GetFirstFolderFromPath(string itemPath)
		{
			return GetFirstFolderFromPath(itemPath, Context.Site.StartPath);
		}

		/// <summary>
		/// Returns the first "slug" of an Item Path, which represents
		/// a flag folder name indicating that the Item is a shared content item.
		/// </summary>
		/// <param name="itemPath">The path to parse.</param>
		/// <param name="startPath">The root path that should be removed before parsing.</param>
		/// <returns>The folder name to test for shared content.</returns>
		public static string GetFirstFolderFromPath(string itemPath, string startPath)
		{
			var path = MainUtil.DecodeName(itemPath).Replace(startPath, string.Empty).ToLower();

			if (path.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
			{
				path = path.Substring(1, path.Length - 1);
			}

			var firstSlash = path.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);

			return firstSlash > 0 ? path.Substring(0, firstSlash) : path;
		}
	}
}
