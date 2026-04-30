using System;
using System.Collections.Generic;
using UnityEngine;

namespace Damdor.Finestrio
{
    internal static class FinestrioInternalHelper
    {
        
        private static readonly Dictionary<Type, List<object>> transitionRequestSetupPool = new();
        private static readonly Dictionary<Type, List<object>> transitionRequestPool = new();
        private static readonly Dictionary<Type, List<object>> pendingTransitionRequestPool = new();

        public static TransitionRequestSetup<TWindow, TModel> GetTransitionRequestSetup<TWindow, TModel>() where TWindow : IFinestrioWindow
        {
            var type = typeof(TransitionRequestSetup<TWindow, TModel>);
            if (!transitionRequestSetupPool.TryGetValue(type, out var list) || list.Count <= 0)
                return new TransitionRequestSetup<TWindow, TModel>();
            
            var setup = list[^1] as TransitionRequestSetup<TWindow, TModel>;
            list.RemoveAt(list.Count - 1);
            return setup;
        }

        public static void ReleaseTransitionRequestSetup<TWindow>(TransitionRequestSetup<TWindow> setup)
            where TWindow : IFinestrioWindow
        {
            var type = setup.GetType();
            if (!transitionRequestSetupPool.TryGetValue(type, out var list))
            {
                list = new List<object>();
                transitionRequestSetupPool[type] = list;
            }

            setup.Reset();
            list.Add(setup);
        }
        
        public static TransitionRequest<TWindow> GetTransitionRequest<TWindow>(TransitionType transitionType) where TWindow : IFinestrioWindow
        {
            var type = typeof(TWindow);
            TransitionRequest<TWindow> request;
            
            if (transitionRequestPool.TryGetValue(type, out var list) && list.Count > 0)
            {
                request = list[^1] as TransitionRequest<TWindow>;
                list.RemoveAt(list.Count - 1);
            }
            else
            {
                request = new TransitionRequest<TWindow>();
            }
        
            request.Type(transitionType);
            return request;
        }
        
        public static void ReleaseTransitionRequest<TWindow>(TransitionRequest<TWindow> request) where TWindow : IFinestrioWindow
        {
            var type = typeof(TWindow);
            if (!transitionRequestPool.TryGetValue(type, out var list))
            {
                list = new List<object>();
                transitionRequestPool[type] = list;
            }
            request.Reset();
            list.Add(request);
        }
        
        public static PendingTransitionRequest<TWindow> GetPendingTransitionRequest<TWindow>() 
            where TWindow : MonoBehaviour, IFinestrioWindow
        {
            var type = typeof(PendingTransitionRequest<TWindow>);
            PendingTransitionRequest<TWindow> request;
            
            if (pendingTransitionRequestPool.TryGetValue(type, out var list) && list.Count > 0)
            {
                request = list[^1] as PendingTransitionRequest<TWindow>;
                list.RemoveAt(list.Count - 1);
            }
            else
            {
                request = new PendingTransitionRequest<TWindow>();
            }
        
            return request;
        }
        
        public static void ReleasePendingTransitionRequest(PendingTransitionRequest request)
        {
            var type = request.GetType();
            if (!pendingTransitionRequestPool.TryGetValue(type, out var list))
            {
                list = new List<object>();
                pendingTransitionRequestPool[type] = list;
            }
            request.Reset();
            list.Add(request);
        }
        
    }
}