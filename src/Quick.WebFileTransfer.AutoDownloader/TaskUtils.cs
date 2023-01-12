using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.WebFileTransfer.AutoDownloader
{
    public class TaskUtils
    {
        //检查时间间隔
        private readonly static TimeSpan checkTimeSpan = TimeSpan.FromMinutes(1);

        private static void doRunAtTime(DateTime time, Task theTask)
        {
            var leftTimeSpan = time - DateTime.Now;
            //如果已经到了执行时间
            if (leftTimeSpan <= TimeSpan.Zero)
                theTask.Start();
            //如果执行时间离现在还有1分钟以上
            else if (leftTimeSpan > checkTimeSpan)
                Task.Delay(checkTimeSpan)
                    .ContinueWith(t1 => doRunAtTime(time, theTask));
            else
                Task.Delay(leftTimeSpan)
                    .ContinueWith(t1 => theTask.Start());
        }

        private static void doRunAtTime(DateTime time, Task theTask, CancellationToken cancellationToken)
        {
            var leftTimeSpan = time - DateTime.Now;
            //如果已经到了执行时间
            if (leftTimeSpan <= TimeSpan.Zero)
                theTask.Start();
            //如果执行时间离现在还有1分钟以上
            else if (leftTimeSpan > checkTimeSpan)
                Task.Delay(checkTimeSpan, cancellationToken)
                    .ContinueWith(t1 =>
                    {
                        if (t1.IsCanceled)
                            return;
                        doRunAtTime(time, theTask, cancellationToken);
                    });
            else
                Task.Delay(leftTimeSpan, cancellationToken)
                    .ContinueWith(t1 =>
                    {
                        if (t1.IsCanceled)
                            return;
                        theTask.Start();
                    });
        }

        /// <summary>
        /// 在指定的时间运行
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task RunAtTime(DateTime time, Action action)
        {
            Task theTask = new Task(action);
            doRunAtTime(time, theTask);
            return theTask;
        }

        /// <summary>
        /// 在指定的时间运行，带取消令牌
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task RunAtTime(DateTime time, Action action, CancellationToken cancellationToken)
        {
            Task theTask = new Task(action, cancellationToken);
            doRunAtTime(time, theTask, cancellationToken);
            return theTask;
        }

        public static Task RunTaskPool(int maxTaskCount, params Action[] actions)
        {
            Queue<Task> taskQueue = new Queue<Task>();
            foreach (var action in actions)
            {
                var task = new Task(action);
                taskQueue.Enqueue(task);
            }
            var rtnTask = Task.WhenAll(taskQueue.ToArray());
            Task.Run(() =>
            {
                var finishedTask = Task.FromResult(0);
                Action<Task> runNextTask = null;
                runNextTask = prTask =>
                {
                    Task nextTask = null;
                    lock (taskQueue)
                    {
                        if (taskQueue.Count == 0)
                            return;
                        nextTask = taskQueue.Dequeue();
                    }
                    nextTask.Start();
                    nextTask.ContinueWith(t => runNextTask?.Invoke(t));
                };
                for (var i = 0; i < maxTaskCount; i++)
                    runNextTask(finishedTask);
            });
            return rtnTask;
        }

        public static async Task<Task> TaskWait(Task task, int timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var retTask = await Task.WhenAny(task, timeoutTask);
            if (retTask == timeoutTask)
                throw new TimeoutException();
            return retTask;
        }
    }
}
