using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NanaCiel
{
    static class TaskExtensions
    {
        public static async Task OnError(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public static async Task OnError(this Task task, Action<Exception> onError)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                onError(e);
            }
        }

        public static async Task<T> OnError<T>(this Task<T> task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            return await task;
        }

        public static async Task<T> OnError<T>(this Task<T> task, Action<Exception> onError)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                onError(e);
            }
            return await task;
        }
    }
}