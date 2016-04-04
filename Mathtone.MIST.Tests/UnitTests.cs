using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mathtone.MIST.Tests {
	[TestClass]
	public class UnitTests {

		string ApplicationPath {
			get {
				return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			}
		}

		[TestMethod]
		public void TestNotificationWeaver() {
			var weaver = new NotificationWeaver(ApplicationPath + "\\Mathtone.MIST.TestNotifier.dll");
			weaver.Weave(true);
			LoadAndTestNotifier1();
			LoadAndTestNotifier2();
		}

		void LoadAndTestNotifier1() {
			var notifier = new TestNotifier();
			var changedProps = new List<string>();

			notifier.PropertyChanged += (a, b) => {
				changedProps.Add(b.PropertyName);
			};

			var change = new[] { "SomeProperty"};
			notifier.SomeProperty = "CHANGE1";
			Assert.IsTrue(changedProps.Intersect(change).Count() == change.Length);
			changedProps.Clear();

			change = new[] { "SomeProperty", "AllProperties" };
			notifier.AllProperties = "CHANGE2";
			Assert.IsTrue(changedProps.Intersect(change).Count() == change.Length);
			changedProps.Clear();
			
		}

		void LoadAndTestNotifier2() {
			//var notifier = new TestNotifier2();
			//var changedProps = new List<string>();

			//notifier.PropertyChanged += (a, b) => {
			//	changedProps.Add(b.PropertyName);
			//};

			//var change = new[] { "SomeProperty" };
			//notifier.SomeProperty = "CHANGE1";
			//Assert.IsTrue(changedProps.Intersect(change).Count() == change.Length);
			//changedProps.Clear();

			//change = new[] { "SomeProperty", "AllProperties" };
			//notifier.AllProperties = "CHANGE2";
			//Assert.IsTrue(changedProps.Intersect(change).Count() == change.Length);
			//changedProps.Clear();

		}
	}
}