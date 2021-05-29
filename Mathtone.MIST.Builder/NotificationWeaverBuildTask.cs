using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathtone.MIST {

	/// <summary>
	/// Class NotificationWeaverBuildTask.
	/// </summary>
	/// <example>
	/// place the following XML in the project file.  The directorey containing Mathtone.MIST.Builder.dll should also contain Mathtone.MIST.dll, Mono.Cecil.dll and Mono.Cecil.pdb.dll
	/// <UsingTask TaskName = "Mathtone.MIST.NotificationWeaverBuildTask"
	///		 AssemblyFile="...path to builder assembly"		 
	/// />
	/// <Target Name = "AfterBuild" >
	///		<NotificationWeaverBuildTask TargetPath="$(TargetPath)" DebugMode="True"/>
	/// </Target>
	/// </example>
	public class NotificationWeaverBuildTask : Task {

		/// <summary>
		/// Gets or sets the target path.
		/// </summary>
		/// <value>The target path.</value>
		[Required]
		public string TargetPath { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [debug mode].
		/// </summary>
		/// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
		[Required]
		public bool DebugMode { get; set; }

		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>true if the task successfully executed; otherwise, false.</returns>
		public override bool Execute() {
			Log.LogMessage(MessageImportance.High, String.Format(Resources.BuildTaskMessage, TargetPath));
			new NotificationWeaver(TargetPath).InsertNotifications(DebugMode);
			return true;
		}
	}
}