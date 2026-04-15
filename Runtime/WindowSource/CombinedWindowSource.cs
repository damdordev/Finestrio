using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public class CombinedWindowSource : IWindowSource
    {
        private readonly List<IWindowSource> windowSources;

        public CombinedWindowSource(params IWindowSource[] windowSources)
        {
            this.windowSources = new List<IWindowSource>(windowSources); 
        }
        
        public bool Support(Type type)
            => GetWindowSource(type) != null;

        public UniTask<TWindow> Create<TWindow>() where TWindow : Window
        {
            var source = GetWindowSource(typeof(TWindow));
            return source?.Create<TWindow>() ?? throw new ArgumentException($"Cannot create window {typeof(TWindow).Name}: source not found");
        }

        public void Destroy(Window window)
        {
            var source = GetWindowSource(window.GetType());
            if (source == null)
            {
                throw new ArgumentException($"Cannot destroy window {window.GetType().Name}: source not found");
            }

            source.Destroy(window);
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