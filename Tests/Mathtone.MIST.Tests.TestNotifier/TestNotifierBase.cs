using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier {
	public class TestNotifierBase : IChangeTracker, IChangeCounter {
		public int ChangeCount => Changes.Count;
		public List<string> Changes { get; } = new List<string>();

		[NotifyTarget]
		protected void OnPropertyChanged(string propertyName) {
			Changes.Add(propertyName);
		}
	}
}