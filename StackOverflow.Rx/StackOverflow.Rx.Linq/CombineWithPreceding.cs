using System;
using System.Reactive.Linq;

namespace StackOverflow.Rx.Linq
{
    /// <summary>
    /// This operator is based on the answer to
    /// http://stackoverflow.com/questions/2820685/get-previous-element-in-iobservable-without-re-evaluating-the-sequence/16581526#16581526
    /// </summary>
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// Projects the current and preceding element of a source sequence into a new form with the selector.
        /// </summary>
        /// <typeparam name="TSource">The type of source.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="source">A sequence of elements to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each source element and it's predecessor.</param>
        /// <returns>An observable sequence in a new form.</returns>
        /// <remarks>The first time the selector is called, a default value is passed for the preceding element.</remarks>
        public static IObservable<TResult> CombineWithPreceding<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, TSource, TResult> selector)
        {
            return source.Scan(
                Tuple.Create(default(TSource), default(TSource)),
                (previous, current) => Tuple.Create(current, previous.Item1))
                .Select(element => selector(element.Item1, element.Item2));
        }
    }
}
