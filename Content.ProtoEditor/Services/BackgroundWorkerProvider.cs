using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Content.ProtoEditor.Services;

/// <summary>
/// Provides access to a worker thread with all IoC based dependencies initialized, so we can
/// make requests of those systems and resolve dependencies during them.
/// </summary>
public sealed class BackgroundWorkerProvider : IDisposable
{
    /// <summary>
    /// Simple single-thread SynchronizationContext for the worker.
    /// </summary>
    private sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {
        /// <summary>
        /// Queue containing the work to be done.
        /// </summary>
        private readonly BlockingCollection<(SendOrPostCallback, object?)> _queue = [];

        /// <summary>
        /// Adds a particular task to the queue.
        /// </summary>
        /// <param name="d">The callback for the work to start.</param>
        /// <param name="state">Any additional state required for the work.</param>
        public override void Post(SendOrPostCallback d, object? state)
        {
            _queue.Add((d, state));
        }

        /// <summary>
        /// Consumes and runs any tasks/work to be done on the thread.
        /// </summary>
        public void RunOnCurrentThread()
        {
            foreach (var (d, state) in _queue.GetConsumingEnumerable())
                d(state);
        }

        /// <summary>
        /// Closes the queue for any more work.
        /// </summary>
        public void Complete()
        {
            _queue.CompleteAdding();
        }
    }

    /// <summary>
    /// The dedicated thread to run tasks on.
    /// </summary>
    private readonly Thread _thread;

    /// <summary>
    /// The scheduler responsible for handling task queuing and running.
    /// </summary>
    private readonly TaskScheduler _scheduler;

    /// <summary>
    ///
    /// </summary>
    private readonly SingleThreadSynchronizationContext _synchronizationContext;
    private readonly CancellationTokenSource _cts = new();

    public BackgroundWorkerProvider(DependencyProvider assembly)
    {
        var schedulerTcs = new TaskCompletionSource<TaskScheduler>();

        _synchronizationContext = new SingleThreadSynchronizationContext();
        _thread = new Thread(() =>
        {
            Thread.CurrentThread.Name = "EditorWorkerThread";

            // Ensure we have the full dependency collection installed on this thread.
            assembly.InitializeForThread();

            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            schedulerTcs.SetResult(scheduler);

            _synchronizationContext.RunOnCurrentThread();
        })
        {
            IsBackground = true
        };

        _thread.Start();

        _scheduler = schedulerTcs.Task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Runs the given task asynchrounously on the dedicated background thread.
    /// </summary>
    /// <param name="func">The function, which returns a task, to run.</param>
    /// <returns>The awaitable task.</returns>
    public Task RunAsync(Func<Task> func)
    {
        return Task.Factory.StartNew(func, _cts.Token, TaskCreationOptions.None, _scheduler).Unwrap();
    }

    /// <summary>
    /// Runs the given task asynchrounously on the dedicated background thread.
    /// </summary>
    /// <param name="func">The function, which returns a task, to run.</param>
    /// <returns>The awaitable task.</returns>
    public Task<T> RunAsync<T>(Func<Task<T>> func)
    {
        return Task.Factory.StartNew(func, _cts.Token, TaskCreationOptions.None, _scheduler).Unwrap();
    }

    /// <summary>
    /// Disposes of this instance, cancelling any additional work and waiting for the last task
    /// to complete.
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
        _synchronizationContext?.Complete();
        _thread.Join();
        _cts.Dispose();
    }
}
