# MIST
Implements change notification for properties (ie: INotifyPropertyChanged) using IL weaving and a custom Visual Studio build task.

This is how it works:

**The NuGet package now covers .NET framework versions 4.0-4.7.**

1. Create a WPF project in visual studio, Target any framework 4.0 - 4.7 (if there's any favorable response I'll set up other frameworks in the NuGet package, what I've done should mostly work for all of them)
2. In the package manager type the NuGet command: `install-package Mathtone.MIST`. Visual Studio will prompt you to reload the project.
3. This should set everything up, adding a reference to the Mathtone.MIST assembly (which only contains the attribute classes you will use to decorate your code), copying the Mathtone.MIST.Builder and Mono.Cecil (this is the framework that does the actual weaving of IL operations, a fine piece of work) assemblies to the NuGet package "tools" folder.  Additionally, it executes a nonthreatening script that modifies your project file and adds the post build task.

Using MIST from Nuget.org

1. Install MIST build components (available from nuget.org) in the project containing classes for which you would like notification to be automatically implemented.

Install-Package Mathtone.MIST

2. If you unload and Edit the project file, it should contain a build task similar to the following:The location of the Mathtone.MIST.Builder assembly is the default and can be changed by modifying your local Nuget.config file (If you install MIST via the nuget package, this should be handled for you).

&lt;UsingTask TaskName="Mathtone.MIST.NotificationWeaverBuildTask"      AssemblyFile="$(ProjectDir)..\packages\Mathtone.MIST.1.0.1\tools\Mathtone.MIST.Builder.dll" /&gt;
   &lt;Target Name="AfterBuild"&gt;
   &lt;NotificationWeaverBuildTask TargetPath="$(TargetPath)" DebugMode="True" /&gt;
&lt;/Target&gt;

Apply notification attributes

3. Use the "NotifierAttribute", "NotifyAttribute" and "NotifyTargetAttributte"

Typical usage scenario:

    public class NotifierBase : INotifyPropertyChanged {

		[NotifyTarget]
		protected void RaisePropertyChanged(string propertyName) {
			var method = PropertyChanged;
			if (method != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
    }

	[Notifier(NotificationMode.Implicit)]
	public class ImplicitTestNotifier : NotifierBase {

		public string Property1 { get; set; }

		public string Property2 { get; set; }

		public string Property3 { get; set; }

		[SupressNotify]
		public string Supressed { get; set; }

		[Notify("Prop1And2", "Property1", "Property2")]
		public string Prop1And2 { get; set; }
	}

	[Notifier]
	public class ExplicitTestNotifier : NotifierBase {

		[Notify]
		public string Property1 { get; set; }

		[Notify]
		public string Property2 { get; set; }

		[Notify]
		public string Property3 { get; set; }

		public string Supressed { get; set; }

		[Notify("Prop1And2", "Property1", "Property2")]
		public string Prop1And2 { get; set; }
	}

If it's all wired up correctly, you should see a message in the build output window: "Lightly Misting: &lt;your project output&gt;" at build time.

OnChange Notification:

Additionally, the MIST utility supports notification only when the value being set changes from a previous value.  This is controlled by setting the notifycation style to NotificationStyle.OnChange and this can be applied to the NotifierAttribute as a default, or the NotifyAttribute as an override.  This method respects all known conventions (at least those known to me) for the comparing of equality among value & reference types, including overridden "Equals" methods, etc:
        
       [Notifier(NotificationMode.Implicit, NotificationStyle.OnChange)]
	public class OnChangeTest : TestNotifierBase
	{
		public string StringValue { get; set; } = "";

		[Notify(NotificationStyle.OnSet)]
		public string ExplicitOnSetString { get; set; }
	}

Happy coding!
