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
    public class AddressableWindowSource : IWindowSource
    {
        private readonly Dictionary<Type, string> typeToPath = new();
        private readonly Transform parent;

        public AddressableWindowSource(Transform parent)
        {
            this.parent = parent;
        }
        
        public bool Support(Type type)
            => UpdateAndGetPath(type) != null;

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