using UnityEngine;
using UniRx;
using UnityFx.Async.Promises;
using uAdventure.Runner;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Simva;
using UnityFx.Async;

namespace uAdventure.Simva
{
    // Manager for "Simva.End"
    public class BackupController : MonoBehaviour, IRunnerChapterTarget
    {
        private bool ready;
        public UnityEngine.UI.Scrollbar progressBar;

        public object Data { get { return null; } set { } }

        public bool IsReady { get { return ready; } }

        protected void OnApplicationResume()
        {
        }

        public void Update()
        {
            if(!ready && SimvaExtension.Instance.saveActivityAndContinueOperation != null)
            {
                Debug.Log("Last progress: " + SimvaExtension.Instance.saveActivityAndContinueOperation.Progress);
                ((AsyncCompletionSource)SimvaExtension.Instance.saveActivityAndContinueOperation).AddProgressCallback((p) =>
                {
                    progressBar.size = p;
                });
                ready = true;
            }
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void RenderScene()
        {
            InventoryManager.Instance.Show = false;
        }

        public void Destroy(float time, Action onDestroy)
        {
            GameObject.DestroyImmediate(this.gameObject);
            onDestroy();
        }

        public InteractuableResult Interacted(PointerEventData pointerData = null)
        {
            return InteractuableResult.IGNORES;
        }

        public bool canBeInteracted()
        {
            return false;
        }

        public void setInteractuable(bool state)
        {
        }
    }
}

