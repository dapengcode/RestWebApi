using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWebApi.Tools
{
    /// <summary>
    /// 表示一个空的任务 
    /// </summary>
    static class EmptyTask
    {
        static readonly Task Null = new Task(() => { });

        public static bool IsNull(this Task task)
        {
            return task == null || ReferenceEquals(task, Null);
        }

        public static bool IsNull<T>(this Task<T> task)
        {
            return task == null || ReferenceEquals(task, EmptyTask<T>.Null);
        }
    }
    /// <summary> 
    /// 表示一个空的任务
    /// </summary>
    static class EmptyTask<T>
    {
        public static readonly Task<T> Null = Task.FromResult(default(T));
    }
}
