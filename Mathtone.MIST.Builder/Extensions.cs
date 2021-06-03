using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Mathtone.MIST.Builder {
	public static class Extensions {
		public static bool ContainsAttribute(this IMemberDefinition definition, Type attributeType) =>
			definition.CustomAttributes.Any(a => a.AttributeType.FullName == attributeType.FullName);

		public static CustomAttribute GetAttribute(this IMemberDefinition definition, Type attributeType) =>
			definition.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == attributeType.FullName);
	}
}