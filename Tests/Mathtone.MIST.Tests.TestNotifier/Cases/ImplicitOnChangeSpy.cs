using Mathtone.MIST.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier.Cases {

	[Notifier(NotificationMode.Implicit, NotificationStyle.OnChange)]
	public class ImplicitOnChangeSpy : TestNotifierBase
	{
		public string StringValue { get; set; } = "";

		[Notify(NotificationStyle.OnSet)]
		public string ExplicitOnSetString { get; set; }
	}
}
