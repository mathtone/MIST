using Mathtone.MIST.TestNotifier;
using Mathtone.MIST.TestNotifier.Cases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
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

		#region Tests Setup

		DefaultAssemblyResolver assemblyResolver;
		MetadataResolver metadataResolver;
		TypeDefinition[] standardModuleTypes;

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

			this.assemblyResolver = new DefaultAssemblyResolver();
			this.assemblyResolver.AddSearchDirectory(ApplicationPath);
			this.metadataResolver = new MetadataResolver(assemblyResolver);


		}
		void InitializeStandardAssembly() {
			var assemblyDef = null as AssemblyDefinition;
			var assembly = typeof(TestNotifier.Patterns.OnChange_Manually).Module.Assembly;

			//Load the assembly.
			using (var stream = File.OpenRead(assembly.Location)) {
				assemblyDef = AssemblyDefinition.ReadAssembly(stream);
			}

			standardModuleTypes = assemblyDef.Modules.SelectMany(a => a.Types).ToArray();
		}

		#endregion Tests Setup

		#region Support for all types of parameter types

		[TestMethod]
		public void OnSet_for_ValueTypes() {
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnSetInt = 43;
			notifier.OnSetInt = 43;
			notifier.OnSetBool = true;
			notifier.OnSetBool = true;
			notifier.OnSetDouble = 3.14159261;
			notifier.OnSetDouble = 3.14159261;
			Assert.AreEqual(6, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_ValueTypes() {
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnChangeInt = 43;
			notifier.OnChangeInt = 43;
			notifier.OnChangeBool = true;
			notifier.OnChangeBool = true;
			notifier.OnChangeDouble = 3.14159261;
			notifier.OnChangeDouble = 3.14159261;
			Assert.AreEqual(3, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_ReferenceTypes() {
			var notifier = new ExplicitRefTypeNotifier();
			notifier.OnChangeObject = new object();
			notifier.OnChangeObject = notifier.OnChangeObject;
			Assert.AreEqual(1, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_ValueTypes_that_overrides_Equals() {
			var notifier = new ImplicitOnChangeNotifier();
			notifier.StructWithEquals = new StructOverrideEquals { Value = 73 };
			notifier.StructWithEquals = new StructOverrideEquals { Value = 55 };
			Assert.AreEqual(1, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_ReferenceTypes_that_overrides_Equals() {
			var notifier = new ImplicitOnChangeNotifier();
			notifier.ObjectWithEquals = new RefObjectOverrideEquals { Value = "A" };
			notifier.ObjectWithEquals = new RefObjectOverrideEquals { Value = "a" };
			Assert.AreEqual(1, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnSet_for_Nullable_ValueTypes() {
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnSetNullableInt = 43;
			notifier.OnSetNullableInt = 43;
			notifier.OnSetNullableInt = null;
			notifier.OnSetNullableInt = null;
			Assert.AreEqual(4, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_Nullable_ValueTypes() {
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnChangeNullableInt = 43;
			notifier.OnChangeNullableInt = 43;
			notifier.OnChangeNullableInt = null;
			notifier.OnChangeNullableInt = null;
			Assert.AreEqual(2, notifier.ChangeCount);
		}

		#endregion Support for all types of parameter types

		#region Class inheritance

		[TestMethod]
		public void OnSet_for_inherited_properties() {
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnSetBaseInt = 43;
			notifier.OnSetBaseInt = 43;
			Assert.AreEqual(2, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_inherited_properties() {
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnChangeBaseInt = 43;
			notifier.OnChangeBaseInt = 43;
			Assert.AreEqual(1, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnSet_for_Virtual_ValueTypes() {
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnSetVirtualInt = 43;
			notifier.OnSetVirtualInt = 43;
			Assert.AreEqual(4, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_Virtual_ValueTypes() {
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnChangeVirtualInt = 43;
			notifier.OnChangeVirtualInt = 43;
			Assert.AreEqual(3, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnSet_for_Virtual_ReferenceTypes() {
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnSetVirtualObject = new object();
			notifier.OnSetVirtualObject = notifier.OnSetVirtualObject;
			Assert.AreEqual(4, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_Virtual_ReferenceTypes() {
			var expected = new[] { "OnChangeObject", "OnChangeVirtualObject", "OnChangeObject" };
			var notifier = new ExplicitRefTypeNotifier();
			notifier.OnChangeVirtualObject = new object();
			notifier.OnChangeVirtualObject = notifier.OnChangeVirtualObject;
			CollectionAssert.AreEqual(expected, notifier.Changes, $"Expected [{string.Join(", ", expected)}] but got [{string.Join(", ", notifier.Changes)}].");
		}

		[TestMethod]
		public void OnSet_for_New_ValueTypes() {
			var notifier = new ExplicitValueTypeNotifier_Specialized();
			notifier.OnSetInt = 43;
			notifier.OnSetInt = 43;
			Assert.AreEqual(4, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_New_ValueTypes() {
			var notifier = new ExplicitValueTypeNotifier_Specialized();
			notifier.OnChangeInt = 43;
			notifier.OnChangeInt = 43;
			Assert.AreEqual(3, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnSet_for_New_ReferenceTypes() {
			var notifier = new ExplicitValueTypeNotifier_Specialized();
			notifier.OnSetVirtualObject = new object();
			notifier.OnSetVirtualObject = notifier.OnSetVirtualObject;
			Assert.AreEqual(2, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_for_New_ReferenceTypes() {
			var notifier = new ExplicitValueTypeNotifier_Specialized();
			notifier.OnChangeVirtualObject = new object();
			notifier.OnChangeVirtualObject = notifier.OnSetVirtualObject;
			Assert.AreEqual(1, notifier.ChangeCount);
		}

		#endregion Class inheritance

		#region NotifyTarget with different number of arguments

		[TestMethod]
		public void OnSet_calls_NotifyTarget_without_arguments() {
			var notifier = new NotifierWithoutArguments();
			notifier.OnSetString = "Value";
			notifier.OnSetString = "Value";
			Assert.AreEqual(2, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnChange_calls_NotifyTarget_without_arguments() {
			var notifier = new NotifierWithoutArguments();
			notifier.OnChangeString = "Value";
			notifier.OnChangeString = "Value";
			Assert.AreEqual(1, notifier.ChangeCount);
		}

		[TestMethod]
		public void OnSet_calls_NotifyTarget_with_one_argument() {
			var expectedProperties = new[] { "OnSetInt", "OnSetInt" };
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnSetInt = 43;
			notifier.OnSetInt = 43;
			CollectionAssert.AreEqual(expectedProperties, notifier.Changes, $"Expected '{string.Join(",", expectedProperties)}' but got {string.Join(",", notifier.Changes)}.");
		}

		[TestMethod]
		public void OnChange_calls_NotifyTarget_with_one_argument() {
			var expectedProperties = new[] { "OnChangeInt", "OnChangeInt" };
			var notifier = new ExplicitValueTypeNotifier();
			notifier.OnChangeInt = 71;
			notifier.OnChangeInt = 9;
			CollectionAssert.AreEqual(expectedProperties, notifier.Changes, $"Expected '{string.Join(",", expectedProperties)}' but got {string.Join(",", notifier.Changes)}.");
		}

		[TestMethod]
		public void OnSet_calls_NotifyTarget_with_two_arguments() {
			var notifier = new NotifierWithTwoArguments();
			notifier.OnSetString = "Value";
			notifier.OnSetString = "Value";
			Assert.AreEqual(2, notifier.ChangeCount);
			//Assert.Fail("Support intentionally removed in v2?");
		}

		[TestMethod]
		public void OnChange_calls_NotifyTarget_with_two_arguments() {
			Assert.Fail("Support intentionally removed in v2?");
		}

		[TestMethod]
		public void OnSet_calls_NotifyTarget_for_multiple_properties() {
			var expectedChanges = new[] { "Value1", "Value2", "Value3", "Value1", "Value2", "Value3" };

			var notifier = new TestNotifier1();
			notifier.All = 4;
			notifier.All = 4;

			CollectionAssert.AreEqual(notifier.Changes, expectedChanges);
		}

		[TestMethod]
		public void OnChange_calls_NotifyTarget_for_multiple_properties() {
			var expectedChanges = new[] { "Value1", "Value2", "Value3" };

			var notifier = new TestNotifier1();
			notifier.AllOnChange = 4;
			notifier.AllOnChange = 4;

			CollectionAssert.AreEqual(notifier.Changes, expectedChanges);
		}

		#endregion NotifyTarget with different number of arguments

		#region Explicit/Implicit tests

		[TestMethod]
		public void Implicit_notify_OnSet() {
			var notifier = new ImplicitSpy();
			notifier.StringValue = "Value";
			notifier.StringValue = "Value";
			Assert.AreEqual(2, notifier.ChangeCount);
		}

		[TestMethod]
		public void Implicit_notify_OnChange() {
			var notifier = new ImplicitOnChangeSpy();

			notifier.StringValue = "ONE";
			Assert.AreEqual(1, notifier.ChangeCount);

			notifier.StringValue = "ONE";
			Assert.AreEqual(1, notifier.ChangeCount);
		}

		[TestMethod]
		public void Explicit_notify_OnSet_when_class_is_ImplicitOnChange() {
			var notifier = new ImplicitOnChangeSpy();
			notifier.ExplicitOnSetString = "Value";
			notifier.ExplicitOnSetString = "Value";
			Assert.AreEqual(2, notifier.ChangeCount);
		}

		#endregion Explicit/Implicit tests

		#region Verify the manual implementations we glean on for Il hints actually work

		[TestMethod]
		public void Manual_OnChange_only_triggers_if_propertys_Equals_override_returns_false() {
			var value1 = new RefObjectOverrideEquals { Value = "A" };
			var value2 = new RefObjectOverrideEquals { Value = "a" };
			Assert.AreEqual(value1, value2, "Test expects TestValue to implement Equal() as case insensitive.");

			var notifier = new TestNotifier.Patterns.OnChange_Manually();
			notifier.TestValue = value1;
			Assert.AreEqual(1, notifier.ChangeCount, "Changing from null to value should trigger.");

			notifier.TestValue = value2;
			Assert.AreEqual(1, notifier.ChangeCount, "Changing from value to equal value should not trigger.");

			notifier.TestValue = null;
			Assert.AreEqual(2, notifier.ChangeCount, "Changing from value to null should trigger.");

			notifier.TestValue = null;
			Assert.AreEqual(2, notifier.ChangeCount, "Changing from null to null should not trigger.");
		}

		[TestMethod]
		public void Manual_OnSet_triggers_on_each_set() {
			var notifier = new TestNotifier.Patterns.OnSet_Manually();
			notifier.StringValue = "Value";
			notifier.StringValue = "Value";
			Assert.AreEqual(2, notifier.ChangeCount);
		}

		#endregion Verify the manual implementations we glean on for Il hints actually work

	}
}