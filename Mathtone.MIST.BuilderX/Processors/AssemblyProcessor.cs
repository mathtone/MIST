﻿using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathtone.MIST.Processors {

	public class AssemblyProcessor : IDefinitionProcessor<AssemblyDefinition> {
		MetadataResolver metadataResolver;

		public bool ContainsChanges { get; protected set; }

		public AssemblyProcessor(MetadataResolver metadataResolver) {
			this.metadataResolver = metadataResolver;
		}

		public void Process(AssemblyDefinition definition) {
			if (IsMisted(definition)) {
				return;
			}

			var moduleProcessor = new ModuleProcessor(metadataResolver);

			foreach (var moduleDef in definition.Modules) {
				moduleProcessor.Process(moduleDef);
			}

			this.ContainsChanges = moduleProcessor.ContainsChanges;

			if (this.ContainsChanges) {
				AddImplementationSummary(definition);
			}
		}

		void AddImplementationSummary(AssemblyDefinition assemblyDefinition) {
			var reference = assemblyDefinition.MainModule.ImportReference(typeof(MistedAssemblyAttribute));
			var definition = reference.Resolve();
			var constructor = definition.Methods.FirstOrDefault(a => a.IsConstructor);
			var attribute = new CustomAttribute(assemblyDefinition.MainModule.ImportReference(constructor));
			assemblyDefinition.CustomAttributes.Add(attribute);
		}

		protected bool IsMisted(AssemblyDefinition definition) {
			var attr = GetAttribute(definition, typeof(MistedAssemblyAttribute));
			return (attr != null);
		}

		public CustomAttribute GetAttribute(AssemblyDefinition definition, Type attributeType) =>
			definition.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == attributeType.FullName);
	}
}
