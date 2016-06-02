using System;
using System.ComponentModel;

namespace Mathtone.MIST.Tests {

	public interface ITestNotifier : INotifyPropertyChanged {
		string Value1 { get; set; }
		int Value2 { get; set; }
		int Value3 { get; set; }
		int All { get; set; }
		int Supressed { get; set; }
	}

	[Notifier]
	public class TestNotifier1 : NotifierBase, ITestNotifier {

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
	public class TestNotifier2 : NotifierBase, ITestNotifier {

		public string Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }

		[Notify("Value1", "Value2", "Value3")]
		public int All { get; set; }

		[SuppressNotify]
		public int Supressed { get; set; }
	}

	[Notifier(NotificationMode.Implicit)]
	public class TestNotifier3 : NotifierBase, ITestNotifier {
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
}