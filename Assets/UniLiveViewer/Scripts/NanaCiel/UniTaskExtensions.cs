using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace NanaCiel
{
    static class UniTaskExtensions
    {
        public static async UniTask OnError(this UniTask task)
        {
            try
            {
                await task;
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                Debug.Log(e.Message);
            }
        }

        public static async UniTask OnError(this UniTask task, Action<Exception> onError)
        {
            try
            {
                await task;
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                onError(e);
            }
        }

        public static async UniTask OnError<T>(this UniTask<T> task)
        {
            try
            {
                await task;
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                Debug.Log(e.Message);
            }
        }

        public static async UniTask<T> OnError<T>(this UniTask<T> task, Action<Exception> onError)
        {
            try
            {
                await task;
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                onError(e);
            }
            return await task;
        }
    }
}