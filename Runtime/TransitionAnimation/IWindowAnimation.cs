using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public interface IWindowAnimation
    {
        UniTask Prepare();
        UniTask Play();
        WindowOrderInAnimation Order { get; }
    }
}