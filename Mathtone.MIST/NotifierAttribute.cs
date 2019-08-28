using System;

namespace Mathtone.MIST {

	/// <summary>
	/// Used to mark a class for automatic notification implementation;
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class NotifierAttribute : Attribute {
		public NotificationMode NotificationMode { get; protected set; }
		public NotificationStyle DefaultStyle { get; protected set; }

		public NotifierAttribute(NotificationMode mode = NotificationMode.Explicit, NotificationStyle defaultStyle = NotificationStyle.OnSet) {
			this.NotificationMode = mode;
			this.DefaultStyle = defaultStyle;
		}
	}
}