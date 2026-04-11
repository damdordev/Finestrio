using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public class WindowManager : ITransitionRequestQueueReceiver
    {
        private readonly List<IWindow> windows = new();
        private readonly IWindowSource windowSource;
        private readonly TransitionRequestQueue requestQueue;

        public WindowManager()
        {
            requestQueue = new TransitionRequestQueue(this);
        }
        
        public UniTask<TWindow> Transite<TWindow>(TransitionRequest<TWindow> request) where TWindow : IWindow 
            => requestQueue.Enqueue(request);

        async UniTask<TWindow> ITransitionRequestQueueReceiver.ProcessRequest<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken)
        {
            return default;
        }
    }
}