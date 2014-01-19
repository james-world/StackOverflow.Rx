using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using StackOverflow.Rx.Linq;
using ObservableExtensions = StackOverflow.Rx.Linq.ObservableExtensions;

namespace Tests.StackOverflow.Rx.Linq
{
    public class Spy : ReactiveTest
    {
        private List<string> _spyOutput;
        private readonly object _spyOutputGate = new object();
        private TestScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            _spyOutput = new List<string>();
            _scheduler = new TestScheduler();
            ObservableExtensions.SetSpyLoggingAction(s =>
            {
                lock (_spyOutputGate)
                {
                    _spyOutput.Add(s);
                }
            });
        }

        [Test]
        public void DefaultNameIsCorrect()
        {
            var source = Observable.Never<int>();

            source.Spy();

            Assert.IsTrue(_spyOutput[0].StartsWith("IObservable<Int32>:"));
        }

        [Test]
        public void CustomNameIsCorrect()
        {
            var source = Observable.Never<int>();

            source.Spy("Test");

            Assert.IsTrue(_spyOutput[0].StartsWith("Test:"));
        }

        [Test]
        public void EvaluationIsLogged()
        {
            var source = Observable.Never<int>();
            const string evaluated = @"Test: Evaluated on Thread \d+";

            source.Spy("Test");

            Assert.IsTrue(Regex.IsMatch(_spyOutput[0], evaluated));
        }

        [Test]
        public void BeginSubscriptionMessageIsLogged()
        {
            var source = Observable.Never<int>();
            const string beginSubscribe = @"Test: Subscription started on Thread \d+";

            source.Spy("Test").Subscribe();

            Assert.IsTrue(Regex.IsMatch(_spyOutput[1], beginSubscribe));
        }

        [Test]
        public void EndSubscriptionMessageIsLogged()
        {
            var source = Observable.Never<int>();
            const string endSubscribe = @"Test: Subscription complete on Thread \d+";

            source.Spy("Test").Subscribe();

            Assert.IsTrue(Regex.IsMatch(_spyOutput[2], endSubscribe));
        }

        [Test]
        public void OnNextIsLogged()
        {
            var source = _scheduler.CreateColdObservable(
                OnNext(100, 1));
            var results = _scheduler.CreateObserver<int>();
            const string expected = @"Test: OnNext\(1\) on Thread \d+";

            source.Spy("Test").Subscribe(results);

            _scheduler.Start();

            Assert.IsTrue(Regex.IsMatch(_spyOutput[3], expected));
        }

        [Test]
        public void OnErrorIsLogged()
        {
            var source = _scheduler.CreateColdObservable(
                OnError<int>(100, new Exception("Test Error")));
            var results = _scheduler.CreateObserver<int>();
            const string expected = @"Test: OnError\(System.Exception: Test Error\) on Thread \d+";

            source.Spy("Test").Subscribe(results);

            _scheduler.Start();

            Assert.IsTrue(Regex.IsMatch(_spyOutput[3], expected));
        }

        [Test]
        public void OnCompletedIsLogged()
        {
            var source = _scheduler.CreateColdObservable(
                OnCompleted<int>(100));
            var results = _scheduler.CreateObserver<int>();
            const string expected = @"Test: OnCompleted\(\) on Thread \d+";

            source.Spy("Test").Subscribe(results);

            _scheduler.Start();

            Assert.IsTrue(Regex.IsMatch(_spyOutput[3], expected));
        }

        [Test]
        public void CleanUpIsLogged()
        {
            var source = _scheduler.CreateColdObservable(
                OnCompleted<int>(100));
            var results = _scheduler.CreateObserver<int>();
            const string expected = @"Test: Cleaned up on Thread \d+";

            source.Spy("Test").Subscribe(results);

            _scheduler.Start();

            Assert.IsTrue(Regex.IsMatch(_spyOutput[4], expected));
        }
    }
}