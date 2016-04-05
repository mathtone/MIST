using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.Console;

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
			LoadAndTestNotifier3();
		}

		[TestMethod]
		public void ExecuteBuildTest() {
			//C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
			WriteLine("Executing build test");

		}

		void LoadAndTestNotifier1() {
			WriteLine("Testing Notifier 1");
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
			WriteLine("Testing Notifier 2");
			var notifier = new TestNotifier2();
			var changedProps = new List<string>();

			notifier.PropertyChanged += (a, b) => {
				changedProps.Add(b.PropertyName);
			};

			var change = new[] { "SomeProperty" };
			notifier.SomeProperty = "CHANGE1";
			Assert.IsTrue(changedProps.Intersect(change).Count() == change.Length);
			changedProps.Clear();

			change = new[] { "SomeProperty", "AllProperties" };
			notifier.AllProperties = "CHANGE2";
			Assert.IsTrue(changedProps.Intersect(change).Count() == change.Length);
			changedProps.Clear();

		}

		void LoadAndTestNotifier3() {
			WriteLine("Testing Notifier 3");
			var notifier = new TestNotifier3();
			var changedProps = new List<string>();

			notifier.PropertyChanged += (a, b) => {
				changedProps.Add(b.PropertyName);
			};

			var change = new[] { "Property1" };
			notifier.Property1= "CHANGE1";
			Assert.IsTrue(changedProps.Intersect(change).Count() == change.Length);
			changedProps.Clear();

			change = new[] { "Property3" };
			notifier.Property3 = 3;
			Assert.IsTrue(changedProps.Intersect(change).Count() == change.Length);
			changedProps.Clear();
		}
	}
}