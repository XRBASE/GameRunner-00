using System;

namespace Base.Ravel.TranslateAttributes
{
	/// <summary>
	/// Attributes which can be applied in transform based components.
	/// </summary>
	[Flags]
	public enum TransformAttribute
	{
		None = 0,
		Position = 1,
		Rotation = 2,
		Scale = 4
	}

	public enum TransformSpace
	{
		World, // global application
		Self, // local application
	}
}