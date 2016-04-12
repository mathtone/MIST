namespace Mathtone.MIST.Tests {

	[Notifier(NotificationMode.Implicit)]
	public class TestNotifier : NotifierBase {

		[Notifier(NotificationMode.Implicit)]
		public class NestedClass : NotifierBase {
			public int Change1 { get; set; }

			public int Change2 { get; set; }
		}

		public string Change1 { get; set; }

		public string Change2 { get; set; }

		public string ReadOnly => Change1 + Change2;

		public NestedClass Nested { get; } = new NestedClass();

		[SuppressNotify]
		public string ChangeNone { get; set; }

		[Notify("Change1","Change2","ReadOnly")]
		public string ChangeAll { get; set; }

	}
}