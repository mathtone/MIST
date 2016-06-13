using Mathtone.MIST.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier.Cases {
	[Notifier]
	public class NotifierWithTwoArguments {

		[SuppressNotify]
		public int ChangeCount { get; private set; }

		[Notify]
		public string OnSetString { get; set; }

		[Notify(NotificationStyle.OnSet)]
		public string OnChangeString { get; set; }

		[NotifyTarget]
		protected void OnPropertyChanged(string name, object value) {
			ChangeCount++;
		}
	}
}