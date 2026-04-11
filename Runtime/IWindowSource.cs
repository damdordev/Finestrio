using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public interface IWindowSource
    {
        UniTask<TWindow> Create<TWindow>(object parameter = null) where TWindow : IWindow;
        void Destroy(IWindow window, object parameter = null);
    }
}