namespace Mathtone.MIST {

	/// <summary>
	/// Use implicit or explicit implementation of notification;
	/// </summary>
	public enum NotificationMode {

		/// <summary>
		/// Properties must be marked with the <see cref="NotifyAttribute"/>
		/// </summary>
		Explicit,

		/// <summary>
		/// Notification will be implemented for all public properties declared within the class;
		/// </summary>
		Implicit,

		///// <summary>
		///// Like Implicit, but only if the propertys backing variable changed value.
		///// </summary>
		//ImplicitOnChange
	}

	/// <summary>
	/// Modes of the Notify attribute
	/// </summary>
	public enum NotificationStyle {

		/// <summary>
		/// Notifications are called each time the setter is called.
		/// </summary>
		OnSet,

		/// <summary>
		/// Notifications are only called if the propertys backing variable is changed.
		/// </summary>
		OnChange,
	}
}