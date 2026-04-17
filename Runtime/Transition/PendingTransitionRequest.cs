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
        private CancellationToken cancellationToken;
        private bool started;

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
        }

        public async UniTask<TWindow> Wait()
        {
            await UniTask.WaitUntil(() => started, cancellationToken: cancellationToken);
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
            started = true;
        }

        public override void Reset()
        {
            if(request != null) FinestrioInternalHelper.ReleaseTransitionRequest(request);
            receiver = null;
            request = null;
            onFinish = null;
            cancellationToken = CancellationToken.None;
            started = false;
        }
        
    }
}