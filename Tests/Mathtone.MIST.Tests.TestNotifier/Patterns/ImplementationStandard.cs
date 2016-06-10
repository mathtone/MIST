using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier.Patterns {

	[Notifier(NotificationMode.Implicit, NotificationStyle.OnSet)]
	public class OnSetImplementation {
		public string StringValue { get; set; }

		public int IntValue { get; set; }

		[NotifyTarget]
		protected void OnChange(string propertyName) {
			Console.WriteLine(propertyName);
		}
	}

	[Notifier(NotificationMode.Implicit, NotificationStyle.OnChange)]
	public class OnChangeImplementation {
		public string StringValue { get; set; }

		public int IntValue { get; set; }

		[NotifyTarget]
		protected void OnChange(string propertyName) {
			Console.WriteLine(propertyName);
		}
	}

	public class OnChangeStandard {
		string stringValue;
		int intValue;

		public string StringValue {
			get { return stringValue; }
			set {
				var tValue = StringValue;
				set_StringValue_Mist(value);
				if (tValue.Equals(value)) {
					OnChange("StringValue");
				}
			}
		}

		private void set_StringValue_Mist(string value) {
			stringValue = value;
		}

		public int IntValue {
			get { return intValue; }
			set {
				var tValue = IntValue;
				set_IntValue_Mist(value);

				if (!tValue.Equals(value)) {
					OnChange("IntValue");
				}
			}
		}

		private void set_IntValue_Mist(int value) {
			intValue = value;
		}

		protected void OnChange(string propertyName) {
			Console.WriteLine(propertyName);
		}
	}

	public class OnSetStandard {
		string stringValue;
		int intValue;

		public string StringValue {
			get { return stringValue; }
			set {
				stringValue = value;
				OnChange("StringValue");
			}
		}

		public int IntValue {
			get { return intValue; }
			set {
				intValue = value;
				OnChange("IntValue");
			}
		}


		protected void OnChange(string propertyName) {
			Console.WriteLine(propertyName);
		}
	}
}