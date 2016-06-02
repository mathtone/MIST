using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Reflection;

namespace Mathtone.MIST {
	/// <summary>
	/// Alters IL assemblies after build and implements a notification mechanism.
	/// </summary>
	public class NotificationWeaver {

		string NotifyTargetName = typeof(NotifyTarget).FullName;
		string NotifierTypeName = typeof(NotifierAttribute).FullName;
		string NotifyTypeName = typeof(NotifyAttribute).FullName;
		string SuppressNotifyTypeName = typeof(SuppressNotifyAttribute).FullName;
		string assemblyPath;
		DefaultAssemblyResolver resolver;
		MetadataResolver mdResolver;

		string ApplicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

		/// <summary>
		/// Initializes a new instance of the <see cref="NotificationWeaver"/> class.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly which is to be altered.</param>
		public NotificationWeaver(string assemblyPath) {

			this.assemblyPath = assemblyPath;
			this.resolver = new DefaultAssemblyResolver();
			this.resolver.AddSearchDirectory(ApplicationPath);
			this.resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
			this.mdResolver = new MetadataResolver(resolver);
		}

		/// <summary>
		/// Weaves the notification mechanism into the assembly
		/// </summary>
		/// <param name="debug">if set to <c>true</c> [debug].</param>
		public void InsertNotifications(bool debug = false) {

			bool mustSave = false;
			var assemblyDef = null as AssemblyDefinition;
			var readParameters = new ReaderParameters { ReadSymbols = debug, AssemblyResolver = resolver };
			var writeParameters = new WriterParameters { WriteSymbols = debug };

			//Load the assembly.
			using (var stream = File.OpenRead(assemblyPath)) {
				assemblyDef = AssemblyDefinition.ReadAssembly(stream, readParameters);
			}

			//Search for types and weave notifiers into them if necessary.
			foreach (var moduleDef in assemblyDef.Modules) {
				foreach (var typeDef in moduleDef.Types) {
					try {
						mustSave |= ProcessType(typeDef);
					}
					catch (Exception ex) {
						throw new BuildTaskErrorException(typeDef.FullName, ex);
					}
				}
			}

			//If the assembly has been altered then rewrite it.
			if (mustSave) {
				using (var stream = File.OpenWrite(assemblyPath)) {
					assemblyDef.Write(stream, writeParameters);
				}
			}
		}

		/// <summary>
		/// Weaves the notification mechanism into the supplied type.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <returns><c>true</c> if the type was altered, <c>false</c> otherwise.</returns>
		/// <exception cref="System.Exception"></exception>
		protected bool ProcessType(TypeDefinition typeDef) {

			var rtn = false;

			//Search for a NotifyAttribute
			var notifierAttr = typeDef.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == NotifierTypeName);

			if (notifierAttr != null) {

				//Use explicit mode if not otherwise specified
				var mode = NotificationMode.Explicit;

				//Locate the notification target method.
				var notifyTarget = GetNotifyTarget(typeDef);

				if (notifyTarget == null) {
					throw new CannotLocateNotifyTargetException(typeDef.FullName);
				}

				//Determine whether to use explicit/implicit notifier identification.
				if (notifierAttr.HasConstructorArguments) {
					mode = (NotificationMode)notifierAttr.ConstructorArguments[0].Value;
				}

				//Identify the name of the property/properties that will be passed to the notification method.
				foreach (var propDef in typeDef.Properties) {
					var propNames = GetNotifyPropertyNames(propDef);

					if (!ContainsAttribute(propDef, SuppressNotifyTypeName)) {
						//In implcit mode implement notification for all public properties
						if (!propNames.Any() && mode == NotificationMode.Implicit && propDef.GetMethod.IsPublic) {
							propNames = new[] { propDef.Name };
						}
						if (propNames != null) {
							InsertNotificationsIntoProperty(propDef, notifyTarget, propNames);
							rtn = true;
						}
					}
				}
			}

			//Recursively process any nested type definitions.
			foreach (var type in typeDef.NestedTypes) {
				ProcessType(type);
			}

			return rtn;
		}

