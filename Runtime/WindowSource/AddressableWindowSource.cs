#if DAMDOR_FINESTRIO_ADDRESSABLES

using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Implements <see cref="IWindowSource"/> using the Unity Addressables package to load prefabs asynchronously.
    /// Decorate your window class with <see cref="WindowInAddressables"/> or call Register directly.
    /// </summary>
    /// <example>
    /// <code>
    /// [WindowInAddressables("MyAddressableKey_Popup")]
    /// public class MyPopup : Window { }
    /// </code>
    /// </example>
    public class AddressableWindowSource : IWindowSource
    {
        private readonly Dictionary<Type, string> typeToPath = new();
        private readonly Transform parent;

        /// <summary>
        /// Creates an AddressableWindowSource targeting a specified parent transform for newly instantiated prefabs.
        /// </summary>
        /// <param name="parent">The transform under which newly instantiated addressable prefabs will reside.</param>
        public AddressableWindowSource(Transform parent)
        {
            this.parent = parent;
        }
        
        /// <summary>
        /// Evaluates if the provided window class type has an associated Addressable key mapping.
        /// </summary>
        /// <param name="type">The type of <see cref="Window"/> to verify.</param>
        /// <returns>True if a key maps to the type via manual registration or attribute, otherwise false.</returns>
        public bool Support(Type type)
            => UpdateAndGetPath(type) != null;

        /// <summary>
        /// Asynchronously instantiates the Addressable prefab associated with a given window type.
        /// </summary>
        /// <typeparam name="TWindow">The type of Window script component attached to the Addressable prefab.</typeparam>
        /// <returns>A UniTask returning the attached Window script instance.</returns>
        /// <exception cref="ArgumentException">Thrown when an Addressable key does not exist or the prefab does not contain the specified Window component.</exception>
        public async UniTask<TWindow> Create<TWindow>() where TWindow : Window
        {
            var path = UpdateAndGetPath(typeof(TWindow));
            if (path == null)
            {
                throw new ArgumentException($"Cannot create window {typeof(TWindow).Name} from addressable: path not provided");
            }

            var gameObject = await Addressables.InstantiateAsync(path, parent);
            var window = gameObject.GetComponent<TWindow>();

            if (window != null) return window;
            Object.Destroy(gameObject);
            throw new ArgumentException($"Cannot create window {typeof(TWindow).Name} from addressable: window not found");
        }

        /// <summary>
        /// Destroys a previously instantiated Addressable window prefab.
        /// Warning: Currently uses Object.Destroy rather than Addressables.ReleaseInstance. Use with care.
        /// </summary>
        /// <param name="window">The active window script on the Addressable instance to clean up.</param>
        public void Destroy(Window window)
        {
            Object.Destroy(window.gameObject);
        }

        /// <summary>
        /// Manually maps a given <see cref="Window"/> type to a specific Addressable string key, overriding attribute metadata.
        /// </summary>
        /// <typeparam name="TWindow">The class extending Window to map to the Addressable string key.</typeparam>
        /// <param name="path">The exact Addressable string key identifying the associated prefab asset.</param>
        public void Register<TWindow>(string path) where TWindow : Window
        {
            typeToPath[typeof(TWindow)] = path;
        }

        private string UpdateAndGetPath(Type type)
        {
            if (typeToPath.TryGetValue(type, out var path)) return path;
            var windowInAddressables = type.GetCustomAttribute<WindowInAddressables>();
            if (windowInAddressables == null)
            {
                typeToPath[type] = null;
                return null;
            }
            
            typeToPath[type] = windowInAddressables.Key;
            return windowInAddressables.Key;
        }
    }
}

#endif
