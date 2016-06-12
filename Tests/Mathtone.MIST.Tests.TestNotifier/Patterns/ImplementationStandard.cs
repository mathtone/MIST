using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier.Patterns {

    [Notifier(NotificationMode.Implicit, NotificationStyle.OnSet)]
	public class OnSet_Misted {
		public string StringValue { get; set; }

		public int IntValue { get; set; }

		[NotifyTarget]
		protected void OnChange(string propertyName) {
			Console.WriteLine(propertyName);
		}
	}

    public class OnSet_Manually
    {
        string stringValue;
        int intValue;

        public int ChangeCount => Changes.Count;
        public List<string> Changes { get; } = new List<string>();

        public string StringValue
        {
            get { return stringValue; }
            set
            {
                stringValue = value;
                OnPropertyChanged("StringValue");
            }
        }

        public int IntValue
        {
            get { return intValue; }
            set
            {
                intValue = value;
                OnPropertyChanged("IntValue");
            }
        }


        protected void OnPropertyChanged(string propertyName)
        {
            //Console.WriteLine(propertyName);
            Changes.Add(propertyName);
        }
    }

    [Notifier(NotificationMode.Implicit, NotificationStyle.OnChange)]
	public class OnChange_Misted {
		public string StringValue { get; set; }

		public int IntValue { get; set; }

		[NotifyTarget]
		protected void OnChange(string propertyName) {
			Console.WriteLine(propertyName);
		}
	}

	public class OnChange_Manually {
		string stringValue;
		int intValue;

        public int ChangeCount => Changes.Count;
        public List<string> Changes { get; } = new List<string>();

        public string StringValue {
			get { return stringValue; }
			set {
				var tValue = StringValue;
                stringValue = value;
                if (!tValue?.Equals(value) ?? (value != null))
                {
                    OnPropertyChanged("StringValue");
				}
			}
		}

		public int IntValue {
			get { return intValue; }
			set {
				var tValue = IntValue;
                intValue = value;
                if (!tValue.Equals(value)) {
					OnPropertyChanged("IntValue");
				}
			}
		}

        public Cases.RefObjectOverrideEquals TestValue
        {
            get { return _testValue; }
            set
            {
                var tValue = _testValue;
                _testValue = value;
                if (!tValue?.Equals(value) ?? (value != null))
                {
                    OnPropertyChanged("TestValue");
                }
            }
        }
        private Cases.RefObjectOverrideEquals _testValue;

        protected void OnPropertyChanged(string propertyName) {
            //Console.WriteLine(propertyName);
            Changes.Add(propertyName);
		}
	}

}