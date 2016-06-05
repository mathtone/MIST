using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mathtone.MIST {
	/// <summary>
	/// Alters IL assemblies after build and implements a notification mechanism.
	/// </summary>
	public class NotificationWeaver {

		string NotifyTargetName = typeof(NotifyTarget).FullName;
		string NotifierTypeName = typeof(NotifierAttribute).FullName;
		string NotifyTypeName = typeof(NotifyAttribute).FullName;
        string NotifyModeName = typeof(NotifyMode).FullName;
		string SuppressNotifyTypeName = typeof(SuppressNotifyAttribute).FullName;
		string assemblyPath;
		DefaultAssemblyResolver resolver;
		MetadataResolver mdResolver;

		string ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
                foreach(var propDef in propertiesToNotifyForIn(typeDef, mode))
                {
                    var propNames = GetNotifyPropertyNames(propDef);
                    var notifyMode = NotifyModeOf(propDef);
                    InsertNotificationsIntoProperty(propDef, notifyTarget, propNames, notifyMode);
                    rtn = true;
                }
			}

			//Recursively process any nested type definitions.
			foreach (var type in typeDef.NestedTypes) {
				ProcessType(type);
			}

			return rtn;
		}

        private NotifyMode NotifyModeOf(PropertyDefinition propDef)
        {
            var attr = propDef.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == NotifyTypeName);
            if (attr == null || !attr.HasConstructorArguments)
                return NotifyMode.OnSet;

            if (!attr.ConstructorArguments.Where(x => x.Type.FullName == NotifyModeName).Any())
                return NotifyMode.OnSet;

            return (NotifyMode)attr.ConstructorArguments
                .Where(x => x.Type.FullName == NotifyModeName)
                .Select(x => x.Value)
                .Single();
        }

        private IEnumerable<PropertyDefinition> propertiesToNotifyForIn(TypeDefinition typeDef, NotificationMode mode)
        {
            foreach (var propDef in typeDef.Properties)
            {
                if (ContainsAttribute(propDef, SuppressNotifyTypeName))
                    continue;

                if (ContainsAttribute(propDef, NotifyTypeName) || mode == NotificationMode.Implicit)
                    yield return propDef;
            }
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
        IEnumerable<string> GetNotifyPropertyNames(PropertyDefinition propDef)
        {
            var attr = propDef.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == NotifyTypeName);
            if (attr == null || !attr.HasConstructorArguments)
            {
                //No fancy stuff, just return the property name.
                yield return propDef.Name;
            }
            else
            {
                //Return property names supplied by the constructor, if none are specified return the property name itself.
                var args = attr.ConstructorArguments[0].Value as CustomAttributeArgument[];
                if (args == null)
                {
                    //Argument is null
                    yield return null;
                }
                else if (args.Length == 0)
                {
                    //Apparently the user saw reason to pass an empty array.
                    yield return propDef.Name;
                }
                else
                {
                    //Multiple arguments have been passed.
                    foreach (var arg in args)
                    {
                        yield return (string)arg.Value;
                    }
                }
            }
        }

		/// <summary>
		/// Weaves notifiers into the property.  This is where the magic happens.
		/// </summary>
		/// <param name="propDef">The property definition.</param>
		/// <param name="notifyTarget">The notify target.</param>
		/// <param name="notifyPropertyNames">The notify property names.</param>
		protected static void InsertNotificationsIntoProperty(PropertyDefinition propDef, MethodReference notifyTarget, IEnumerable<string> notifyPropertyNames, NotifyMode notifyMode) {

            if (propDef.SetMethod == null)
				//This is a read-only property
				return;
			else if (propDef.SetMethod.Body == null) {
				//This is an abstract property, we don't do these either.
				throw new InvalidNotifierException();
			}

            if (IsAutoProperty(propDef))
            {
                RewriteAsAutoPropertyWithNotifications(propDef, notifyTarget, notifyPropertyNames, notifyMode);
                return;
            }

            var methodBody = propDef.SetMethod.Body;

			//Retrieve an IL writer
			var msil = methodBody.GetILProcessor();

            //Insert a Nop before the first instruction (like... at the beginning).
            var begin = msil.Create(OpCodes.Nop);
            msil.InsertBefore(methodBody.Instructions[0], begin);

            //Call the notification target method for 
            foreach (var notifyPropertyName in notifyPropertyNames) {

                //Emit a call to the notify target
                var beginInstructions = BeginInstructionsToCallMethodFrom(msil, propDef, notifyTarget).ToArray();
                var endInstructions = EndInstructionsToCallMethod(msil, notifyTarget, notifyPropertyName).ToArray();

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

        protected static IEnumerable<Instruction> BeginInstructionsToCallMethodFrom(ILProcessor msil, PropertyDefinition propDef, MethodReference method)
        {
            switch (method.Parameters.Count)
            {
                case 0:
                case 1:
                case 2:
                    break;

                //case 3:
                //    //this one is a little more complicated
                //    //Create a local variable and set it to the current value of the property.
                //    var variableType = propDef.SetMethod.Parameters[0].ParameterType;
                //    var variableDef = new VariableDefinition($"f__{propDef.Name}_temp", variableType);
                //    propDef.SetMethod.Body.Variables.Add(variableDef);
                //    yield return msil.Create(OpCodes.Ldarg_0);
                //    yield return msil.Create(OpCodes.Call, propDef.GetMethod);
                //    yield return msil.Create(OpCodes.Stloc_0);
                //    break;

                default:
                    throw new InvalidNotifyTargetException(method.FullName);
            }

            yield break;
        }

        protected static IEnumerable<Instruction> EndInstructionsToCallMethod(ILProcessor msil, MethodReference method, string parameterName)
        {
            var methodCallInstruction = msil.Create(OpCodes.Call, method);

            var loadParameterNameInstruction = parameterName == null
                ? msil.Create(OpCodes.Ldnull)
                : msil.Create(OpCodes.Ldstr, parameterName);

            switch (method.Parameters.Count)
            {
                case 0:
                    yield return msil.Create(OpCodes.Ldarg_0);
                    yield return methodCallInstruction;
                    yield return msil.Create(OpCodes.Nop);
                    break;

                case 1:
                    yield return msil.Create(OpCodes.Ldarg_0);
                    yield return loadParameterNameInstruction;
                    yield return methodCallInstruction;
                    yield return msil.Create(OpCodes.Nop);
                    break;

                //This works, but allowing this simply create too many questions.  Eliminating these options in favor of simplicity.
                //In the future I will 
                //case 2:
                //  yield return msil.Create(OpCodes.Ldarg_0);
                //  yield return loadParameterNameInstruction;
                //  yield return msil.Create(OpCodes.Ldarg_1);
                //  yield return methodCallInstruction;
                //  yield return msil.Create(OpCodes.Nop);
                //	break;
                
                //case 3:
				//	//Pass propertyname, oldValue and newValue
                //  yield return msil.Create(OpCodes.Ldarg_0);
                //  yield return loadParameterNameInstruction;
                //  yield return msil.Create(OpCodes.Ldloc_0);
                //  yield return msil.Create(OpCodes.Ldarg_1);
                //  yield return methodCallInstruction;
                //  yield return msil.Create(OpCodes.Nop);
                //	break;

                default:
                    throw new InvalidNotifyTargetException(method.FullName);
            }
        }

        protected static bool IsAutoProperty(PropertyDefinition propDef)
        {
            if (AutoPropertyBackingFieldOf(propDef) == null)
                return false;
            return ReturnPointsOf(propDef).Count() == 1;
        }

        protected static IEnumerable<Instruction> ReturnPointsOf(PropertyDefinition propDef)
        {
            return propDef.SetMethod.Body
                .Instructions
                .Where(a => a.OpCode == OpCodes.Ret);
        }

        protected static FieldDefinition AutoPropertyBackingFieldOf(PropertyDefinition propDef)
        {
            var autoPropertyBackingFieldName = $"<{propDef.Name}>k__BackingField";
            var autoPropertyBackingField = propDef.DeclaringType.Fields
                .SingleOrDefault(field => field.Name == autoPropertyBackingFieldName);
            return autoPropertyBackingField;
        }

        protected static void RewriteAsAutoPropertyWithNotifications(PropertyDefinition propDef, MethodReference notifyTarget, IEnumerable<string> notifyPropertyNames, NotifyMode notifyMode)
        {
            var autoPropertyBackingField = AutoPropertyBackingFieldOf(propDef);
            var returnPoint = ReturnPointsOf(propDef).Single();

            var methodBody = propDef.SetMethod.Body;
            var msil = methodBody.GetILProcessor();

            var nopInstruction = msil.Create(OpCodes.Nop);
            msil.InsertBefore(methodBody.Instructions[0], nopInstruction);

            if (notifyMode == NotifyMode.OnChange)
            {
                var instructionsToBranchToReturn = BranchIfNotChangedInstructions(propDef, autoPropertyBackingField, returnPoint, msil);
                InsertAfter(msil, instructionsToBranchToReturn, nopInstruction);
            }

            var notificationsInstructions = new List<Instruction>();
            foreach (var propertyName in notifyPropertyNames)
            {
                notificationsInstructions.AddRange(EndInstructionsToCallMethod(msil, notifyTarget, propertyName));
            }
            InsertBefore(msil, notificationsInstructions, returnPoint);
        }

        protected static Instruction[] BranchIfNotChangedInstructions(PropertyDefinition propDef, FieldDefinition autoPropertyBackingField, Instruction returnPoint, ILProcessor msil)
        {
            var stringInequality = propDef.Module.ImportReference(typeof(string).GetMethod("op_Inequality"));
            return new[]
            {
                msil.Create(OpCodes.Ldarg_0),
                msil.Create(OpCodes.Ldfld, autoPropertyBackingField),
                msil.Create(OpCodes.Ldarg_1),
                msil.Create(OpCodes.Call, stringInequality),
                msil.Create(OpCodes.Brfalse_S, returnPoint),
            };
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