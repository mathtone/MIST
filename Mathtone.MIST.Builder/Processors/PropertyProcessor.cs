using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mathtone.MIST;
using Mono.Cecil.Cil;
using System.Reflection;

namespace Mathtone.MIST.Processors {

	public class PropertyProcessor : IDefinitionProcessor<PropertyDefinition> {

		NotificationMode defaultMode;
		NotificationStyle defaultStyle;
		MethodReference notifyTarget;
		
		//static MethodInfo equalsMethod = typeof(object).GetMethods().FirstOrDefault(a => a.Name == "Equals" && !a.IsStatic);
		public bool ContainsChanges { get; protected set; }

		public PropertyProcessor(MethodReference target, NotificationMode mode, NotificationStyle style) {
			this.notifyTarget = target;
			this.defaultMode = mode;
			this.defaultStyle = style;
		}

		public void Process(PropertyDefinition property) {
			var strategy = new PropertyStrategy(property, defaultMode, defaultStyle, notifyTarget);
			//var strategy = GetStrategy(property, defaultMode, notifyTarget);
			if (!strategy.IsIgnored) {
				ImplementStrategy(strategy);
			}
		}

		void ImplementStrategy(ImplementationStrategy strategy) {
			//Just in case...
			if (strategy.IsIgnored) {
				return;
			}
			if (strategy.Property.SetMethod.IsAbstract) {
				//This is an abstract property, we don't do these.
				throw new InvalidNotifierException();
			}

			switch (strategy.ImplementationStyle) {
				case ImplementationStyle.Inline:
					ImplementInline(strategy);
					break;
				case ImplementationStyle.Wrapped:
					ImplementWrapped(strategy);
					break;
				default: throw new NotImplementedException();
			}
			ContainsChanges = true;
		}

		static void ImplementInline(ImplementationStrategy strategy) {

			var method = strategy.Property.SetMethod;
			var msil = method.Body.GetILProcessor();
			var begin = method.Body.Instructions.First();
			var end = method.Body.Instructions.Last();

			if (strategy.NotificationStyle == NotificationStyle.OnSet) {
				InsertBefore(msil, msil.Create(OpCodes.Nop), begin);
				InsertBefore(msil, CallNotifyTargetInstructions(msil, strategy), end);
			}
			else {
				throw new BuildTaskErrorException("Inline implementation does not support OnChange notification");
			}
		}

		static IEnumerable<Instruction> CallNotifyTargetInstructions(ILProcessor ilProcessor, ImplementationStrategy strategy) {
			foreach (var name in strategy.NotifyValues) {

				yield return ilProcessor.Create(OpCodes.Ldarg_0);


				if (strategy.NotifyTarget.Parameters.Count > 0) {
					yield return ilProcessor.Create(OpCodes.Ldstr, name);
				}
				if (strategy.NotifyTarget.Parameters.Count > 1) {
					if (strategy.Property.PropertyType.IsValueType) {
						yield return ilProcessor.Create(OpCodes.Box,strategy.NotifyTarget.Parameters[1].ParameterType);
					}
					yield return ilProcessor.Create(OpCodes.Ldarg_1);
				}
				var opCode = strategy.NotifyTargetDefinition.IsVirtual ?
						OpCodes.Callvirt : OpCodes.Call;
				yield return ilProcessor.Create(opCode, strategy.NotifyTarget);
				yield return ilProcessor.Create(OpCodes.Nop);
			}
		}

