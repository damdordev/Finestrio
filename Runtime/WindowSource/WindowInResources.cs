using System;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Attribute used to decorate a <see cref="Window"/> class with its subpath within a Unity 'Resources' directory.
    /// Enables <see cref="ResourcesWindowSource"/> to automatically load the mapped prefab.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WindowInResources : Attribute
    {
        /// <summary>
        /// The path relative to an active 'Resources' folder containing the prefab asset.
        /// </summary>
        public string ResourcePath { get; private set; }

        /// <summary>
        /// Applies the Resources folder subpath mapping to a specific window.
        /// </summary>
        /// <param name="resourcePath">The folder hierarchy structure excluding the .prefab extension.</param>
        public WindowInResources(string resourcePath)
        {
            ResourcePath = resourcePath;
        }
    }
}
