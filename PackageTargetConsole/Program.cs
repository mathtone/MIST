using Mathtone.MIST;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PackageTargetConsole {

	[Notifier(NotificationMode.Implicit)]
	class Program : NotifierBase {

		public string Id { get; set; }
		public string Name { get; set; }

		static void Main(string[] args) => _ = new Program {
			Id = "1",
			Name = "2"
		};
	}

	public class NotifierBase {

		[NotifyTarget]
		[SuppressMessage("Performance", "CA1822:Mark members as static")]
		protected void OnNotify(string name, object newValue) =>
			Console.WriteLine($"{name} changed to {newValue}");
	}
}
