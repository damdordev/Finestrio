using System;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public interface IWindowSource
    {
        bool Support(Type type);
        UniTask<TWindow> Create<TWindow>() where TWindow : Window;
        void Destroy(Window window);
    }
}