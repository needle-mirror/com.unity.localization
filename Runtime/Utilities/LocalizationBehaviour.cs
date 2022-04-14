using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.Localization
{
    class LocalizationBehaviour : ComponentSingleton<LocalizationBehaviour>
    {
        Queue<(int frame, AsyncOperationHandle handle)> m_ReleaseQueue = new Queue<(int, AsyncOperationHandle)> ();

        protected override string GetGameObjectName() => "Localization Resource Manager";

        /// <summary>
        /// To prevent you having to explicitly release operations, Unity does this automatically a frame after the operation is completed.
        /// If you plan to keep hold of a reference, call <see cref="AsyncOperationHandle.Acquire"/>, and <see cref="AsyncOperationHandle.Release"/> when it's finished.
        /// </summary>
        /// <param name="handle"></param>
        public static void ReleaseNextFrame(AsyncOperationHandle handle) => Instance.DoReleaseNextFrame(handle);

        void DoReleaseNextFrame(AsyncOperationHandle handle)
        {
            enabled = true;
            m_ReleaseQueue.Enqueue((Time.frameCount, handle));
        }

        void LateUpdate()
        {
            var currentFrame = Time.frameCount;
            while(m_ReleaseQueue.Count > 0 && m_ReleaseQueue.Peek().frame < currentFrame)
            {
                var item = m_ReleaseQueue.Dequeue();
                AddressablesInterface.SafeRelease(item.handle);
            }

            if (m_ReleaseQueue.Count == 0)
                enabled = false;
        }
    }
}
