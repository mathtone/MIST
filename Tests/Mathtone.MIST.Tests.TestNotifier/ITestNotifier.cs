using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier {
	public interface IChangeTracker {
		List<string> Changes { get; }
	}

	public interface IChangeCounter {
		int ChangeCount { get; }
	}

	public interface ITestNotifier : IChangeTracker, IChangeCounter {
		string Value1 { get; set; }
		int Value2 { get; set; }
		int Value3 { get; set; }
		int All { get; set; }
		int Supressed { get; set; }
	}
}