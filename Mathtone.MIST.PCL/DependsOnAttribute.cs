using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathtone.MIST {
	/// <summary>
	/// Used to mark a property as a notifier that depends on another property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class DependsOnAttribute : Attribute {

		public string[] DependsOnProperties { get; protected set; }

		public DependsOnAttribute(params string[] propertyNames) {
			this.DependsOnProperties = propertyNames;
		}
	}
}