		/// <summary>
		/// Gets the notification target method, market with a <see cref="NotifyTarget"/> attribute.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <returns>MethodReference.</returns>
		protected MethodReference GetNotifyTarget(TypeDefinition typeDef) {

			//Check each method for a NotifyTargetAttribute
			foreach (var methDef in typeDef.Methods) {
				if (ContainsAttribute(methDef, NotifyTargetName)) {
					var isValid = false;
					switch (methDef.Parameters.Count) {
						case 0:
							isValid = true;
							break;
						case 1:
							isValid = methDef.Parameters[0].ParameterType.FullName == typeof(string).FullName;
							break;
							//case 2:
							//	isValid = methDef.Parameters[0].ParameterType.FullName == typeof(string).FullName &&
							//		methDef.Parameters[1].ParameterType.FullName == typeof(object).FullName;
							//	break;
							//case 3:
							//	isValid = methDef.Parameters[0].ParameterType.FullName == typeof(string).FullName &&
							//		methDef.Parameters[1].ParameterType.FullName == typeof(object).FullName &&
							//		methDef.Parameters[2].ParameterType.FullName == typeof(object).FullName;
							//	break;
					}
					if (isValid) {
						return methDef;
					}
					else {
						throw new InvalidNotifyTargetException(methDef.FullName);
					}
				}
			}

			//Notify target not found, search base type
			var baseType = typeDef.BaseType;

			if (baseType != null) {

				//Get the definition of the base type
				var baseTypeDef = mdResolver.Resolve(baseType);

				//Search recursively for a target
				var rtn = GetNotifyTarget(baseTypeDef);

				if (rtn != null) {

					//A target has been found, import a reference to the target method;
					rtn = typeDef.Module.ImportReference(rtn);
				}

				return rtn;
			}
			else {
				return null;
			}
		}

		/// <summary>
		/// Determines whether the specified definition is decorated with an attribute of the named type.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="attributeTypeName">Name of the attribute type.</param>
		/// <returns><c>true</c> if the specified definition contains attribute; otherwise, <c>false</c>.</returns>
		public static bool ContainsAttribute(MethodDefinition definition, string attributeTypeName) =>
			definition.CustomAttributes.Any(a => a.AttributeType.FullName == attributeTypeName);

