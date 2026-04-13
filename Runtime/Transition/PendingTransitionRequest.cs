using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    internal abstract class PendingTransitionRequest
    {
        public abstract void Run();
        public abstract void Reset();
    }
    
    internal class PendingTransitionRequest<TWindow> : PendingTransitionRequest where TWindow : Window
    {
        private ITransitionRequestQueueReceiver receiver;
        private TransitionRequest<TWindow> request;
        private Action onFinish;
        private UniTaskCompletionSource<TWindow> tcs;
        private CancellationToken cancellationToken;

        public void Setup(
            ITransitionRequestQueueReceiver receiver,
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken,
            Action onFinish)
        {
            this.receiver = receiver;
            this.request = request;
            this.onFinish = onFinish;
            this.cancellationToken = cancellationToken;
            tcs = new UniTaskCompletionSource<TWindow>();
        }

        public UniTask<TWindow> Wait()
        {
            return tcs.Task;
        }

        public override void Run()
        {
            receiver.ProcessRequest(request, cancellationToken).ContinueWith(OnFinish);
        }

        public override void Reset()
        {
            receiver = null;
            request = null;
            onFinish = null;
            tcs = null;
            cancellationToken = CancellationToken.None;
        }

        private void OnFinish(TWindow window)
        {
            tcs.TrySetResult(window);
            onFinish?.Invoke();
        }
        
    }
}