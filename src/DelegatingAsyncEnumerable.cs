#region Copyright 2019 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

#if !NO_ASYNC_STREAM

namespace Dsv
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    sealed class DelegatingAsyncEnumerable<T>(Func<CancellationToken, IAsyncEnumerator<T>> delegatee) :
        IAsyncEnumerable<T>
    {
        readonly Func<CancellationToken, IAsyncEnumerator<T>> _delegatee = delegatee ?? throw new ArgumentNullException(nameof(delegatee));

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            _delegatee(cancellationToken);
    }
}

#endif // !NO_ASYNC_STREAM
