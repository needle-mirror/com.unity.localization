using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.Localization
{
    /// <summary>
    /// In order to avoid users having to explicitly release operations we do this automatically a frame after the operation has completed.
    /// This means that if someone does plan to keep hold of a reference then they will need to call Acquire on it and Release when they have finished.
    /// </summary>
    class OperationHandleDeferedRelease : ComponentSingleton<OperationHandleDeferedRelease>
    {
        List<AsyncOperationHandle> m_CurrentReleaseHandles;
        int m_ReleaseFrame = -1;

        protected override string GetGameObjectName() => "Localization Resource Manager";

        public void ReleaseNextFrame(AsyncOperationHandle handle)
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
                m_CurrentReleaseHandles.Add(handle);
            }
        }

        static IEnumerator ReleaseHandlesNextFrame(List<AsyncOperationHandle> handles)
        {
            // Defer to the next frame
            yield return null;

            foreach (var h in handles)
            {
                AddressablesInterface.Release(h);
            }
            ListPool<AsyncOperationHandle>.Release(handles);
        }
    }
}
