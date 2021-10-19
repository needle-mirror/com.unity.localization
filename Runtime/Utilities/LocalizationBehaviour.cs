using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.Localization
{
    class LocalizationBehaviour : ComponentSingleton<LocalizationBehaviour>
    {
        List<AsyncOperationHandle> m_CurrentReleaseHandles;
        int m_ReleaseFrame = -1;

        protected override string GetGameObjectName() => "Localization Resource Manager";

        /// <summary>
        /// To prevent you having to explicitly release operations, Unity does this automatically a frame after the operation is completed.
        /// If you plan to keep hold of a reference, call <see cref="AsyncOperationHandle.Acquire"/>, and <see cref="AsyncOperationHandle.Release"/> when it's finished.
        /// </summary>
        /// <param name="handle"></param>
        public static void ReleaseNextFrame(AsyncOperationHandle handle) => Instance.DoReleaseNextFrame(handle);

        void DoReleaseNextFrame(AsyncOperationHandle handle)
        {
            if (Time.frameCount != m_ReleaseFrame)
            {
                // Start a new release coroutine
                m_ReleaseFrame = Time.frameCount;
                m_CurrentReleaseHandles = ListPool<AsyncOperationHandle>.Get();
                m_CurrentReleaseHandles.Add(handle);
                StartCoroutine(ReleaseHandlesNextFrame(m_CurrentReleaseHandles));
            }
            else
            {
                // Queue up to the next release call
                m_CurrentReleaseHandles?.Add(handle);
            }
        }

        static IEnumerator ReleaseHandlesNextFrame(List<AsyncOperationHandle> handles)
        {
            // Defer to the next frame
            yield return null;

            foreach (var h in handles)
            {
                AddressablesInterface.SafeRelease(h);
            }
            ListPool<AsyncOperationHandle>.Release(handles);
        }
    }
}
