using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.Processors {
	public class TypeProcessor {
		MetadataResolver metadataResolver;
		public bool ContainsChanges { get; protected set; }

		public TypeProcessor(MetadataResolver metadataResolver) {
			this.metadataResolver = metadataResolver;
		}

		/// <summary>
		/// Weaves the notification mechanism into the supplied type.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <returns><c>true</c> if the type was altered, <c>false</c> otherwise.</returns>
		/// <exception cref="System.Exception"></exception>
		public void Process(TypeDefinition typeDef) {

			var notifierAttr = typeDef.GetAttribute(typeof(NotifierAttribute));
			var summaryAttr = typeDef.GetAttribute(typeof(ImplementationSummaryAttribute));

			if (summaryAttr == null && notifierAttr != null) {

				typeDef.CustomAttributes.Remove(notifierAttr); //Make sure we only ever MIST a property ONCE (mostly useful for unit tests)

				//Use explicit mode if not otherwise specified
				var mode = NotificationMode.Explicit;
				var style = NotificationStyle.OnSet;

				//Locate the notification target method.
				var notifyTarget = GetNotifyTarget(typeDef);
				if (notifyTarget == null) {
					throw new CannotLocateNotifyTargetException(typeDef.FullName);
				}

				//Determine whether to use explicit/implicit notifier identification.
				if (notifierAttr.HasConstructorArguments) {
					mode = (NotificationMode)notifierAttr.ConstructorArguments.FirstOrDefault(a => a.Type.FullName == typeof(NotificationMode).FullName).Value;
					style = (NotificationStyle)notifierAttr.ConstructorArguments.FirstOrDefault(a => a.Type.FullName == typeof(NotificationStyle).FullName).Value;
				}

				var processor = new PropertyProcessor(notifyTarget, mode, style);
				foreach (var property in typeDef.Properties) {
					processor.Process(property);
				}

				ContainsChanges |= processor.ContainsChanges;
			}

			//Recursively process any nested type definitions.
			foreach (var type in typeDef.NestedTypes) {
				Process(type);
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
				if (methDef.ContainsAttribute(typeof(NotifyTarget))) {
					var isValid = false;
					switch (methDef.Parameters.Count) {
						case 0:
							isValid = true;
							break;
						case 1:
							isValid = methDef.Parameters[0].ParameterType.FullName == typeof(string).FullName;
							break;
						case 2:
							isValid = methDef.Parameters[0].ParameterType.FullName == typeof(string).FullName &&
								methDef.Parameters[1].ParameterType.FullName == typeof(object).FullName;
							break;
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
				var baseTypeDef = metadataResolver.Resolve(baseType);

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
	}
}