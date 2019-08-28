using System;

namespace Mathtone.MIST {
	/// <summary>
	/// Used to identify the notification target method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class NotifyTarget : Attribute {

	}
}