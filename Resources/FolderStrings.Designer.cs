﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Quantumart.QP8.Resources {
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
    public class FolderStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal FolderStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Quantumart.QP8.Resources.FolderStrings", typeof(FolderStrings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Basic Parameters.
        /// </summary>
        public static string BasicParameters {
            get {
                return ResourceManager.GetString("BasicParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot delete the Root Folder.
        /// </summary>
        public static string CanDeleteRootFolder {
            get {
                return ResourceManager.GetString("CanDeleteRootFolder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &gt;.
        /// </summary>
        public static string CannotRenameNonEmptyFolder {
            get {
                return ResourceManager.GetString("CannotRenameNonEmptyFolder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot update the Root Folder.
        /// </summary>
        public static string CanUpdateRootFolder {
            get {
                return ResourceManager.GetString("CanUpdateRootFolder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Folder &apos;{0}&apos; is not empty.  Do you still want to remove this folder?.
        /// </summary>
        public static string FolderIsNotEmptyConfirm {
            get {
                return ResourceManager.GetString("FolderIsNotEmptyConfirm", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Folder not found in database (Id = {0}).
        /// </summary>
        public static string FolderNotFound {
            get {
                return ResourceManager.GetString("FolderNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Folder Name.
        /// </summary>
        public static string Name {
            get {
                return ResourceManager.GetString("Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid folder name.
        /// </summary>
        public static string NameInvalidFormat {
            get {
                return ResourceManager.GetString("NameInvalidFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The length of folder name should not exceed {1} characters.
        /// </summary>
        public static string NameMaxLengthExceeded {
            get {
                return ResourceManager.GetString("NameMaxLengthExceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Folder name is already in using in the parent folder.
        /// </summary>
        public static string NameNonUnique {
            get {
                return ResourceManager.GetString("NameNonUnique", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter folder name.
        /// </summary>
        public static string NameNotEntered {
            get {
                return ResourceManager.GetString("NameNotEntered", resourceCulture);
            }
        }
    }
}
