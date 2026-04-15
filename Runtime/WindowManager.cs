using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
            target.IndexOnStack = windows.Count;
            
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
            target.IndexOnStack = windows.Count;

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
            var shouldRevertWindowsForAnimation = animation != null && source != null && target != null &&
                                      ShouldRevertWindowsForAnimation(request.TransitionType, animation.Order);
           
            if(shouldRevertWindowsForAnimation) RevertWindowsForAnimations(source, target);
            if (animation != null) await animation.Prepare();
            request.RetrieveAllModels(cancellationToken);
            await request.SetupAllModels(target, cancellationToken);
            if (animation != null)
            {
                await animation.Play();
            }
            if(shouldRevertWindowsForAnimation) RevertWindowsForAnimations(source, target);
        }

        private ITransitionAnimation GetTransitionAnimation<TWindow>(
            TransitionRequest<TWindow> request,
            Window source,
            TWindow target,
            TransitionType transitionType) where TWindow : Window
            => request.GetAnimation != null
                ? request.GetAnimation(source, target, transitionType)
                : CompoundTransitionAnimation.Combine(source, target, transitionType);

        private bool ShouldRevertWindowsForAnimation(TransitionType transitionType, WindowOrderInAnimation order)
            => transitionType switch
            {
                TransitionType.Add or TransitionType.Change => order == WindowOrderInAnimation.OldOnTop,
                TransitionType.Back => order == WindowOrderInAnimation.NewOnTop,
                _ => throw new ArgumentOutOfRangeException(nameof(transitionType), transitionType, null)
            };

        private void RevertWindowsForAnimations(Window source, Window target)
        {
            var sourceIndex = source.IndexOnStack;
            var targetIndex = target.IndexOnStack;

            source.IndexOnStack = targetIndex;
            target.IndexOnStack = sourceIndex;
        }

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