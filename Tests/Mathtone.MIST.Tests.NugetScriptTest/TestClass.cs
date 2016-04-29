using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.Tests.NugetScriptTest {
	[Notifier]
	public class TestClass {
		[Notify]
		public string Test { get; set; }

		[NotifyTarget]
		void Changed(string propertyName) {
			;
		}
	}
}