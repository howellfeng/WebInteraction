using System;
using System.Threading.Tasks;

namespace WebInteraction
{
    public static class TaskExtension
    {
        public static T WaitForResult<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
