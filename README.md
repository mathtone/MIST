# MIST
Implements change notification for properties (ie: INotifyPropertyChanged) using IL weaving and a custom Visual Studio build task.

This is how it works:

**The NuGet package now covers .NET framework versions 4.0-4.6+.**

1. Create a WPF project in visual studio, Target any framework 4.0 - 4.6.2 (if there's any favorable response I'll set up other frameworks in the NuGet package, what I've done should mostly work for all of them)
2. In the package manager type the NuGet command: `install-package Mathtone.MIST`. Visual Studio will prompt you to reload the project.
3. This should set everything up, adding a reference to the Mathtone.MIST assembly (which only contains the attribute classes you will use to decorate your code), copying the Mathtone.MIST.Builder and Mono.Cecil (this is the framework that does the actual weaving of IL operations, a fine piece of work) assemblies to the NuGet package "tools" folder.  Additionally, it executes a nonthreatening script that modifies your project file and adds the post build task.

Here is an example:

```c#
    using Mathtone.MIST;

    namespace Lightly.Misted {
    
    	//THe "Notifier" attribute indicates that notification should be implemented for this class.
    	//Using explicit (default) notification
    	[Notifier]
    	public class ViewModel {
    		
    		//Raises the default notification ("WillNotify")
    		[Notify]
    		public string WillNotify { get; set; }
    
    		[Notify]
    		public string WillAlsoNotify { get; set; }
    
    		//Notification will not be implemented
    		public string WontNotify { get; set; }
    
    		//Raises notification for the "DefinitelyNotAlias" property 
    		[Notify("DefinitelyNotAlias")]
    		public string AliasNotify { get; set; }
    
    		//Raises multiple notification events
    		[Notify("WillNotify","ComplexNotify")]
    		public string ComplexNotify { get; set; }
    
    		//Notification target, any method visible to the notifying property.
    		//Can be implemented in a base class.
    		[NotifyTarget]
    		protected void PropertyChanged(string propertyName) {
    			//up to you.
    		}
    	}
    
    	//Implicit notification will implement notification for all settable properties, unless they are explicitly excluded.
    	[Notifier(NotificationMode.Implicit)]
    	public class ViewModel2 {
    
    		public string WillNotify { get; set; }
    		public string WillAlsoNotify { get; set; }
    
    		[SuppressNotify]
    		public string WontNotify { get; set; }
    		[Notify("DefinitelyNotAlias")]
    		public string AliasNotify { get; set; }
    		[Notify("WillNotify", "ComplexNotify")]
    		public string ComplexNotify { get; set; }

            [NotifyTarget]
            protected void PropertyChanged(string propertyName) {
                //up to you.
            }
    	}
    }

The notify target method can be implemented in a base class.  One thing I would mention is that when using implicit notification,
I would recommend suppressing notification for properties you have implemented.  
Most of the time this will work, but officially I have to plead "no soportado."
