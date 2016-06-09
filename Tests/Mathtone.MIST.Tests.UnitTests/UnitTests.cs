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

		//****************
		//NOTE: Clean and rebuild the solution when these tests are run otherwise notifications will be implemented twice and the tests will fail.
		//****************
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

			var notifiers = new ITestNotifier[] {
				new TestNotifier1(),
				new TestNotifier2(),
				new TestNotifier3(),

			};

			var expectedChanges = new[] { "Value1", "Value2", "Value3", "Value1", "Value2", "Value3" };

			foreach (var notifier in notifiers) {
				WriteLine($"Testing notifier: {notifier.GetType().Name}");
				notifier.Value1 = "ONE";
				notifier.Value2 = 1;
				notifier.Value3 = 2;
				notifier.Supressed = 3;
				notifier.All = 4;
				CollectionAssert.AreEqual(notifier.Changes,expectedChanges);
			}
		}


		[TestMethod]
		public void TestNotifierNotifierNotifier() {
			var notifier = new TestNotifier4();
			notifier.SomeNotifier = new TestNotifier1();
			notifier.SomeNotifier = notifier.SomeNotifier;
			Assert.AreEqual(1, notifier.ChangeCount);
		}

		[TestMethod]
		public void TestEqualityOverride() {
			var notifier = new TestNotifier5();
			notifier.SomeValue = new TestValue { Value = "A" };
			notifier.SomeValue = new TestValue { Value = "a" };
			Assert.AreEqual(1, notifier.ChangeCount);
		}

        [TestMethod]
        public void Explicit_notify_on_set_No_args()
        {
            var notifier = new TestNotifier.Explicit_NoArgsSpy();

            notifier.StringValue = "Value";
            notifier.StringValue = "Value";

            Assert.AreEqual(2, notifier.ChangeCount);
        }

        [TestMethod]
        public void Explicit_notify_on_set_One_arg()
        {
            var expectedProperties = new[] { "StringValue", "StringValue" };

            var notifier = new TestNotifier.Explicit_OneArgSpy();

            notifier.StringValue = "ONE";
            Assert.AreEqual("ONE", notifier.StringValue, "Value should change to ONE");

            notifier.StringValue = "ONE";
            CollectionAssert.AreEqual(expectedProperties, notifier.Changes, $"Expected '{string.Join(",", expectedProperties)}' but got {string.Join(",", notifier.Changes)}.");
        }

        [TestMethod]
        public void Implicit_notify_on_set()
        {
            var notifier = new TestNotifier.ImplicitSpy();

            notifier.StringValue = "Value";
            notifier.StringValue = "Value";

            Assert.AreEqual(2, notifier.ChangeCount);
        }

        [TestMethod]
        public void Explicit_notify_on_change()
        {
            var notifier = new TestNotifier.ExplicitOnChangeSpy();

            notifier.StringValue = "ONE";
            Assert.AreEqual(1, notifier.ChangeCount);

            notifier.StringValue = "ONE";
            Assert.AreEqual(1, notifier.ChangeCount);
        }

        [TestMethod]
        public void Implicit_notify_on_change()
        {
            var notifier = new TestNotifier.ImplicitOnChangeSpy();

            notifier.StringValue = "ONE";
            Assert.AreEqual(1, notifier.ChangeCount);

            notifier.StringValue = "ONE";
            Assert.AreEqual(1, notifier.ChangeCount);
        }

        [TestMethod]
        public void Explicit_notify_on_set_when_class_is_ImplicitOnChange()
        {
            var notifier = new TestNotifier.ImplicitOnChangeSpy();

            notifier.ExplicitOnSetString = "Value";
            notifier.ExplicitOnSetString = "Value";

            Assert.AreEqual(2, notifier.ChangeCount);
        }
    }
}