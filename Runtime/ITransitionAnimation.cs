using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public interface ITransitionAnimation
    {
        UniTask Prepare(IWindow source, IWindow target);
        UniTask Play();
    }
}