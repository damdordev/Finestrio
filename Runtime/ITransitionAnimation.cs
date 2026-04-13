using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public interface ITransitionAnimation
    {
        UniTask Prepare(Window source, Window target);
        UniTask Play();
    }
}