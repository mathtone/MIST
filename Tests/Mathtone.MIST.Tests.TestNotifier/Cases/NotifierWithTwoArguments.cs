using Mathtone.MIST.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathtone.MIST.TestNotifier.Cases {
	[Notifier]
	public class NotifierWithTwoArguments {

		[SuppressNotify]
		public int ChangeCount { get; private set; }

		[Notify]
		public string OnSetString { get; set; }

		[Notify]
		public int OnSetInt { get; set; }

		[Notify(NotificationStyle.OnChange)]
		public string OnChangeString { get; set; }

		[Notify(NotificationStyle.OnChange)]
		public int OnChangeInt { get; set; }

		[NotifyTarget]
		protected void OnPropertyChanged(string name, object value) {
			ChangeCount++;
		}
	}
}