using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Damdor.Finestrio
{
    public class ResourcesWindowSource : IWindowSource
    {
        private readonly Dictionary<Type, string> typeToPath = new();
        private readonly Transform parent;

        public ResourcesWindowSource(Transform parent)
        {
            this.parent = parent;
        }
        
        public bool Support(Type type)
            => UpdateAndGetPath(type) != null;

        public UniTask<TWindow> Create<TWindow>() where TWindow : Window
        {
            var path = UpdateAndGetPath(typeof(TWindow));
            if (path == null)
            {
                throw new ArgumentException($"Cannot create window {typeof(TWindow).Name} from resource: path not provided");
            }

            var mono = Resources.Load<MonoBehaviour>(path);
            if (path == null)
            {
                throw new ArgumentException($"Cannot create window {typeof(TWindow).Name} from resource: prefab {path} not found");
            }
            
            var window = mono.GetComponent<TWindow>();
            if (window == null)
            {
                throw new ArgumentException($"Cannot create window {typeof(TWindow).Name} from resource: prefab {path} has not window");
            }
            
            var windowInstance = Object.Instantiate(window, parent);

            return new UniTask<TWindow>(windowInstance);
        }

        public void Destroy(Window window)
        {
            Object.Destroy(window.gameObject);
        }

        public void Register<TWindow>(string path) where TWindow : Window
        {
            typeToPath[typeof(TWindow)] = path;
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