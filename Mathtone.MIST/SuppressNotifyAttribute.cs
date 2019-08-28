using System;

namespace Mathtone.MIST {
	/// <summary>
	/// Used on public properties in implicit notification scenarios for which notificaiton should NOT be implemented.
	/// </summary>
	/// <seealso cref="System.Attribute" />
	[AttributeUsage(AttributeTargets.Property)]
	public class SuppressNotifyAttribute : Attribute {

	}
}
