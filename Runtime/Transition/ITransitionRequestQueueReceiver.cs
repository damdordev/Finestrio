using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Damdor.Finestrio
{
    internal interface ITransitionRequestQueueReceiver
    {
        UniTask<TWindow> ProcessRequest<TWindow>(TransitionRequest<TWindow> request, CancellationToken cancellationToken)
            where TWindow : MonoBehaviour, IFinestrioWindow;
    }
}