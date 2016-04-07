using System.ComponentModel;

namespace Mathtone.MIST.Tests {

	public interface ITestNotifier : INotifyPropertyChanged {
		string Property1 { get; set; }
		string Property2 { get; set; }
		string Property3 { get; set; }
		string Supressed { get; set; }
		string Prop1And2 { get; set; }
	}

	[Notifier(NotificationMode.Implicit)]
	public class ImplicitTestNotifier : NotifierBase, ITestNotifier {

		public string Property1 { get; set; }

		public string Property2 { get; set; }

		public string Property3 { get; set; }

		[IgnoreNotify]
		public string Supressed { get; set; }

		[Notify("Prop1And2", "Property1", "Property2")]
		public string Prop1And2 { get; set; }
	}

	[Notifier]
	public class ExplicitTestNotifier : NotifierBase, ITestNotifier {

		[Notify]
		public string Property1 { get; set; }

		[Notify]
		public string Property2 { get; set; }

		[Notify]
		public string Property3 { get; set; }

		public string Supressed { get; set; }

		[Notify("Prop1And2", "Property1", "Property2")]
		public string Prop1And2 { get; set; }
	}


	//[Notifier(NotificationMode.Implicit)]
	//public class TestNotifier3 : NotifierBase {

	//	public string Property1 { get; set; }

	//	public string Property2 { get { return Property1; } }

	//	int property3;
	//	public int Property3 {
	//		get { return property3; }
	//		set { property3 = value; }
	//	}

	//	[IgnoreNotify]
	//	public bool Property4 { get; set; }
	//}
}