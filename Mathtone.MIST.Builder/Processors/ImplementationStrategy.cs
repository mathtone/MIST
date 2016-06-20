using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Mathtone.MIST.Processors {

	enum ImplementationStyle {
		Inline, Wrapped
	}

	class ImplementationStrategy {
		public bool IsIgnored { get; set; }
		public NotificationStyle NotificationStyle { get; set; }
		public ImplementationStyle ImplementationStyle { get; set; }
		public string[] NotifyValues { get; set; }
		public PropertyDefinition Property { get; set; }
		public MethodReference NotifyTarget { get; set; }
		public MethodDefinition NotifyTargetDefinition;
	}
}