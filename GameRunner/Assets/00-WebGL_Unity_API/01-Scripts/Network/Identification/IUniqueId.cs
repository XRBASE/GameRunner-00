/// <summary>
/// Should be derived from MonoBehaviour to work.
/// </summary>
public interface IUniqueId
{
	/// <summary>
	/// Identifying index, should always be -1 when not set. System will automatically assign indices, making sure all
	/// objects indices are unique.
	/// </summary>
	public int Identifier { get; set; }
	public string Name { get; }
}
