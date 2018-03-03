using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _3rdParty.AsyncAwaitUtil.Source {
    public static class TaskExtensions
    {
        public static IEnumerator AsIEnumerator(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }

        public static IEnumerator<T> AsIEnumerator<T>(this Task<T> task)
            where T : class
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }

            yield return task.Result;
        }
    }
}
