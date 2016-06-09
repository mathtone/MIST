using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mathtone.MIST.Tests {

	public interface IChangeTracker {
		List<string> Changes { get; }
	}

	public interface IChangeCounter {
		int ChangeCount { get; }
	}

	public interface ITestNotifier : IChangeTracker, IChangeCounter {
		string Value1 { get; set; }
		int Value2 { get; set; }
		int Value3 { get; set; }
		int All { get; set; }
		int Supressed { get; set; }
	}


	public class TestNotifierBase : IChangeTracker, IChangeCounter {
		public int ChangeCount => Changes.Count;
		public List<string> Changes { get; } = new List<string>();

		[NotifyTarget]
		protected void OnPropertyChanged(string propertyName) {
			Changes.Add(propertyName);
		}
	}

	[Notifier]
	public class TestNotifier1 : TestNotifierBase, ITestNotifier {

		[Notify]
		public string Value1 { get; set; }

		[Notify]
		public int Value2 { get; set; }
		[Notify]
		public int Value3 { get; set; }

		[Notify("Value1", "Value2", "Value3")]
		public int All { get; set; }

		public int Supressed { get; set; }
	}

	[Notifier(NotificationMode.Implicit)]
	public class TestNotifier2 : TestNotifierBase, ITestNotifier {

		public string Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }

		[Notify("Value1", "Value2", "Value3")]
		public int All { get; set; }

		[SuppressNotify]
		public int Supressed { get; set; }
	}

	[Notifier(NotificationMode.Implicit)]
	public class TestNotifier3 : TestNotifierBase, ITestNotifier {
		string value1;
		int value2, value3;
		public string Value1 {
			get {
				return value1;
			}
			set {
				value1 = value;
			}
		}
		public int Value2 {
			get {
				return value2;
			}
			set {
				value2 = value;
			}
		}
		//this is the acid test
		public int Value3 {
			get {
				return value3;
			}
			set {
				if (value % 2 == 0) {
					return;
				}
				else {
					value3 = value;
					return;
				}
			}
		}

		[Notify("Value1", "Value2", "Value3")]
		public int All { get; set; }

		[SuppressNotify]
		public int Supressed { get; set; }
	}

	[Notifier]
	public class TestNotifier4 : TestNotifierBase {

		[Notify(NotificationStyle.OnChange)]
		public string SomeValue { get; set; }

		string someValue2;

		[SuppressNotify]
		public string SomeValue2 {
			get { return someValue2; }
			set {
				var curValue = SomeValue2;
				someValue2 = value;
				bool isEqual = value == curValue;
				if (!isEqual) {
					DoSomething("BERRRRRG");
				}
			}
		}

		void DoSomething(string value) {

		}
	}
	//[Notifier(NotificationMode.Implicit)]
	//public class TestNotifier4 : NotifierBase { 
	//	[Notify(null)]
	//	public int RaiseNull { get; set; }
	//}
}