using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public enum WindowAnimationType
    {
        Add,
        Remove,
        Pause,
        Resume
    }
    
    public interface IWindowAnimation
    {
        UniTask Prepare(IWindow source, IWindow target);
        UniTask Run();
    }
}