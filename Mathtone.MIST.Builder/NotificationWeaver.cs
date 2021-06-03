using Mathtone.MIST.Builder.Processors;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mathtone.MIST.Builder {
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
			var pdbPath = FindPdbPathFor(assemblyPath);

			if (!File.Exists(pdbPath))
				throw new FileNotFoundException($"Assembly {assemblyPath} pdb file not found at '{pdbPath}'");

			var assemblyReadPath = CopyToTempFolder(assemblyPath, true);
			var pdbReadPath = CopyToTempFolder(pdbPath, true);
			//var assemblyReadPath = assemblyPath;
			//var pdbReadPath = pdbPath;

			try {
				InsertNotifications(assemblyReadPath, debug);
			}
			finally {
				try { File.Delete(assemblyReadPath); } catch { }
				try { File.Delete(pdbReadPath); } catch { }
				//
			}
		}

		private void InsertNotifications(string assemblyReadPath, bool debug) {

			var readParameters = new ReaderParameters { ReadSymbols = debug, AssemblyResolver = resolver };
			var writeParameters = new WriterParameters { WriteSymbols = debug };
			var assemblyProcessor = new AssemblyProcessor(mdResolver);

			//Load the assembly.
			using (var stream = File.OpenRead(assemblyReadPath)) {
				var assemblyDef = AssemblyDefinition.ReadAssembly(stream, readParameters);

				assemblyProcessor.Process(assemblyDef);

				//If the assembly has been altered then rewrite it.
				if (assemblyProcessor.ContainsChanges) {
					using (var outputStream = File.OpenWrite(assemblyPath)) {
						assemblyDef.Write(outputStream, writeParameters);
					}
				}
			}
		}

		private static string FindPdbPathFor(string assemblyPath) {

			return Path.GetDirectoryName(assemblyPath) + "\\" + Path.GetFileNameWithoutExtension(assemblyPath) + ".pdb";
		}

		private static string CopyToTempFolder(string path, bool overwrite = false) {
			var tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(path));

			File.Copy(path, tempPath, overwrite);

			return tempPath;
		}
	}
}