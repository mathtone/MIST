using System;

namespace Mathtone.MIST {
	/// <summary>
	/// Used to mark a property as a notification provider.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class NotifyAttribute : Attribute {
		public NotificationStyle Style { get; protected set; }

		public string[] NotificationSource { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NotifyAttribute"/> class.
		/// </summary>
		/// <param name="sourceNames">Properties that will be passed to the cotification target method.</param>
		public NotifyAttribute(params string[] sourceNames) {
			Style = NotificationStyle.OnSet;
			NotificationSource = sourceNames;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NotifyAttribute"/> class.
		/// </summary>
		/// <param name="sourceNames">Properties that will be passed to the cotification target method.</param>
		public NotifyAttribute(NotificationStyle style, params string[] sourceNames) {
			Style = style;
			NotificationSource = sourceNames;
		}
	}
}