using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public class WindowManager : ITransitionRequestQueueReceiver
    {
        public Window TopWindow => windows.Count > 0 ? windows[^1] : null;
        
        private readonly List<Window> windows = new();
        
        private readonly IWindowSource windowSource;
        private readonly TransitionRequestQueue requestQueue;

        public WindowManager(IWindowSource windowSource)
        {
            requestQueue = new TransitionRequestQueue(this);
        }
        
        public UniTask<TWindow> Transite<TWindow>(TransitionRequest<TWindow> request) where TWindow : Window 
            => requestQueue.Enqueue(request);

        UniTask<TWindow> ITransitionRequestQueueReceiver.ProcessRequest<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken)
            => request.TransitionType switch
            {
                TransitionType.Add => ProcessAdd(request, cancellationToken),
                TransitionType.Change => ProcessChange(request, cancellationToken),
                TransitionType.Back => ProcessBack(request, cancellationToken),
                _ => throw new ArgumentOutOfRangeException($"Unknown transition type: {request.TransitionType}")
            };

        private async UniTask<TWindow> ProcessAdd<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            return default;
        }
        
        private async UniTask<TWindow> ProcessChange<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            return default;
        }
        
        private async UniTask<TWindow> ProcessBack<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            return default;
        }
    }
}