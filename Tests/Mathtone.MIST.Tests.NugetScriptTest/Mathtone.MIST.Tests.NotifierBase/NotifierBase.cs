using System.ComponentModel;

namespace Mathtone.MIST.Tests {
	public class NotifierBase : INotifyPropertyChanged {

		[NotifyTarget]
		protected void RaisePropertyChanged(string propertyName) {
			var method = PropertyChanged;
			if (method != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}