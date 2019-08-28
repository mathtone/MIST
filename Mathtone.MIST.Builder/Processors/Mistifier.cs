using Mono.Cecil;
using System;
using System.IO;
using System.Reflection;

namespace Mathtone.MIST.Processors {
	public class Mistifier : IDisposable {

		readonly string assemblyPath;
		readonly DefaultAssemblyResolver assemblyresolver;
		readonly MetadataResolver metadataResolver;

		string ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public Mistifier(string assemblyPath) {
			this.assemblyPath = assemblyPath;
			this.assemblyresolver = new DefaultAssemblyResolver();
			this.assemblyresolver.AddSearchDirectory(ApplicationPath);
			this.assemblyresolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
			this.metadataResolver = new MetadataResolver(assemblyresolver);
		}

		public void Dispose() {
			assemblyresolver.Dispose();
		}

		public void Process(bool debug = false) {

			var readParameters = new ReaderParameters { ReadSymbols = debug, AssemblyResolver = assemblyresolver, ReadWrite = true };
			var writeParameters = new WriterParameters { WriteSymbols = debug };
			var assemblyProcessor = new AssemblyProcessor(metadataResolver);

			//Load the assembly.

			using (var assemblyDef = AssemblyDefinition.ReadAssembly(assemblyPath, readParameters)) {
				assemblyProcessor.Process(assemblyDef);

				//If the assembly has been altered then rewrite it.
				if (assemblyProcessor.ContainsChanges) {
					assemblyDef.Write(writeParameters);
				}
			}
		}
	}
}
