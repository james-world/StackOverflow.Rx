using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace StackOverflow.Rx.Linq
{
    public partial class ObservableExtensions
    {
        private static Action<string> _spyLoggingAction = msg => Debug.WriteLine(msg);

        /// <summary>
        /// Set the method Spy uses to log messages
        /// </summary>
        /// <param name="log"></param>
        public static void SetSpyLoggingAction(Action<string> log)
        {
            _spyLoggingAction = log;
        }

        /// <summary>
        /// The Spy operator is based on the answer to
        /// http://stackoverflow.com/questions/20220755/how-can-i-see-what-my-reactive-extensions-query-is-doing
        /// It outputs details logging on the progress of an Observable.
        /// Use ObservableExtensions.SetSpyLoggingAction to provide a log method suitable
        /// for your logging library.
        /// </summary>
        public static IObservable<TSource> Spy<TSource>(
            this IObservable<TSource> source,
            string name = null)
        {
            name = name ?? "IObservable<" + typeof (TSource).Name + ">";
            _spyLoggingAction(name + ": Evaluated on Thread " + Environment.CurrentManagedThreadId);
            return Observable.Create<TSource>(o =>
            {
                _spyLoggingAction(
                    string.Format(
                        "{0}: Subscription started on Thread {1}",
                        name,
                        Environment.CurrentManagedThreadId));
                try
                {
                    var subscription = source
                        .Do(x  => _spyLoggingAction(
                            string.Format("{0}: OnNext({1}) on Thread {2}",
                                          name,
                                          x,
                                          Environment.CurrentManagedThreadId)),
                            ex => _spyLoggingAction(
                                string.Format(
                                    "{0}: OnError({1}) on Thread {2}",
                                    name,
                                    ex,
                                    Environment.CurrentManagedThreadId)),
                            () => _spyLoggingAction(
                                string.Format(
                                    "{0}: OnCompleted() on Thread {1}",
                                    name,
                                    Environment.CurrentManagedThreadId))
                        )
                        .Subscribe(o);
                    return new CompositeDisposable(
                        subscription,
                        Disposable.Create(() => _spyLoggingAction(
                            string.Format(
                                    "{0}: Cleaned up on Thread {1}",
                                    name,
                                    Environment.CurrentManagedThreadId))));
                }
                finally
                {
                    _spyLoggingAction(
                        string.Format(
                            "{0}: Subscription complete on Thread {1}",
                            name,
                            Environment.CurrentManagedThreadId));
                }
            });
        }

    }
}