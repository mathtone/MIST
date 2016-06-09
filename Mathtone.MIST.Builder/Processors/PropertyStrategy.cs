using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.Processors {
	class PropertyStrategy : ImplementationStrategy {

		static OpCode[] simpleInstructions = new[] {
			OpCodes.Ldarg_0,
			OpCodes.Ldarg_1,
			OpCodes.Stfld,
			OpCodes.Ret
		};

		public PropertyStrategy(PropertyDefinition definition, NotificationMode mode,
			NotificationStyle defaultStyle, MethodReference target) {

			this.Property = definition;
			this.NotifyValues = GetNotifyPropertyNames(definition, mode).ToArray();
			this.IsIgnored = !NotifyValues.Any() || Property.SetMethod == null;

			if (!this.IsIgnored) {
				this.NotifyTarget = target;
				this.NotifyTargetDefinition = target.Resolve();
				this.NotificationStyle = GetNotificationStyle(definition) ?? defaultStyle;
				this.ImplementationStyle = this.NotificationStyle == NotificationStyle.OnChange || IsComplex(definition) ? ImplementationStyle.Wrapped : ImplementationStyle.Inline;
			}
		}

		/// <summary>
		/// Gets the property names that should be passed to the notification target method when the property value is changed.
		/// </summary>
		/// <param name="property">The property definition.</param>
		/// <returns>IEnumerable&lt;System.String&gt;.</returns>
		static IEnumerable<string> GetNotifyPropertyNames(PropertyDefinition property, NotificationMode mode) {

			//Check for Supression
			var suppress = property.GetAttribute(typeof(SuppressNotifyAttribute));

			if (suppress == null) {

				var notify = property.GetAttribute(typeof(NotifyAttribute));

				if (notify == null && mode == NotificationMode.Implicit) {
					yield return property.Name;
				}
				else if (notify != null) {

					//Return property names supplied by the constructor, if none are specified return the property name itself.
					if (notify.HasConstructorArguments) {
						var args = notify.ConstructorArguments.FirstOrDefault(a => a.Type.FullName == typeof(string[]).FullName).Value as CustomAttributeArgument[];

						if (args == null) {
							yield return null;
						}
						else if (args.Length == 0) {
							yield return property.Name;
						}
						else {
							foreach (var arg in args) {
								yield return (string)arg.Value;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the notification style for the supplied property.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <returns>System.Nullable&lt;NotificationStyle&gt;., if specifiedm otherwise returns default style</returns>
		static NotificationStyle? GetNotificationStyle(PropertyDefinition definition) {

			var attribute = definition.CustomAttributes.FirstOrDefault(
				a => a.AttributeType.FullName == typeof(NotifyAttribute).FullName
			);

			if (attribute != null && attribute.HasConstructorArguments) {
				var style = attribute.ConstructorArguments.SingleOrDefault(a => a.Type.FullName == typeof(NotificationStyle).FullName).Value;
				if (style == null) {
					return null;
				}
				else {
					return (NotificationStyle)style;
				}

			}
			else {
				return null;
			}
		}

		/// <summary>
		/// Determines whether the specified property is complex.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <returns><c>true</c> if the specified property is complex; otherwise, <c>false</c>.</returns>
		static bool IsComplex(PropertyDefinition property) =>
			property.SetMethod.IsAbstract ||
			!property.SetMethod.Body.Instructions.Select(a => a.OpCode)
				.SequenceEqual(simpleInstructions);
	}
}