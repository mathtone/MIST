using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.BuildTest
{
	[Notifier]
    public class TestNotifier
    {
		public string SomeProperty { get; set; }

		public TestNotifier() {
			SomeProperty = "HELLO";
		}

		[NotifyTarget]
		void PropertyChanged(string name) {

		}
    }
}