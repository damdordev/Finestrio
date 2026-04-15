using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    internal class TransitionRequestQueue
    {
        private readonly ITransitionRequestQueueReceiver receiver;
        private readonly CancellationTokenSource cancellationTokenSource = new();
        
        private readonly Queue<PendingTransitionRequest> pendingRequests = new();
        private PendingTransitionRequest currentRequest;
        
        internal TransitionRequestQueue(ITransitionRequestQueueReceiver receiver)
        {
            this.receiver = receiver;
        }
        
        public UniTask<TWindow> Enqueue<TWindow>(TransitionRequest<TWindow> request) where TWindow : Window
        {
            var pendingRequest = FinestrioInternalHelper.GetPendingTransitionRequest<TWindow>();
            pendingRequest.Setup(receiver, request, cancellationTokenSource.Token, OnRequestFinished);
            pendingRequests.Enqueue(pendingRequest);

            var result = pendingRequest.Wait();
            TryStartNextRequest();
            return result;
        }

        public void Release()
        {
            pendingRequests.Clear();
            currentRequest = null;
            cancellationTokenSource.Cancel();
        }

        private void OnRequestFinished()
        {
            FinestrioInternalHelper.ReleasePendingTransitionRequest(currentRequest);
            currentRequest = null;
            TryStartNextRequest();
        }

        private void TryStartNextRequest()
        {
            if (currentRequest != null || pendingRequests.Count == 0) return;
            currentRequest = pendingRequests.Dequeue();
            currentRequest.Run();
        }
        
    }
}