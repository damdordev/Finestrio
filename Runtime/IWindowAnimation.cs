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
        UniTask Prepare(Window source, Window target);
        UniTask Run();
    }
}