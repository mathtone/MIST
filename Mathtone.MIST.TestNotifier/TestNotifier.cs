namespace Mathtone.MIST.Tests {


	[Notifier]
	public class TestNotifier : NotifierBase {

		[Notify]
		public string SomeProperty { get; set; }

		[Notify("SomeProperty", "AllProperties")]
		public string AllProperties { get; set; }
	}

	[Notifier(NotificationMode.Implicit)]
	public class TestNotifier2 : NotifierBase {

		public string SomeProperty { get; set; }

		[Notify("SomeProperty", "AllProperties")]
		public string AllProperties { get; set; }
	}
}