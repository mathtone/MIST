namespace Mathtone.MIST.Tests {

	[Notifier(NotificationMode.Implicit)]
	public partial class TestNotifier {

	}

	public partial class TestNotifier : NotifierBase {

		[Notifier(NotificationMode.Explicit)]
		public class NestedClass : NotifierBase {

			int change2;

			[Notify]
			public int Change1 { get; set; }

			[Notify]
			public int Change2 {
				get { return change2; }
				set { change2 = value + 1; }
			}

			[NotifyTarget]
			void Notify(string propertyName) {
				base.RaisePropertyChanged("-" + propertyName);
			}
		}

		public string Change1 { get; set; }

		public string Change2 { get; set; }

		public string ReadOnly => Change1 + Change2;

		public NestedClass Nested { get; } = new NestedClass();

		[SuppressNotify]
		public string ChangeNone { get; set; }

		[Notify("Change1", "Change2", "ReadOnly")]
		public string ChangeAll { get; set; }

		[Notify(null)]
		public string ChangeNull { get; set; }
	}

	[Notifier(NotificationMode.Implicit)]
	public class TestNotifier2 : NotifierBase {

		public string Value { get; set; }

		//[NotifyTarget]
		void OnNotify() {
			base.RaisePropertyChanged(null);
		}

		[NotifyTarget]
		void OnNotify1(string propertyName) {
			base.RaisePropertyChanged(propertyName);
		}

		//[NotifyTarget]
		void OnNotify2(string propertyName, object value) {
			base.RaisePropertyChanged(propertyName);
		}

		//[NotifyTarget]
		void OnNotify(string propertyName, object oldValue, object newValue) {
			base.RaisePropertyChanged(propertyName);
		}
	}

	[Notifier(NotificationMode.Implicit)]
	public class TestNotifier3<T> {
		public T Value { get; set; }
	}
	/*
	 .method public hidebysig specialname instance void 
        set_Value(string 'value') cil managed
		{
		  // Code size       30 (0x1e)
		  .maxstack  4
		  .locals init ([0] string oldValue)
		  IL_0000:  nop
		  IL_0001:  ldarg.0
		  IL_0002:  ldfld      string Mathtone.MIST.Tests.ControlExample::'value'
		  IL_0007:  stloc.0
		  IL_0008:  ldarg.0
		  IL_0009:  ldarg.1
		  IL_000a:  stfld      string Mathtone.MIST.Tests.ControlExample::'value'
		  IL_000f:  ldarg.0
		  IL_0010:  ldstr      "Value"
		  IL_0015:  ldloc.0
		  IL_0016:  ldarg.1
		  IL_0017:  call       instance void Mathtone.MIST.Tests.ControlExample::OnNotify(string,
																						  object,
																						  object)
		  IL_001c:  nop
		  IL_001d:  ret
		} // end of method ControlExample::set_Value

		.method public hidebysig specialname instance void 
        set_Value2(string 'value') cil managed
		{
		  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
		  // Code size       8 (0x8)
		  .maxstack  8
		  IL_0000:  ldarg.0
		  IL_0001:  ldarg.1
		  IL_0002:  stfld      string Mathtone.MIST.Tests.ControlExample::'<Value2>k__BackingField'
		  IL_0007:  ret
		} // end of method ControlExample::set_Value2
	 */

	public class ControlExample {

		string value;
		public string Value {
			get { return value; }
			set {
				var oldValue = this.value;
				this.value = value;
				OnNotify("Value", oldValue, value);
			}
		}

		public string Value2 {
			get; set;
		}

		//[NotifyTarget]
		void OnNotify(string propertyName, object oldValue, object newValue) {
			//base.RaisePropertyChanged(propertyName);
		}
	}
}