using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Mathtone.MIST.Processors {

	public class ModuleProcessor : IDefinitionProcessor<ModuleDefinition> {
		MetadataResolver metadataResolver;
		public bool ContainsChanges { get; protected set; }

		public ModuleProcessor(MetadataResolver metadataResolver) {
			this.metadataResolver = metadataResolver;
		}

		public void Process(ModuleDefinition definition) {
			var processor = new TypeProcessor(metadataResolver);
			foreach (var typeDef in definition.Types) {
				try {
					processor.Process(typeDef);
				}
				catch (Exception ex) {
					throw new BuildTaskErrorException(typeDef.FullName, ex);
				}
			}
			ContainsChanges |= processor.ContainsChanges;
		}
	}
}