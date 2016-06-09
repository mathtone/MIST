using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.Processors {
	public interface IDefinitionProcessor<T> {
		bool ContainsChanges { get; }
		void Process(T definition);
	}
}
