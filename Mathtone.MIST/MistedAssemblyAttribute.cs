using System;

namespace Mathtone.MIST {
	/// <summary>
	/// Used by the build process to mark assemblies that have undergone processing.  This attribute should NOT be implemented by the user.
	/// </summary>
	/// <seealso cref="System.Attribute" />
	//[AttributeUsage(AttributeTargets.Assembly)]
	//Ok this works, it will prevent the user from adding this attribute to the assembly but could this possibly be a good idea?
	[AttributeUsage(AttributeTargets.Module)]
	public class MistedAssemblyAttribute : Attribute {

	}
}
