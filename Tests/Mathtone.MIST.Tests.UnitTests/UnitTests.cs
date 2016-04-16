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
		public void TestNotifier() {
			WriteLine("Testing Notifier...");
			var notifier = new TestNotifier();
			var changedProps = new List<string>();

			PropertyChangedEventHandler handler = (a, b) => {
				changedProps.Add(b.PropertyName);
			};

			notifier.PropertyChanged += handler;
			notifier.Nested.PropertyChanged += handler;

			notifier.Change1 = "";
			notifier.Change2 = "";
			notifier.ChangeAll = "";
			notifier.ChangeNone = "";
			notifier.Nested.Change1 = 1;
			notifier.Nested.Change2 = 2;

			Assert.IsTrue(changedProps.SequenceEqual(
				new[] {
					"Change1",
					"Change2",
					"Change1",
					"Change2",
					"ReadOnly",
					"-Change1",
					"-Change2"
				}
			));

			notifier.PropertyChanged -= handler;
			notifier.Nested.PropertyChanged -= handler;
		}
	}
}