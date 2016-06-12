using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mathtone.MIST.TestNotifier.Cases {

	[Notifier]
	public class TestNotifier1 : TestNotifierBase, ITestNotifier {

		[Notify]
		public string Value1 { get; set; }

		[Notify]
		public int Value2 { get; set; }

        [Notify]
		public int Value3 { get; set; }

		[Notify("Value1", "Value2", "Value3")]
		public int All { get; set; }

        [Notify(NotificationStyle.OnChange, "Value1", "Value2", "Value3")]
        public int AllOnChange { get; set; }

        public int Supressed { get; set; }
	}

	[Notifier(NotificationMode.Implicit)]
	public class TestNotifier2 : TestNotifierBase, ITestNotifier {

		public string Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }

		[Notify("Value1", "Value2", "Value3")]
		public int All { get; set; }

		[SuppressNotify]
		public int Supressed { get; set; }
	}

	[Notifier(NotificationMode.Implicit)]
	public class TestNotifier3 : TestNotifierBase, ITestNotifier {
		string value1;
		int value2, value3;

		public string Value1 {
			get { return value1; }
			set { value1 = value; }
		}
		public int Value2 {
			get { return value2; }
			set { value2 = value; }
		}

		//this is the acid test
		public int Value3 {
			get {
				return value3;
			}
			set {
				if (value % 2 == 0) {
					return;
				}
				else {
					value3 = value;
					return;
				}
			}
		}

		[Notify("Value1", "Value2", "Value3")]
		public int All { get; set; }

		[SuppressNotify]
		public int Supressed { get; set; }
	}


	//Yodawg, I heard you liked notifiers, 
	[Notifier(NotificationMode.Explicit)]
	public class ExplicitRefTypeNotifier : TestNotifierBase {

		[Notify(NotificationStyle.OnChange)]
		public object OnChangeObject { get; set; }

        [Notify(NotificationStyle.OnSet)]
        public override object OnSetVirtualObject
        {
            get { return base.OnSetVirtualObject; }
            set
            {
                OnChangeObject = new object();
                base.OnSetVirtualObject = value;
            }
        }

        [Notify(NotificationStyle.OnChange)]
        public override object OnChangeVirtualObject
        {
            get { return base.OnChangeVirtualObject; }
            set
            {
                OnChangeObject = new object();
                base.OnChangeVirtualObject = value;
            }
        }
    }

    [Notifier(NotificationMode.Implicit, NotificationStyle.OnChange)]
	public class ImplicitOnChangeNotifier : TestNotifierBase {

        public RefObjectOverrideEquals ObjectWithEquals { get; set; }
        public StructOverrideEquals StructWithEquals { get; set; }
    }

    [Notifier]
	public class ExplicitValueTypeNotifier : TestNotifierBase {

        [Notify(NotificationStyle.OnChange)]
        public int OnChangeInt { get; set; }

        [Notify(NotificationStyle.OnSet)]
        public int OnSetInt { get; set; }

        [Notify(NotificationStyle.OnChange)]
        public double OnChangeDouble { get; set; }

        [Notify(NotificationStyle.OnSet)]
        public double OnSetDouble { get; set; }

        [Notify(NotificationStyle.OnChange)]
        public bool OnChangeBool { get; set; }

        [Notify(NotificationStyle.OnSet)]
        public bool OnSetBool { get; set; }

        [Notify(NotificationStyle.OnChange)]
        public int? OnChangeNullableInt { get; set; }

        [Notify(NotificationStyle.OnSet)]
        public int? OnSetNullableInt { get; set; }

        [Notify(NotificationStyle.OnSet)]
        public override int OnSetVirtualInt
        {
            get { return base.OnSetVirtualInt; }
            set
            {
                OnSetBool = !OnSetBool;
                base.OnSetVirtualInt = value;
            }
        }

        [Notify(NotificationStyle.OnChange)]
        public override int OnChangeVirtualInt
        {
            get { return base.OnChangeVirtualInt; }
            set
            {
                OnChangeBool = !OnChangeBool;
                base.OnChangeVirtualInt = value;
            }
        }
    }

    public class RefObjectOverrideEquals {

		public string Value { get; set; }

		public override bool Equals(object obj) {
			var value = obj as RefObjectOverrideEquals;
            if (value == null) return false;
            if (value.Value == null && this.Value == null) return true;
            return value.Value.ToLower() == this.Value.ToLower();
		}

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public struct StructOverrideEquals
    {
        public int Value { get; set; }

        public override bool Equals(object obj)
        {
            var value = (StructOverrideEquals)obj;
            return value.Value % 2 == this.Value % 2;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}