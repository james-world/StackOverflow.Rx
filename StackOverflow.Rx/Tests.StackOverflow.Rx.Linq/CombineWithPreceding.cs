using System;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using StackOverflow.Rx.Linq;

namespace Tests.StackOverflow.Rx.Linq
{
    public class CombineWithPreceding : ReactiveTest
    {
        [Test]
        public void IsCorrect()
        {
            var scheduler = new TestScheduler();
            var source = scheduler.CreateColdObservable(
                OnNext(100, 1),
                OnNext(200, 2),
                OnNext(300, 3),
                OnCompleted<int>(400));

            var results = scheduler.CreateObserver<Tuple<int,int>>();

            source.CombineWithPreceding(Tuple.Create).Subscribe(results);

            scheduler.Start();

            results.Messages.AssertEqual(
                OnNext(100, Tuple.Create(1, 0)),
                OnNext(200, Tuple.Create(2, 1)),
                OnNext(300, Tuple.Create(3, 2)),
                OnCompleted<Tuple<int,int>>(400));
        }
    }
}
