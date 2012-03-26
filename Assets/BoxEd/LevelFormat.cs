
namespace BoxEd
{
	/// <summary>
	/// Stores useful format-related information.
	/// </summary>
	public static class BoxFormat
	{
		/// <summary>
		/// The name of the scene used by the editor itself for levels.
		/// </summary>
		public const string LevelName = "BEDScene";

		/// <summary>
		/// The root node for the Box format.
		/// </summary>
		public const string RootNode = "BoxLevel";

		/// <summary>
		/// The node containing metadata for the Box format.
		/// </summary>
		public const string MetaNode = "Meta";

		/// <summary>
		/// The node containing the data for the Box format.
		/// </summary>
		public const string DataNode = "Data";
	}
}