namespace Mathtone.MIST.Tests {

	[Notifier(NotificationMode.Implicit)]
	public partial class TestNotifier {

	}

	public partial class TestNotifier : NotifierBase {

		[Notifier(NotificationMode.Explicit)]
		public class NestedClass : NotifierBase {

			int change2;

			[Notify]
			public int Change1 { get; set; }

			[Notify]
			public int Change2 {
				get { return change2; }
				set { change2 = value + 1; }
			}

			[NotifyTarget]
			void Notify(string propertyName) {
				base.RaisePropertyChanged("-" + propertyName);
			}
		}

		public string Change1 { get; set; }

		public string Change2 { get; set; }

		public string ReadOnly => Change1 + Change2;

		public NestedClass Nested { get; } = new NestedClass();

		[SuppressNotify]
		public string ChangeNone { get; set; }

		[Notify("Change1", "Change2", "ReadOnly")]
		public string ChangeAll { get; set; }

		[Notify(null)]
		public string ChangeNull { get; set; }
	}
}