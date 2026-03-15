using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Bridge.Tests;

public class BrowserMutationQueueTests
{
    private sealed class TestMutation : BrowserMutation
    {
        public bool Executed { get; private set; }
        public Func<Task>? OnExecute { get; init; }

        public override async Task ExecuteAsync()
        {
            Executed = true;
            if (OnExecute is not null)
                await OnExecute();
        }
    }

    private sealed class ThrowingMutation : BrowserMutation
    {
        public required Exception Exception { get; init; }

        public override Task ExecuteAsync() => throw Exception;
    }

    [Fact]
    public async Task FlushAsync_ExecutesMutations()
    {
        var queue = new BrowserMutationQueue();
        var a = new TestMutation { ElementId = "el-1" };
        var b = new TestMutation { ElementId = "el-2" };

        queue.Enqueue(a);
        queue.Enqueue(b);

        await queue.FlushAsync();

        Assert.True(a.Executed);
        Assert.True(b.Executed);
    }

    [Fact]
    public async Task FlushAsync_DeduplicatesByElementId_LastWins()
    {
        var queue = new BrowserMutationQueue();
        var first = new TestMutation { ElementId = "el-1" };
        var last = new TestMutation { ElementId = "el-1" };

        queue.Enqueue(first);
        queue.Enqueue(last);

        await queue.FlushAsync();

        Assert.False(first.Executed);
        Assert.True(last.Executed);
    }

    [Fact]
    public async Task FlushAsync_EmptyQueue_NoOp()
    {
        var queue = new BrowserMutationQueue();

        // Should not throw.
        await queue.FlushAsync();
    }

    [Fact]
    public async Task FlushAsync_SwallowsJSDisconnectedException()
    {
        var queue = new BrowserMutationQueue();
        var failing = new ThrowingMutation
        {
            ElementId = "el-1",
            Exception = new JSDisconnectedException("gone"),
        };
        var after = new TestMutation { ElementId = "el-2" };

        queue.Enqueue(failing);
        queue.Enqueue(after);

        await queue.FlushAsync();

        Assert.True(after.Executed);
    }

    [Fact]
    public async Task FlushAsync_SwallowsOperationCanceledException()
    {
        var queue = new BrowserMutationQueue();
        var failing = new ThrowingMutation
        {
            ElementId = "el-1",
            Exception = new OperationCanceledException(),
        };
        var after = new TestMutation { ElementId = "el-2" };

        queue.Enqueue(failing);
        queue.Enqueue(after);

        await queue.FlushAsync();

        Assert.True(after.Executed);
    }

    [Fact]
    public async Task FlushAsync_RethrowsUnexpectedExceptions()
    {
        var queue = new BrowserMutationQueue();
        var failing = new ThrowingMutation
        {
            ElementId = "el-1",
            Exception = new InvalidOperationException("unexpected"),
        };

        queue.Enqueue(failing);

        await Assert.ThrowsAsync<InvalidOperationException>(() => queue.FlushAsync());
    }

    [Fact]
    public async Task FlushAsync_EnqueueDuringFlush_GoesToNextCycle()
    {
        var queue = new BrowserMutationQueue();
        var deferred = new TestMutation { ElementId = "el-deferred" };

        var trigger = new TestMutation
        {
            ElementId = "el-trigger",
            OnExecute = () =>
            {
                queue.Enqueue(deferred);
                return Task.CompletedTask;
            },
        };

        queue.Enqueue(trigger);

        // First flush: only trigger executes, deferred is enqueued during execution.
        await queue.FlushAsync();

        Assert.True(trigger.Executed);
        Assert.False(deferred.Executed);

        // Second flush: deferred executes.
        await queue.FlushAsync();

        Assert.True(deferred.Executed);
    }
}
