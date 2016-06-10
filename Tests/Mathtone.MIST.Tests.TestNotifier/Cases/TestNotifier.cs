using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mathtone.MIST.TestNotifier.Cases {

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


	//Yodawg, I heard you liked notifiers, 
	[Notifier(NotificationMode.Explicit)]
	public class TestNotifier4 : TestNotifierBase {

		[Notify(NotificationStyle.OnChange)]
		public TestNotifier1 SomeNotifier { get; set; }

	}

	[Notifier(NotificationMode.Implicit, NotificationStyle.OnChange)]
	public class TestNotifier5 : TestNotifierBase {

		public TestValue SomeValue { get; set; }

	}

	[Notifier]
	public class ValueTypeTestNotifier : TestNotifierBase {

		[Notify(NotificationStyle.OnSet)]
		public string TestString { get; set; }

		[Notify(NotificationStyle.OnChange)]
		public int TestInt { get; set; }
	}

	public class TestValue {

		public string Value { get; set; }

		public override bool Equals(object obj) {
			var value = obj as TestValue;
			return value?.Value.ToLower() == this.Value.ToLower();
		}
	}
}