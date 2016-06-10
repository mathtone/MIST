using Mathtone.MIST.Processors;
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

			var assemblyDef = null as AssemblyDefinition;
			var readParameters = new ReaderParameters { ReadSymbols = debug, AssemblyResolver = resolver };
			var writeParameters = new WriterParameters { WriteSymbols = debug };
			var assemblyProcessor = new AssemblyProcessor(mdResolver);

			//Load the assembly.
			using (var stream = File.OpenRead(assemblyPath)) {
				assemblyDef = AssemblyDefinition.ReadAssembly(stream, readParameters);
			}

			assemblyProcessor.Process(assemblyDef);

			//If the assembly has been altered then rewrite it.
			if (assemblyProcessor.ContainsChanges) {
				using (var stream = File.OpenWrite(assemblyPath)) {
					assemblyDef.Write(stream, writeParameters);
					stream.Flush();
				}
			}
		}
	}
}