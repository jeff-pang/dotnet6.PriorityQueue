// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace dotnet6.PriorityQueue
{
    public class PriorityQueue_NonGeneric_Tests
    {
        protected PriorityQueue<string, int> CreateSmallPriorityQueue(out HashSet<(string, int)> items)
        {
            items = new HashSet<(string, int)>
            {
                ("one", 1),
                ("two", 2),
                ("three", 3)
            };
            var queue = new PriorityQueue<string, int>(items);

            return queue;
        }

        protected PriorityQueue<int, int> CreatePriorityQueue(int initialCapacity, int count)
        {
            var pq = new PriorityQueue<int, int>(initialCapacity);
            for (int i = 0; i < count; i++)
            {
                pq.Enqueue(i, i);
            }

            return pq;
        }

        [Fact]
        public void PriorityQueue_Generic_EnqueueDequeue_Empty()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();

            Assert.Equal("hello", queue.EnqueueDequeue("hello", 42));
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void PriorityQueue_Generic_EnqueueDequeue_SmallerThanMin()
        {
            PriorityQueue<string, int> queue = CreateSmallPriorityQueue(out HashSet<(string, int)> enqueuedItems);

            string actualElement = queue.EnqueueDequeue("zero", 0);

            Assert.Equal("zero", actualElement);
            Assert.True(enqueuedItems.SetEquals(queue.UnorderedItems));
        }

        [Fact]
        public void PriorityQueue_Generic_EnqueueDequeue_LargerThanMin()
        {
            PriorityQueue<string, int> queue = CreateSmallPriorityQueue(out HashSet<(string, int)> enqueuedItems);

            string actualElement = queue.EnqueueDequeue("four", 4);

            Assert.Equal("one", actualElement);
            Assert.Equal("two", queue.Dequeue());
            Assert.Equal("three", queue.Dequeue());
            Assert.Equal("four", queue.Dequeue());
        }

        [Fact]
        public void PriorityQueue_Generic_EnqueueDequeue_EqualToMin()
        {
            PriorityQueue<string, int> queue = CreateSmallPriorityQueue(out HashSet<(string, int)> enqueuedItems);

            string actualElement = queue.EnqueueDequeue("one-not-to-enqueue", 1);

            Assert.Equal("one-not-to-enqueue", actualElement);
            Assert.True(enqueuedItems.SetEquals(queue.UnorderedItems));
        }

        [Fact]
        public void PriorityQueue_Generic_Constructor_IEnumerable_Null()
        {
            (string, int)[] itemsToEnqueue = new(string, int)[] { (null, 0), ("one", 1) } ;
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>(itemsToEnqueue);
            Assert.Null(queue.Dequeue());
            Assert.Equal("one", queue.Dequeue());
        }

        [Fact]
        public void PriorityQueue_Generic_Enqueue_Null()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();

            queue.Enqueue(element: null, 1);
            queue.Enqueue(element: "zero", 0);
            queue.Enqueue(element: "two", 2);

            Assert.Equal("zero", queue.Dequeue());
            Assert.Null(queue.Dequeue());
            Assert.Equal("two", queue.Dequeue());
        }

        [Fact]
        public void PriorityQueue_Generic_EnqueueRange_Null()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();

            queue.EnqueueRange(new string[] { null, null, null }, 0);
            queue.EnqueueRange(new string[] { "not null" }, 1);
            queue.EnqueueRange(new string[] { null, null, null }, 0);

            for (int i = 0; i < 6; ++i)
            {
                Assert.Null(queue.Dequeue());
            }

            Assert.Equal("not null", queue.Dequeue());
        }

        [Fact]
        public void PriorityQueue_EmptyCollection_Dequeue_ShouldThrowException()
        {
            var queue = new PriorityQueue<int, int>();

            Assert.Equal(0, queue.Count);
            Assert.False(queue.TryDequeue(out _, out _));
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }

        [Fact]
        public void PriorityQueue_EmptyCollection_Peek_ShouldReturnFalse()
        {
            var queue = new PriorityQueue<int, int>();

            Assert.False(queue.TryPeek(out _, out _));
            Assert.Throws<InvalidOperationException>(() => queue.Peek());
        }

        #region EnsureCapacity, TrimExcess

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 5)]
        [InlineData(1, 1)]
        [InlineData(3, 100)]
        public void PriorityQueue_TrimExcess_ShouldNotChangeCount(int initialCapacity, int count)
        {
            PriorityQueue<int, int> queue = CreatePriorityQueue(initialCapacity, count);

            Assert.Equal(count, queue.Count);
            queue.TrimExcess();
            Assert.Equal(count, queue.Count);
        }

        private static int GetUnderlyingBufferCapacity<TPriority, TElement>(PriorityQueue<TPriority, TElement> queue)
        {
            FieldInfo nodesField = queue.GetType().GetField("_nodes", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(nodesField);
            var nodes = ((TElement Element, TPriority Priority)[])nodesField.GetValue(queue);
            return nodes.Length;
        }

        #endregion

        #region Enumeration

        [Theory]
        [MemberData(nameof(GetNonModifyingOperations))]
        public void PriorityQueue_Enumeration_ValidOnNonModifyingOperation(Action<PriorityQueue<int, int>> nonModifyingOperation, int count)
        {
            PriorityQueue<int, int> queue = CreatePriorityQueue(initialCapacity: count, count: count);
            using var enumerator = queue.UnorderedItems.GetEnumerator();
            nonModifyingOperation(queue);
            enumerator.MoveNext();
        }

        [Theory]
        [MemberData(nameof(GetModifyingOperations))]
        public void PriorityQueue_Enumeration_InvalidationOnModifyingOperation(Action<PriorityQueue<int, int>> modifyingOperation, int count)
        {
            PriorityQueue<int, int> queue = CreatePriorityQueue(initialCapacity: count, count: count);
            using var enumerator = queue.UnorderedItems.GetEnumerator();
            modifyingOperation(queue);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        public static IEnumerable<object[]> GetModifyingOperations()
        {
            yield return WrapArg(queue => queue.Enqueue(42, 0), 0);
            yield return WrapArg(queue => queue.Dequeue(), 5);
            yield return WrapArg(queue => queue.TryDequeue(out _, out _), 5);
            yield return WrapArg(queue => queue.EnqueueDequeue(5, priority: int.MaxValue), 5);
            yield return WrapArg(queue => queue.EnqueueDequeue(5, priority: int.MaxValue), 5);
            yield return WrapArg(queue => queue.EnqueueRange(new[] { (1,2) }), 0);
            yield return WrapArg(queue => queue.EnqueueRange(new[] { (1, 2) }), 10);
            yield return WrapArg(queue => queue.EnqueueRange(new[] { 1, 2 }, 42), 0);
            yield return WrapArg(queue => queue.EnqueueRange(new[] { 1, 2 }, 42), 10);
            yield return WrapArg(queue => queue.EnsureCapacity(2 * queue.Count), 4);
            yield return WrapArg(queue => queue.Clear(), 5);
            yield return WrapArg(queue => queue.Clear(), 0);

            static object[] WrapArg(Action<PriorityQueue<int, int>> arg, int queueCount) => new object[] { arg, queueCount };
        }

        public static IEnumerable<object[]> GetNonModifyingOperations()
        {
            yield return WrapArg(queue => queue.Peek(), 1);
            yield return WrapArg(queue => queue.TryPeek(out _, out _), 1);
            yield return WrapArg(queue => queue.TryDequeue(out _, out _), 0);
            yield return WrapArg(queue => queue.EnqueueDequeue(5, priority: int.MinValue), 1);
            yield return WrapArg(queue => queue.EnqueueDequeue(5, priority: int.MaxValue), 0);
            yield return WrapArg(queue => queue.EnqueueRange(Array.Empty<(int, int)>()), 5);
            yield return WrapArg(queue => queue.EnqueueRange(Array.Empty<int>(), 42), 5);
            yield return WrapArg(queue => queue.EnsureCapacity(5), 5);

            static object[] WrapArg(Action<PriorityQueue<int, int>> arg, int queueCount) => new object[] { arg, queueCount };
        }

        #endregion
    }
}
