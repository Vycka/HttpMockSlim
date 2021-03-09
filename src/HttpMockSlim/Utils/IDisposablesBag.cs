using System;
using System.Collections;
using System.Collections.Generic;

namespace HttpMockSlim.Utils
{
    public interface IDisposablesBag : IDisposable
    {
        IList<IDisposable> Disposables { get; }
    }
}