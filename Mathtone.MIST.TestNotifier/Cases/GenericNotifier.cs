using System.Collections.Generic;

namespace Mathtone.MIST.TestNotifier.Cases {
	[Notifier]
	public abstract class GenericNotifierBase<T> {
		public T Value { get; set; }
		public List<string> Changes { get; } = new List<string>();

		[NotifyTarget]
		protected void OnPropertyChanged(string propertyName) {
			Changes.Add(propertyName);
		}
	}

	public class GenericNotifierDerived : GenericNotifierBase<int> {
		public GenericNotifierDerived() {
			this.Value = 1;
		}
	}
}