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
    
    internal class PendingTransitionRequest<TWindow> : PendingTransitionRequest 
        where TWindow : IFinestrioWindow
    {
        private ITransitionRequestQueueReceiver receiver;
        private TransitionRequest<TWindow> request;
        private Action onFinish;
        private CancellationToken cancellationToken;
        private UniTaskCompletionSource startedCompletionSource;

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
            startedCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask<TWindow> Wait()
        {
            await startedCompletionSource.Task;
            try
            {
                return await receiver.ProcessRequest(request, cancellationToken);
            }
            finally
            {
                onFinish?.Invoke();
            }
        }

        public override void Run()
        {
            startedCompletionSource.TrySetResult();
        }

        public override void Reset()
        {
            if(request != null) FinestrioInternalHelper.ReleaseTransitionRequest(request);
            receiver = null;
            request = null;
            onFinish = null;
            cancellationToken = CancellationToken.None;
            startedCompletionSource = null;
        }
        
    }
}