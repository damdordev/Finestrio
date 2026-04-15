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
            this.windowSource = windowSource;
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
            var source = windows.Count > 0 ? windows[^1] : null;
            var target = await windowSource.Create<TWindow>();
            windows.Add(target);
            
            await SetupAndAnimate(request, source, target, cancellationToken);
            UpdateVisibilities();
            
            return target;
        }
        
        private async UniTask<TWindow> ProcessChange<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            var source = windows.Count > 0 ? windows[^1] : null;
            var target = await windowSource.Create<TWindow>();

            if (source != null)
            {
                windows.Remove(source);
                windowSource.Destroy(source);
            }
            windows.Add(target);
            
            await SetupAndAnimate(request, source, target, cancellationToken);
            UpdateVisibilities();
            
            return target;
        }
        
        private async UniTask<TWindow> ProcessBack<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            var source = windows.Count > 0 ? windows[^1] : null;
            var target = windows.Count > 1 ? (TWindow) windows[^2] : null;
            if (source != null) windows.Remove(source);
            
            await SetupAndAnimate(request, source, target, cancellationToken);
            
            if (source != null) windowSource.Destroy(source);
            UpdateVisibilities();
            
            return (TWindow) TopWindow;
        }
        
        private async UniTask SetupAndAnimate<TWindow>(TransitionRequest<TWindow> request, Window source, TWindow target, CancellationToken cancellationToken)
            where TWindow : Window
        {
            source?.SetVisible(true);
            target?.SetVisible(true);
            var animation = GetTransitionAnimation(request, source, target, request.TransitionType);
            await animation.Prepare();
            request.RetrieveAllModels(cancellationToken);
            await request.SetupAllModels(target, cancellationToken);
            await animation.Play();
        }

        private ITransitionAnimation GetTransitionAnimation<TWindow>(
            TransitionRequest<TWindow> request,
            Window source,
            Window target,
            TransitionType type) where TWindow : Window
            => new EmptyTransitionAnimation(0f, 0f);

        private void UpdateVisibilities()
        {
            var wasFullscreenWindow = false;
            for (var i = windows.Count - 1; i >= 0; i--)
            {
                windows[i].SetVisible(!wasFullscreenWindow);
                if (!windows[i].IsTransparent) wasFullscreenWindow = true;
            }
        }

    }
}