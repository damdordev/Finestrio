using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public interface ITransitionRequestQueueReceiver
    {
        UniTask<TWindow> ProcessRequest<TWindow>(TransitionRequest<TWindow> request, CancellationToken cancellationToken)
            where TWindow : IWindow;
    }
}