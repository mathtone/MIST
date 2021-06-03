using Mathtone.MIST;
using System;

namespace PackageTargetConsole {

	[Notifier(NotificationMode.Implicit)]
	class Program : NotifierBase {

		public string Id { get; set; }
		public string Name { get; set; }

		static void Main(string[] args) => _ = new Program {
			Id = "eyyyyy",
			Name = "therer"
		};
	}

	public class NotifierBase {

		[NotifyTarget]
		protected void OnNotify(string name, object newValue) =>
			Console.WriteLine($"{name} changed to {newValue}");
	}
}