using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public delegate UniTask TransitionErrorHandler(Exception e, CancellationToken cancellationToken);
}