using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mathtone.MIST {
	/// <summary>
	/// Alters IL assemblies after build and implements a notification mechanism.
	/// </summary>
	public class NotificationWeaver {

		string NotifyTargetName = typeof(NotifyTarget).FullName;
		string NotifierTypeName = typeof(NotifierAttribute).FullName;
		string NotifyTypeName = typeof(NotifyAttribute).FullName;
		string assemblyPath;
		DefaultAssemblyResolver resolver;
		MetadataResolver mdResolver;

		/// <summary>
		/// Initializes a new instance of the <see cref="NotificationWeaver"/> class.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly which is to be altered.</param>
		public NotificationWeaver(string assemblyPath) {
			this.assemblyPath = assemblyPath;
			this.resolver = new DefaultAssemblyResolver();
			resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
			mdResolver = new MetadataResolver(resolver);
		}

		/// <summary>
		/// Weaves the notification mechanism into the assembly
		/// </summary>
		/// <param name="debug">if set to <c>true</c> [debug].</param>
		public void Weave(bool debug = false) {
			bool mustSave = false;
			var assemblyDef = null as AssemblyDefinition;
			var readParameters = new ReaderParameters { ReadSymbols = debug };
			var writeParameters = new WriterParameters { WriteSymbols = debug };

			//Load the assembly.
			using (var stream = File.OpenRead(assemblyPath)) {
				assemblyDef = AssemblyDefinition.ReadAssembly(stream, readParameters);
			}

			//Search for types and weave notifiers into them if necessary.
			foreach (var moduleDef in assemblyDef.Modules) {
				foreach (var typeDef in moduleDef.Types) {
					mustSave |= WeaveType(typeDef);
				}
			}

			//If the assembly has been altered then rewrite it.
			if (mustSave) {
				using (var stream = File.OpenWrite(assemblyPath)) {
					assemblyDef.Write(stream, writeParameters);
					stream.Flush();
				}
			}
		}

		/// <summary>
		/// Weaves the notification mechanism into the supplied type.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <returns><c>true</c> if the type was altered, <c>false</c> otherwise.</returns>
		/// <exception cref="System.Exception"></exception>
		bool WeaveType(TypeDefinition typeDef) {

			var rtn = false;
			var notifierAttr = typeDef.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == NotifierTypeName);
			var mode = NotificationMode.Explicit;
			//Search for a NotifyAttribute
			if (notifierAttr != null) {

				//Locate the notification target method.
				var notifyTarget = GetNotifyTarget(typeDef);

				if (notifyTarget == null) {
					throw new Exception($"Cannot locate notify target for type: {typeDef.Name}");
				}

				//Determine whether to use explicit/implicit notifier identification.
				if (notifierAttr.HasConstructorArguments) {
					mode = (NotificationMode)notifierAttr.ConstructorArguments[0].Value;
				}
				//Identify the name of the property/properties that will be passed to the notificaiton method.
				foreach (var propDef in typeDef.Properties) {

					var propNames = GetNotifyPropertyNames(propDef);

					if(!propNames.Any() && mode == NotificationMode.Implicit && propDef.GetMethod.IsPublic) {
						propNames = new[] { propDef.Name };
					}
					if (propNames != null) {
						WeaveNotifiersIntoProperty(propDef, notifyTarget, propNames);
						rtn = true;
					}
				}
			}
			return rtn;

		}

		/// <summary>
		/// Gets the notification target method, market with a <see cref="NotifyTarget"/> attribute.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <returns>MethodReference.</returns>
		MethodReference GetNotifyTarget(TypeDefinition typeDef) {
			foreach (var methDef in typeDef.Methods) {
				if (methDef.CustomAttributes.Any(a => a.AttributeType.FullName == NotifyTargetName)) {
					return methDef;
				}
			}
			var bt = typeDef.BaseType;
			if (bt != null) {
				var btd = mdResolver.Resolve(bt);
				var rtn = GetNotifyTarget(btd);
				if (rtn != null) {
					rtn = typeDef.Module.ImportReference(rtn);
				}
				return rtn;
			}
			else {
				return null;
			}
		}

		/// <summary>
		/// Gets the property names that should be passed to the notification target method when the property value is changed.
		/// </summary>
		/// <param name="propDef">The property definition.</param>
		/// <returns>IEnumerable&lt;System.String&gt;.</returns>
		IEnumerable<string> GetNotifyPropertyNames(PropertyDefinition propDef) {
			//Check for the NotifyAttribute
			var attr = propDef.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == NotifyTypeName);

			if (attr != null) {
				//Return property names supplied by the constructor, if none are specified return the property name itself.
				if (attr.HasConstructorArguments) {
					var args = attr.ConstructorArguments[0].Value as CustomAttributeArgument[];
					if (args.Length == 0) {
						yield return propDef.Name;
					}
					else {
						foreach (var arg in args) {
							yield return (string)arg.Value;
						}
					}
				}
				else {
					yield return propDef.Name;
				}
			}
		}

		/// <summary>
		/// Weaves notifiers into the property.  This is where the magic happens.
		/// </summary>
		/// <param name="propDef">The property definition.</param>
		/// <param name="notifyTarget">The notify target.</param>
		/// <param name="notifyPropertyNames">The notify property names.</param>
		void WeaveNotifiersIntoProperty(PropertyDefinition propDef, MethodReference notifyTarget, IEnumerable<string> notifyPropertyNames) {

			var msil = propDef.SetMethod.Body.GetILProcessor();
			//Should produce something liek the following.
			/*
			.method public hidebysig specialname instance void 
			.et_SomeProperty(string 'value') cil managed
			{
			  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
			  // Code size       8 (0x8)
			  .maxstack  8
			  IL_0000:  ldarg.0
			  IL_0001:  ldarg.1
			  IL_0002:  stfld      string Mathtone.MIST.Tests.TestNotifier::'<SomeProperty>k__BackingField'
			  IL_0007:  ret
			} // end of method TestNotifier::set_SomeProperty
			*/

			msil.InsertBefore(propDef.SetMethod.Body.Instructions[0], msil.Create(OpCodes.Nop));
			foreach (var notifyPropertyName in notifyPropertyNames) {
				var ldarg0 = msil.Create(OpCodes.Ldarg_0);
				var callNotifyTarget = msil.Create(OpCodes.Call, notifyTarget);
				var propertyName = msil.Create(OpCodes.Ldstr, notifyPropertyName);
				msil.InsertBefore(propDef.SetMethod.Body.Instructions[propDef.SetMethod.Body.Instructions.Count - 1], ldarg0);
				msil.InsertAfter(ldarg0, propertyName);
				msil.InsertAfter(propertyName, callNotifyTarget);
				msil.InsertAfter(callNotifyTarget, msil.Create(OpCodes.Nop));
			}
		}

	}

	/// <summary>
	/// Class NotificationWeaverBuildTask.
	/// </summary>
	/// <example>
	/// place the following XML in the project file.  The directorey containing Mathtone.MIST.Builder.dll should also contain Mathtone.MIST.dll, Mono.Cecil.dll and Mono.Cecil.pdb.dll
	/// <UsingTask TaskName = "Mathtone.MIST.NotificationWeaverBuildTask"
	///		 AssemblyFile="...path to "		 
	/// />
	/// <Target Name = "AfterBuild" >
	///		<NotificationWeaverBuildTask TargetPath="$(TargetPath)" DebugMode="True"/>
	/// </Target>
	/// </example>
	public class NotificationWeaverBuildTask : Task {

		/// <summary>
		/// Gets or sets the target path.
		/// </summary>
		/// <value>The target path.</value>
		[Required]
		public string TargetPath { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [debug mode].
		/// </summary>
		/// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
		[Required]
		public bool DebugMode { get; set; }

		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>true if the task successfully executed; otherwise, false.</returns>
		public override bool Execute() {
			new NotificationWeaver(TargetPath).Weave(DebugMode);
			return true;
		}
	}
}