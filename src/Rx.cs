#region Copyright 2017 Atif Aziz. All rights reserved.
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

namespace Dsv
{
    using System;
    using Reactive;

    partial class Parser
    {
        public static IObservable<TRow> ParseCsv<THead, TRow>(this IObservable<string> lines,
            Func<TextRow, THead> headSelector,
            Func<THead, TextRow, TRow> rowSelector) =>
            lines.ParseDsv(Format.Csv, headSelector, rowSelector);

        public static IObservable<TRow> ParseDsv<THead, TRow>(this IObservable<string> lines,
            Format format,
            Func<TextRow, THead> headSelector,
            Func<THead, TextRow, TRow> rowSelector) =>
            lines.ParseDsv(format, _ => false, headSelector, rowSelector);

        public static IObservable<TRow> ParseDsv<THead, TRow>(this IObservable<string> lines,
            Format format,
            Func<string, bool> lineFilter,
            Func<TextRow, THead> headSelector,
            Func<THead, TextRow, TRow> rowSelector)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (lineFilter == null) throw new ArgumentNullException(nameof(lineFilter));
            if (headSelector == null) throw new ArgumentNullException(nameof(headSelector));
            if (rowSelector == null) throw new ArgumentNullException(nameof(rowSelector));

            return Observable.Create((IObserver<TRow> o) =>
            {
                var haveHead = false;
                var head = default(THead);

                return lines
                        .ParseDsv(format, lineFilter)
                        .Subscribe(row =>
                            {
                                if (!haveHead)
                                {
                                    head = headSelector(row);
                                    haveHead = true;
                                }
                                else
                                {
                                    o.OnNext(rowSelector(head, row));
                                }
                            },
                            o.OnError,
                            o.OnCompleted);
            });
        }

        public static IObservable<TextRow>
            ParseCsv(this IObservable<string> lines) =>
                lines.ParseDsv(Format.Csv);

        public static IObservable<TextRow>
            ParseCsv(this IObservable<string> lines,
                     Func<string, bool> lineFilter) =>
            lines.ParseDsv(Format.Csv, lineFilter);

        public static IObservable<TextRow>
            ParseDsv(this IObservable<string> lines, Format format) =>
                lines.ParseDsv(format, _ => false);

        public static IObservable<TextRow>
            ParseDsv(this IObservable<string> lines,
                     Format format,
                     Func<string, bool> lineFilter)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (lineFilter == null) throw new ArgumentNullException(nameof(lineFilter));

            return Observable.Create((IObserver<TextRow> o) =>
            {
                var (onLine, onEoi) = Create(format, lineFilter);
                return lines.Subscribe(
                    onNext: line =>
                    {
                        if (onLine(line) is TextRow row)
                            o.OnNext(row);
                    },
                    onError: o.OnError,
                    onCompleted: () =>
                    {
                        if (onEoi() is Exception e)
                            o.OnError(e);
                        else
                            o.OnCompleted();
                    });
            });
        }
    }
}

namespace Dsv.Reactive
{
    using System;

    static class Observable
    {
        public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> subscriptionHandler) =>
            new Observable<T>(subscriptionHandler);

        public static IDisposable Subscribe<T>(this IObservable<T> source,
            Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (onNext == null) throw new ArgumentNullException(nameof(onNext));
            if (onError == null) throw new ArgumentNullException(nameof(onError));
            if (onCompleted == null) throw new ArgumentNullException(nameof(onCompleted));

            try
            {
                return source.Subscribe(Observer.Create(onNext, onError, onCompleted));
            }
            catch (Exception e)
            {
                onError(e);
                return Disposable.Nop;
            }
        }
    }

    static class Disposable
    {
        public static readonly IDisposable Nop = new NopDisposable();

        sealed class NopDisposable : IDisposable
        {
            public void Dispose() {}
        }
    }

    static class Observer
    {
        public static IObserver<T> Create<T>(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null) =>
            new Observer<T>(onNext, onError, onCompleted);
    }

    sealed class Observable<T> : IObservable<T>
    {
        readonly Func<IObserver<T>, IDisposable> _subscriptionHandler;

        public Observable(Func<IObserver<T>, IDisposable> subscriptionHandler) =>
            _subscriptionHandler = subscriptionHandler
                                    ?? throw new ArgumentNullException(nameof(subscriptionHandler));

        public IDisposable Subscribe(IObserver<T> observer) =>
            _subscriptionHandler(observer);
    }

    sealed class Observer<T> : IObserver<T>
    {
        readonly Action<T> _onNext;
        readonly Action<Exception> _onError;
        readonly Action _onCompleted;

        public Observer(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            _onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnCompleted() => _onCompleted?.Invoke();
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnNext(T value) => _onNext(value);
    }
}