		/// <summary>
		/// Determines whether the specified definition is decorated with an attribute of the named type.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="attributeTypeName">Name of the attribute type.</param>
		/// <returns><c>true</c> if the specified definition contains attribute; otherwise, <c>false</c>.</returns>
		public static bool ContainsAttribute(PropertyDefinition definition, string attributeTypeName) =>
			definition.CustomAttributes.Any(a => a.AttributeType.FullName == attributeTypeName);

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
					if (args == null) {
						//Argument is null.
						yield return null;
					}
					else if (args.Length == 0) {
						//Property names not specified.
						yield return propDef.Name;
					}
					else {
						//Multiple arguments have been passed.
						foreach (var arg in args) {
							yield return (string)arg.Value;
						}
					}
				}
				else {
					//No fancy stuff, just return the property name.
					yield return propDef.Name;
				}
			}
		}

		protected static void InsertNotificationsIntoProperty(PropertyDefinition propDef, MethodReference notifyTarget, IEnumerable<string> notifyPropertyNames) {
			if (propDef.SetMethod == null)
				//This is a read-only property, there's nothing to do.
				return;
			else if (propDef.SetMethod.Body == null) {
				//This is an abstract property, we don't do these either.
				throw new InvalidNotifierException();
			}

			var setMeth = propDef.SetMethod;
			var newMeth = new MethodDefinition(setMeth.Name, setMeth.Attributes, setMeth.ReturnType);
			var msil = newMeth.Body.GetILProcessor();
			var instructions = new List<Instruction>();

			newMeth.Name = setMeth.Name;
			newMeth.DeclaringType = setMeth.DeclaringType;
			newMeth.Parameters.Add(new ParameterDefinition(setMeth.Parameters[0].ParameterType));

			instructions.AddRange(new[] {
				msil.Create(OpCodes.Ldarg_0),
				msil.Create(OpCodes.Ldarg_1),
				msil.Create(OpCodes.Call, setMeth)
			});

			foreach (var notifyPropertyName in notifyPropertyNames) {
				instructions.AddRange(new[] {
					msil.Create(OpCodes.Ldarg_0),
					msil.Create(OpCodes.Ldstr, notifyPropertyName),
					msil.Create(OpCodes.Call, notifyTarget),
					msil.Create(OpCodes.Nop)
				});
			}

			foreach (var i in instructions) {
				newMeth.Body.Instructions.Add(i);
			}

			newMeth.Body.Instructions.Add(msil.Create(OpCodes.Ret));
			setMeth.Name = setMeth.Name + "`Impl";
			propDef.SetMethod = newMeth;
			newMeth.DeclaringType.Methods.Add(newMeth);
		}

		/// <summary>
		/// Weaves notifiers into the property.  This is where the magic happens.
		/// </summary>
		/// <param name="propDef">The property definition.</param>
		/// <param name="notifyTarget">The notify target.</param>
		/// <param name="notifyPropertyNames">The notify property names.</param>
		protected static void InsertNotificationsIntoProperty2(PropertyDefinition propDef, MethodReference notifyTarget, IEnumerable<string> notifyPropertyNames) {

			if (propDef.SetMethod == null)
				//This is a read-only property
				return;
			else if (propDef.SetMethod.Body == null) {
				//This is an abstract property, we don't do these either.
				throw new InvalidNotifierException();
			}

			var methodBody = propDef.SetMethod.Body;

			//Retrieve an IL writer
			var msil = methodBody.GetILProcessor();

			//Insert a Nop before the first instruction (like... at the beginning).
			var begin = msil.Create(OpCodes.Nop);
			msil.InsertBefore(methodBody.Instructions[0], begin);

			//Call the notification target method for 
			foreach (var notifyPropertyName in notifyPropertyNames) {

				var beginInstructions = new Instruction[0];
				var endInstructions = new Instruction[0];

				//Load the value of the property name to be passed to the notify target onto the stack.
				var propertyName = notifyPropertyName == null ?
					msil.Create(OpCodes.Ldnull) :
					msil.Create(OpCodes.Ldstr, notifyPropertyName);


				//Emit a call to the notify target
				var callNotifyTarget = msil.Create(OpCodes.Call, notifyTarget);
				switch (notifyTarget.Parameters.Count) {
					case 0:
						endInstructions = new[] {
							msil.Create(OpCodes.Ldarg_0),
							msil.Create(OpCodes.Call, notifyTarget),
							msil.Create(OpCodes.Nop)
						};
						break;
					case 1:
						endInstructions = new[] {
							msil.Create(OpCodes.Ldarg_0),
							propertyName,
							msil.Create(OpCodes.Call, notifyTarget),
							msil.Create(OpCodes.Nop)
						};
						break;

					//This works, but allowing this simply create too many questions.  Eliminating these options in favor of simplicity.
					//In the future I will 
					//case 2:
					//	endInstructions = new[] {
					//		msil.Create(OpCodes.Ldarg_0),
					//		propertyName,
					//		msil.Create(OpCodes.Ldarg_1),
					//		msil.Create(OpCodes.Call, notifyTarget),
					//		msil.Create(OpCodes.Nop)
					//	};
					//	break;
					//case 3:
					//	//this one is a little more complicated
					//	//Create a local variable and set it to the current value of the property.
					//	var variableType = propDef.SetMethod.Parameters[0].ParameterType;
					//	var variableDef = new VariableDefinition($"f__{propDef.Name}_temp", variableType);
					//	propDef.SetMethod.Body.Variables.Add(variableDef);
					//	beginInstructions = new[] {
					//		msil.Create(OpCodes.Ldarg_0),
					//		msil.Create(OpCodes.Call,propDef.GetMethod),
					//		msil.Create(OpCodes.Stloc_0)
					//	};

					//	//Pass propertyname, oldValue and newValue
					//	endInstructions = new[] {
					//		msil.Create(OpCodes.Ldarg_0),
					//		propertyName,
					//		msil.Create(OpCodes.Ldloc_0),
					//		msil.Create(OpCodes.Ldarg_1),
					//		msil.Create(OpCodes.Call, notifyTarget),
					//		msil.Create(OpCodes.Nop)
					//	};
					//	break;

					default:
						throw new InvalidNotifyTargetException(notifyTarget.FullName);
				}

				//Insert IL instructions before end of method body
				//Find all return statements in the method and raise notification there, this is a little more complicated.
				//...Also any statements that branch to them and make correction.
				var returnPoints = methodBody.Instructions.Where(a => a.OpCode == OpCodes.Ret).ToArray();
				foreach (var instruction in returnPoints) {

					InsertBefore(msil, endInstructions, instruction);
					var branches = methodBody.Instructions.Where(a => a.OpCode == OpCodes.Br_S && a.Operand == instruction).ToArray();
					var branchTarget = endInstructions[0];

					foreach (var b in branches) {
						b.Operand = branchTarget;
					}
				}

			}
		}

		protected static void InsertAfter(ILProcessor ilProcessor, IEnumerable<Instruction> instructions, Instruction startPoint) {
			var currentInstruction = startPoint;
			foreach (var instruction in instructions) {
				ilProcessor.InsertAfter(currentInstruction, instruction);
				currentInstruction = instruction;
			}
		}
		protected static void InsertBefore(ILProcessor ilProcessor, IEnumerable<Instruction> instructions, Instruction startPoint) {

			var currentInstruction = null as Instruction;
			foreach (var instruction in instructions) {
				if (currentInstruction == null) {
					ilProcessor.InsertBefore(startPoint, instruction);
				}
				else {
					ilProcessor.InsertAfter(currentInstruction, instruction);
				}
				currentInstruction = instruction;
			}
		}
	}
}