// ReSharper disable AssignmentIsFullyDiscarded

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    internal static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken Cancel)
        {
            var tcs = new TaskCompletionSource<T>();
            Cancel.Register(() => tcs.TrySetCanceled(), false);
            var cancellation_task = tcs.Task;
            var result_task = await Task.WhenAny(task, cancellation_task);

            if (result_task == cancellation_task)
                _ = task.ContinueWith(_ => task.Exception, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            return await result_task;
        }
    }
}
