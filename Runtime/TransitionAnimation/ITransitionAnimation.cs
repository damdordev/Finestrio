using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public interface ITransitionAnimation
    {
        UniTask Prepare();
        UniTask Play();
    }
}