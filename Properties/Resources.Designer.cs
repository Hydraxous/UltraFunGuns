﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UltraFunGuns.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("UltraFunGuns.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to To add custom voxel textures, place images in this folder. Then press refresh custom voxels.
        ///Please note:
        ///- supported filetypes are .png .jpeg and .jpg
        ///- non-square images are not supported
        ///- custom transparent textures arent supported yet.
        ///- changing the name of the image in the folder may break save files that use it..
        /// </summary>
        public static string customvoxels_readme {
            get {
                return ResourceManager.GetString("customvoxels_readme", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] UltraFunGuns {
            get {
                object obj = ResourceManager.GetObject("UltraFunGuns", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;unhardened-bundle-0&quot;: [
        ///    &quot;Assets/Prefabs/Enemies/SisyphusPrime.prefab&quot;,
        ///    &quot;Assets/Data/Sandbox/Enemies/Mass.asset&quot;,
        ///    &quot;Assets/Models/SisyphusPrime/T_SisyphusBody_Pattern5.tga&quot;,
        ///    &quot;Assets/Data/Sandbox/Enemies/Malicious Face.asset&quot;,
        ///    &quot;Assets/Textures/Bloodsplatter_Atlas.tga&quot;,
        ///    &quot;Assets/Models/SisyphusPrime/SisyHeart.mat&quot;,
        ///    &quot;Assets/Data/Sandbox/Enemies/Mindflayer.asset&quot;,
        ///    &quot;Assets/Data/Sandbox/Enemies/Super Projectile Husk.asset&quot;,
        ///    &quot;Assets/Sounds/Sisyphus Prime/sp_youcantescape.ogg&quot;, [rest of string was truncated]&quot;;.
        /// </summary>
        public static string UnhardendedBundlesJson {
            get {
                return ResourceManager.GetString("UnhardendedBundlesJson", resourceCulture);
            }
        }
    }
}
