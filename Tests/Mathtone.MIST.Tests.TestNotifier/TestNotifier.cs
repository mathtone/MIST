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
}