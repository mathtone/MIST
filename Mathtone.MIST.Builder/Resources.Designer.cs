﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mathtone.MIST.Builder {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Mathtone.MIST.Builder.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Errors encountered processing type: {0}.
        /// </summary>
        internal static string BuildTaskError {
            get {
                return ResourceManager.GetString("BuildTaskError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lightly misting {0}.
        /// </summary>
        internal static string BuildTaskMessage {
            get {
                return ResourceManager.GetString("BuildTaskMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot locate notify target for type.
        /// </summary>
        internal static string CannotLocateNotifyTarget {
            get {
                return ResourceManager.GetString("CannotLocateNotifyTarget", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Notify target {0} is not an Action&lt;string&gt;.
        /// </summary>
        internal static string InvalidNotifyTarget {
            get {
                return ResourceManager.GetString("InvalidNotifyTarget", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NotifyAttribute cannot be set on abstract properties.
        /// </summary>
        internal static string NotifyAttributeCannotBeSet {
            get {
                return ResourceManager.GetString("NotifyAttributeCannotBeSet", resourceCulture);
            }
        }
    }
}
