using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public delegate UniTask<TModel> TransitionRequestModelRetrieveAsync<TModel>(CancellationToken cancelToken);
    public delegate void TransitionRequestModelSetup<TWindow, TModel>(TWindow window, TModel model);
    public delegate UniTask TransitionRequestModelSetupAsync<TWindow, TModel>(TWindow window, TModel model, CancellationToken cancelToken);
    public delegate ITransitionAnimation TransitionRequestAnimationGetter<TWindow>(Window source, TWindow target, TransitionType transitionType);
}