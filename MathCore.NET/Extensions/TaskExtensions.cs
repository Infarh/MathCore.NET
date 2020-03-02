// ReSharper disable AssignmentIsFullyDiscarded

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    internal static class TaskExtensions
    {
        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken Cancel)
        {
            if (!Cancel.CanBeCanceled)
                return task;
            if (Cancel.IsCancellationRequested)
                return Task.FromCanceled<T>(Cancel);

            return task.WithCancellationAwait(Cancel);
        }

        private static async Task<T> WithCancellationAwait<T>(this Task<T> task, CancellationToken Cancel)
        {
            var tcs = new TaskCompletionSource<T>();
            using (Cancel.Register(src => ((TaskCompletionSource<T>)src).TrySetCanceled(), tcs, false))
            {
                var result_task = await Task.WhenAny(task, tcs.Task);

                if (result_task != task)
                    _ = task.ContinueWith(_ => task.Exception, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

                return await result_task;
            }
        }

        public static Task<T> FromAsync<T>(
            this TaskFactory<T> factory,
            Func<AsyncCallback, object, IAsyncResult> BeginAction,
            Func<IAsyncResult, T> EndFunction,
            object State,
            CancellationToken Cancel)
        {
            if (Cancel.IsCancellationRequested)
                return Task.FromCanceled<T>(Cancel);

            T EndMethod(IAsyncResult result)
            {
                Cancel.ThrowIfCancellationRequested();
                return EndFunction(result);
            }

            return factory.FromAsync(BeginAction, EndMethod, State);
        }
    }
}
