using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.Console;

namespace Mathtone.MIST.Tests {
	[TestClass]
	public class UnitTests {

		static bool initialized;

		static string ApplicationPath {
			get {
				return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			}
		}

		[TestInitialize]
		public void InitializeTest() {
			if (!initialized) {
				var weaver = new NotificationWeaver(ApplicationPath + "\\Mathtone.MIST.TestNotifier.dll");
				weaver.InsertNotifications(true);
				initialized = true;
			}
		}

		[TestMethod]
		public void TestNotificationImplementation() {

			var changedProps = new List<string>();
			var notifiers = new ITestNotifier[] {
				new TestNotifier1(),
				new TestNotifier2()
			};

			var expectedChanges = new[] { "Value1", "Value2", "Value3", "Value1", "Value2", "Value3" };

			PropertyChangedEventHandler handler = (a, b) => {
				changedProps.Add(b.PropertyName);
			};

			foreach (var notifier in notifiers) {
				WriteLine($"Testing notifier: {notifier.GetType().Name}");
				changedProps.Clear();
				notifier.PropertyChanged += handler;
				notifier.Value1 = "ONE";
				notifier.Value2 = 1;
				notifier.Value3 = 2;
				notifier.Supressed = 1;
				notifier.All = 1;
				notifier.PropertyChanged -= handler;
				Assert.IsTrue(changedProps.SequenceEqual(expectedChanges));
			}
		}
	}
}