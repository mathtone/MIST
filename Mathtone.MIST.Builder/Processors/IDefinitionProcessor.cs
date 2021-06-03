using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathtone.MIST.Builder.Processors {
	public interface IDefinitionProcessor<T> {
		bool ContainsChanges { get; }
		void Process(T definition);
	}
}
