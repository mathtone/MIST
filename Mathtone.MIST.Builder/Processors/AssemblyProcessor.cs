using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.Processors {

	public class AssemblyProcessor : IDefinitionProcessor<AssemblyDefinition> {
		MetadataResolver metadataResolver;

		public bool ContainsChanges { get; protected set; }

		public AssemblyProcessor(MetadataResolver metadataResolver) {
			this.metadataResolver = metadataResolver;
		}

		public void Process(AssemblyDefinition definition) {
			//Search for types and weave notifiers into them if necessary.
			var moduleProcessor = new ModuleProcessor(metadataResolver);
			foreach (var moduleDef in definition.Modules) {
				moduleProcessor.Process(moduleDef);
			}
			this.ContainsChanges = moduleProcessor.ContainsChanges;
		}
	}
}