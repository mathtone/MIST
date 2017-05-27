using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathtone.MIST.Tests.NugetScriptTest {

	[Notifier]
	public class TestClass {

		[Notify(NotificationStyle.OnChange)]
		public string TestString { get; set; }

		[Notify]
		public int TestInt { get; set; }

		[NotifyTarget]
		void Changed(string propertyName) {
			;
		}

		static void Main(string[] args) {
			try {
				var testClass = new TestClass();
				testClass.TestString = "1";
				testClass.TestInt = 1;
			}
			catch(Exception) {
				;
			}
		}
	}
}