using System;
using System.Collections.Generic;

namespace HttpMockSlim.Utils
{
    public interface IDisposablesBag : IDisposable
    {
        void AddDisposable(IDisposable disposable);
    }
}