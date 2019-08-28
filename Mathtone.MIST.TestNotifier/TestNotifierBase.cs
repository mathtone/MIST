using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathtone.MIST.TestNotifier {

	[Notifier]
	public class TestNotifierBase : IChangeTracker, IChangeCounter {
		public int ChangeCount => Changes.Count;
		public List<string> Changes { get; } = new List<string>();

		[Notify]
		public int OnSetBaseInt { get; set; }

		[Notify(NotificationStyle.OnChange)]
		public int OnChangeBaseInt { get; set; }

		public virtual int OnSetVirtualInt { get; set; }
		public virtual int OnChangeVirtualInt { get; set; }
		public virtual object OnSetVirtualObject { get; set; }
		public virtual object OnChangeVirtualObject { get; set; }

		[NotifyTarget]
		protected void OnPropertyChanged(string propertyName) {
			Changes.Add(propertyName);
		}
	}
}