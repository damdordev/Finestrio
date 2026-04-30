using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Damdor.Finestrio
{
    /// <summary>
    /// An aggregate <see cref="IWindowSource"/> that attempts to locate windows using multiple configured sources.
    /// Provides fallback capabilities. The first source supporting the requested window type is used.
    /// </summary>
    /// <example>
    /// <code>
    /// var addressablesSource = new AddressableWindowSource(parent);
    /// var resourcesSource = new ResourcesWindowSource(parent);
    /// var combined = new CombinedWindowSource(addressablesSource, resourcesSource);
    /// </code>
    /// </example>
    public class CombinedWindowSource : IWindowSource
    {
        private readonly List<IWindowSource> windowSources;

        /// <summary>
        /// Instantiates a CombinedWindowSource composed of an ordered list of other sources.
        /// </summary>
        /// <param name="windowSources">An array of <see cref="IWindowSource"/> to check in order.</param>
        public CombinedWindowSource(params IWindowSource[] windowSources)
        {
            this.windowSources = new List<IWindowSource>(windowSources); 
        }
        
        /// <summary>
        /// Checks if any of the underlying sources support the specified window type.
        /// </summary>
        /// <param name="type">The type of window class to lookup.</param>
        /// <returns>True if at least one contained source can instantiate the window.</returns>
        public bool Support(Type type)
            => GetWindowSource(type) != null;

        /// <summary>
        /// Defers creation of a window to the first configured source that supports the given type.
        /// </summary>
        /// <typeparam name="TWindow">The class extending <see cref="IFinestrioWindow"/> to instantiate.</typeparam>
        /// <returns>A UniTask resolving to the created instance of the window.</returns>
        /// <exception cref="ArgumentException">Thrown when no underlying source supports the requested window type.</exception>
        public UniTask<TWindow> Create<TWindow>() where TWindow : IFinestrioWindow
        {
            var source = GetWindowSource(typeof(TWindow));
            return source?.Create<TWindow>() ?? throw new ArgumentException($"Cannot create window {typeof(TWindow).Name}: source not found");
        }

        /// <summary>
        /// Defers destruction of a window to the source that originally instantiated it (based on its supported type).
        /// </summary>
        /// <param name="finestrioWindow">The active window instance to clean up.</param>
        /// <exception cref="ArgumentException">Thrown when no underlying source supports destroying the specified window.</exception>
        public void Destroy(IFinestrioWindow finestrioWindow)
        {
            var source = GetWindowSource(finestrioWindow.GetType());
            if (source == null)
            {
                throw new ArgumentException($"Cannot destroy window {finestrioWindow.GetType().Name}: source not found");
            }

            source.Destroy(finestrioWindow);
        }

        private IWindowSource GetWindowSource(Type type)
        {
            foreach (var source in windowSources)
            {
                if (source.Support(type)) return source;
            }

            return null;
        }
    }
}
