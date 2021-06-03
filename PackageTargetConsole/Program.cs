using Mathtone.MIST;
using System;

namespace PackageTargetConsole {

	[Notifier]
	class Program {


		static void Main(string[] args) {
			Console.WriteLine("Hello World!");
		}

		[NotifyTarget]
		void OnNotify(string name) {

		}
	}
}