		static void ImplementWrapped(ImplementationStrategy strategy) {

			var setMethod = strategy.Property.SetMethod;
			var newMethod = new MethodDefinition(setMethod.Name, setMethod.Attributes, setMethod.ReturnType);
			var msil = newMethod.Body.GetILProcessor();
			var instructions = new List<Instruction>();
			var rtn = msil.Create(OpCodes.Ret);
			newMethod.DeclaringType = setMethod.DeclaringType;
			newMethod.Parameters.Add(new ParameterDefinition(setMethod.Parameters[0].ParameterType));

			if (strategy.NotificationStyle == NotificationStyle.OnChange) {

				var boolType = strategy.Property.Module.ImportReference(typeof(bool));
				

				var propertyType = strategy.Property.PropertyType.Resolve();

				//find equals method
				var equalsMethod = SeekMethod(
					propertyType,
					a =>
						a.Name == "Equals" &&
						a.Parameters.Count == 1
				);
				var equality = strategy.Property.Module.ImportReference(equalsMethod);

				var equalityReference = equalsMethod.Resolve();

				var v1 = new VariableDefinition(strategy.Property.PropertyType);
				var v2 = new VariableDefinition(boolType);
				var v3 = new VariableDefinition(boolType);

				newMethod.Body.Variables.Add(v1);
				newMethod.Body.Variables.Add(v2);
				newMethod.Body.Variables.Add(v3);

				if (!propertyType.IsPrimitive) {
					instructions.AddRange(
						new[] {
							msil.Create(OpCodes.Nop),
							msil.Create(OpCodes.Ldarg_0),
							msil.Create(OpCodes.Call,strategy.Property.GetMethod),
							msil.Create(OpCodes.Stloc_0),
							msil.Create(OpCodes.Ldarg_0),
							msil.Create(OpCodes.Ldarg_1),
							msil.Create(OpCodes.Call, setMethod),
							msil.Create(OpCodes.Nop),
							msil.Create(OpCodes.Ldloc_0),
							msil.Create(OpCodes.Ldarg_1),
							msil.Create(equalityReference.IsVirtual? OpCodes.Callvirt:OpCodes.Call,equality),
							msil.Create(OpCodes.Stloc_1),
							msil.Create(OpCodes.Ldloc_1),
							msil.Create(OpCodes.Brfalse_S,rtn),
							msil.Create(OpCodes.Nop)
						}
					);
				}
				else {
					instructions.AddRange(
						new[] {
							msil.Create(OpCodes.Nop),
							msil.Create(OpCodes.Ldarg_0),
							msil.Create(OpCodes.Call,strategy.Property.GetMethod),
							msil.Create(OpCodes.Stloc_0),
							msil.Create(OpCodes.Ldarg_0),
							msil.Create(OpCodes.Ldarg_1),
							msil.Create(OpCodes.Call, setMethod),
							msil.Create(OpCodes.Nop),
							msil.Create(OpCodes.Ldloc_0),
							msil.Create(OpCodes.Ldarg_1),
							msil.Create(OpCodes.Call,equality),
							msil.Create(OpCodes.Ldc_I4_0),
							msil.Create(OpCodes.Ceq),
							msil.Create(OpCodes.Stloc_1),
							msil.Create(OpCodes.Ldloc_1),
							msil.Create(OpCodes.Brfalse_S,rtn),
							msil.Create(OpCodes.Nop)
						}
					);
				};

				instructions.AddRange(CallNotifyTargetInstructions(msil, strategy));
				instructions.Add(msil.Create(OpCodes.Nop));
				instructions.Add(rtn);

			}
			else {
				instructions.AddRange(new[] {
					msil.Create(OpCodes.Ldarg_0),
					msil.Create(OpCodes.Ldarg_1),
					msil.Create(OpCodes.Call, setMethod),
				});
				instructions.AddRange(CallNotifyTargetInstructions(msil, strategy));
				instructions.Add(rtn);
			}

			foreach (var instruction in instructions) {
				newMethod.Body.Instructions.Add(instruction);
			}
			setMethod.Name = $"{setMethod.Name}`mist"; // "misted`" + setMethod.Name;
			strategy.Property.SetMethod = newMethod;
			newMethod.DeclaringType.Methods.Add(newMethod);
		}

		static IEnumerable<MethodDefinition> GetMethods(TypeDefinition type, Func<MethodDefinition, bool> evaluator) =>
			type.Methods.Where(a => evaluator(a));

		static MethodDefinition SeekMethod(TypeDefinition type, Func<MethodDefinition, bool> evaluator) {

			var rtn = GetMethods(type, evaluator).FirstOrDefault();
			if (rtn == null && type.BaseType != null) {
				rtn = SeekMethod(type.BaseType.Resolve(), evaluator);
			}
			return rtn;
			//MethodDefinition rtn;

		}

		static void InsertAfter(ILProcessor ilProcessor, IEnumerable<Instruction> instructions, Instruction insertionPoint) {
			var currentInstruction = insertionPoint;
			foreach (var instruction in instructions) {
				ilProcessor.InsertAfter(currentInstruction, instruction);
				currentInstruction = instruction;
			}
		}

		static void InsertAfter(ILProcessor ilProcessor, Instruction instruction, Instruction insertionPoint) =>
			InsertAfter(ilProcessor, new[] { instruction }, insertionPoint);

		static void InsertBefore(ILProcessor ilProcessor, Instruction instruction, Instruction insertionPoint) =>
			InsertBefore(ilProcessor, new[] { instruction }, insertionPoint);

		static void InsertBefore(ILProcessor ilProcessor, IEnumerable<Instruction> instructions, Instruction insertionPoint) {
			var currentInstruction = null as Instruction;
			foreach (var instruction in instructions) {
				if (currentInstruction == null) {
					ilProcessor.InsertBefore(insertionPoint, instruction);
				}
				else {
					ilProcessor.InsertAfter(currentInstruction, instruction);
				}
				currentInstruction = instruction;
			}
		}
	}
}