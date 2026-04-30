using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public delegate UniTask<TModel> TransitionRequestModelRetrieveAsync<TModel>(CancellationToken cancelToken);
    public delegate void TransitionRequestModelSetup<in TWindow, in TModel>(TWindow window, TModel model);
    public delegate UniTask TransitionRequestModelSetupAsync<in TWindow, in TModel>(TWindow window, TModel model, CancellationToken cancelToken);
    public delegate ITransitionAnimation TransitionRequestAnimationGetter<in TWindow>(IFinestrioWindow source, TWindow target, TransitionType transitionType);
}