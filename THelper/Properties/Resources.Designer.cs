﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace THelper.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("THelper.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;Project ToolsVersion=&quot;4.0&quot; DefaultTargets=&quot;Build&quot; xmlns=&quot;http://schemas.microsoft.com/developer/msbuild/2003&quot;&gt;
        ///  &lt;PropertyGroup&gt;
        ///    &lt;Configuration Condition=&quot; &apos;$(Configuration)&apos; == &apos;&apos; &quot;&gt;Debug&lt;/Configuration&gt;
        ///    &lt;Platform Condition=&quot; &apos;$(Platform)&apos; == &apos;&apos; &quot;&gt;x86&lt;/Platform&gt;
        ///    &lt;ProductVersion&gt;8.0.30703&lt;/ProductVersion&gt;
        ///    &lt;SchemaVersion&gt;2.0&lt;/SchemaVersion&gt;
        ///    &lt;ProjectGuid&gt;{15A0B679-5A23-4E98-9BEC-E934DC0B8CF3}&lt;/ProjectGuid&gt;
        ///    &lt;OutputType&gt;WinExe&lt;/OutputType&gt;
        /// </summary>
        internal static string dxSampleGrid {
            get {
                return ResourceManager.GetString("dxSampleGrid", resourceCulture);
            }
        }
    }
}