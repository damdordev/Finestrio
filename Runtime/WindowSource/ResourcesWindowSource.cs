using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Loads windows from Unity's Resources system by path.
    /// Decorate your window class with <see cref="WindowInResources"/> or call Register directly.
    /// </summary>
    /// <example>
    /// <code>
    /// [WindowInResources("UI/Popups/MyPopup")]
    /// public class MyPopup : Window { }
    /// </code>
    /// </example>
    public class ResourcesWindowSource : IWindowSource
    {
        private readonly Dictionary<Type, string> typeToPath = new();
        private readonly Transform parent;

        /// <summary>
        /// Instantiates a new ResourcesWindowSource targeting a specific parent transform in the scene hierarchy.
        /// </summary>
        /// <param name="parent">The transform under which newly instantiated window prefabs will reside.</param>
        public ResourcesWindowSource(Transform parent)
        {
            this.parent = parent;
        }
        
        /// <summary>
        /// Verifies whether the provided window class type is mapped to a valid Resources path.
        /// </summary>
        /// <param name="type">The specific Window Type to query.</param>
        /// <returns>True if a mapped path exists (either registered manually or via attribute).</returns>
        public bool Support(Type type)
            => UpdateAndGetPath(type) != null;

        /// <summary>
        /// Synchronously loads a prefab from Resources and instantiates it asynchronously.
        /// </summary>
        /// <typeparam name="TWindow">The type of <see cref="IFinestrioWindow"/> to spawn.</typeparam>
        /// <returns>A UniTask returning the attached script instance of the newly spawned window prefab.</returns>
        /// <exception cref="ArgumentException">Thrown when a registered path is invalid, missing a prefab, or the prefab lacks the appropriate <see cref="IFinestrioWindow"/> script component.</exception>
        public UniTask<TWindow> Create<TWindow>() where TWindow : IFinestrioWindow
        {
            var path = UpdateAndGetPath(typeof(TWindow));
            if (path == null)
            {
                throw new ArgumentException($"Cannot create window {typeof(TWindow).Name} from resource: path not provided");
            }

            var mono = Resources.Load<MonoBehaviour>(path);
            if (mono == null)
            {
                throw new ArgumentException($"Cannot create window {typeof(TWindow).Name} from resource: prefab {path} not found");
            }
            
            var window = mono.GetComponent<TWindow>();
            if (window == null)
            {
                throw new ArgumentException($"Cannot create window {typeof(TWindow).Name} from resource: prefab {path} has not window");
            }

            var windowInstanceMono = Object.Instantiate(mono, parent);
            var windowInstance = windowInstanceMono.GetComponent<TWindow>();
            
            if (windowInstance == null)
            {
                Object.Destroy(windowInstanceMono.gameObject);
                throw new ArgumentException($"Cannot create window {typeof(TWindow).Name} from resource: prefab {path} has not window");
            }

            return new UniTask<TWindow>(windowInstance);
        }

        /// <summary>
        /// Destroys the instantiated GameObject of the provided window immediately.
        /// </summary>
        /// <param name="finestrioWindow">The window script attached to the instantiated prefab hierarchy to destroy.</param>
        public void Destroy(IFinestrioWindow finestrioWindow)
        {
            var monoBehaviour = finestrioWindow as MonoBehaviour;
            if(monoBehaviour != null) Object.Destroy(monoBehaviour.gameObject);
        }

        /// <summary>
        /// Manually registers a specific <see cref="IFinestrioWindow"/> type to a Resources subpath, overriding any class attributes.
        /// </summary>
        /// <typeparam name="TWindow">The class extending Window to associate with the resource string.</typeparam>
        /// <param name="path">The folder path (relative to a 'Resources' directory) to load the prefab from.</param>
        public void Register<TWindow>(string path) where TWindow : IFinestrioWindow
        {
            typeToPath[typeof(TWindow)] = path;
        }
        
        /// <summary>
        /// Manually maps a given <see cref="FinestrioWindow"/> type to a specific Addressable string key, overriding attribute metadata.
        /// </summary>
        /// <param name="type">The class extending Window to map to the Addressable string key.</typeparam>
        /// <param name="path">The exact Addressable string key identifying the associated prefab asset.</param>
        public void Register(Type type, string path)
        {
            typeToPath[type] = path;
        }
        
        /// <summary>
        /// Registers an interface type to resolve to the same Resource path as the specified implementation class type.
        /// Useful when windows are requested by interface rather than concrete type.
        /// </summary>
        /// <param name="interfaceType">The interface type that acts as an alias or abstraction for the window.</param>
        /// <param name="classType">The concrete class type (extending Window) whose resource path will be used.</param>
        public void RegisterWithInterface(Type interfaceType, Type classType)
        {
            Register(interfaceType, UpdateAndGetPath(classType));
        }

        private string UpdateAndGetPath(Type type)
        {
            if (typeToPath.TryGetValue(type, out var path)) return path;
            var windowInResources = type.GetCustomAttribute<WindowInResources>();
            if (windowInResources == null)
            {
                typeToPath[type] = null;
                return null;
            }
            
            typeToPath[type] = windowInResources.ResourcePath;
            return windowInResources.ResourcePath;
        }
    }
}
