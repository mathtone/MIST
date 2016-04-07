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

		static string ApplicationPath {
			get {
				return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			}
		}
		static bool initialized;

		[TestInitialize]
		public void InitializeTest() {
			if (!initialized) {
				var weaver = new NotificationWeaver(ApplicationPath + "\\Mathtone.MIST.TestNotifier.dll");
				weaver.InsertNotifications(true);
				initialized = true;
			}
		}

		[TestMethod]
		public void TestExplicitNotifier() {
			TestNotifier(new ExplicitTestNotifier());
		}

		[TestMethod]
		public void TestImplicitNotifier() {
			TestNotifier(new ImplicitTestNotifier());
		}

		void TestNotifier(ITestNotifier notifier) {
			AssertNotificationTest(
				notifier,
				a => {
					a.Property1 = "A";
					a.Property2 = "B";
					a.Prop1And2 = "C";
					a.Supressed = "X";
				},
				a => a.SequenceEqual(new[] { "Property1", "Property2", "Prop1And2", "Property1", "Property2" })
			);
		}

		void AssertNotificationTest<T>(T notifier, Action<T> actions, Func<IEnumerable<string>, bool> verifier) where T : ITestNotifier {
			Assert.IsTrue(ExecuteNotificationTest(notifier, actions, verifier));
		}

		bool ExecuteNotificationTest<T>(T notifier, Action<T> actions, Func<IEnumerable<string>, bool> verifier) where T : ITestNotifier {
			var changed = new List<string>();
			PropertyChangedEventHandler handler = (sender, args) => {
				changed.Add(args.PropertyName);
			};
			notifier.PropertyChanged += handler;
			actions(notifier);
			notifier.PropertyChanged -= handler;
			var rtn = verifier(changed);
			return rtn;
		}
	}
}