using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace LingeringHopes.UI
{
    [Serializable]
    struct VideoTrigger
    {
        [SerializeField]
        string triggerTag;

        [SerializeField]
        VideoClip clip;

        public VideoClip Clip => clip;

        public bool IsTriggered(string word) => string.Equals(triggerTag, word, StringComparison.OrdinalIgnoreCase);
    }

    public class BackgroundVideo : FadeablePanel
    {

        [Header("Video Configuration")]
        [SerializeField]
        VideoTrigger[] videoTriggers = null;

        [SerializeField]
        VideoClip defaultVideo = null;

        [Space(10)]
        [SerializeField]
        VideoPlayer videoPlayer = null;

        VideoClip currentClip = null;

        Coroutine currentRoutine = null;

        bool updatePending = false;

        protected override bool IsValid() => base.IsValid() && videoTriggers != null && videoTriggers.Length > 0 && defaultVideo && videoPlayer;

        void OnEnable()
        {
            if (updatePending)
            {
                UpdateVideoPlayer();
            }
        }

        public void SelectNewVideoBasedOnTag(string environmentTag)
        {
            // If we already have selected a clip for this AI's response then ignore this step.
            if (currentClip)
            {
                return;
            }

            foreach (VideoTrigger trigger in videoTriggers)
            {
                if (trigger.IsTriggered(environmentTag))
                {
                    currentClip = trigger.Clip;
                    UpdateVideoPlayer();
                    return;
                }
            }
        }

        void UpdateVideoPlayer()
        {
            if (!gameObject.activeInHierarchy)
            {
                updatePending = true;
                return;
            }

            updatePending = false;

            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            currentRoutine = StartCoroutine(ChangeVideoWithFadeRoutine());
        }

        IEnumerator ChangeVideoWithFadeRoutine()
        {
            if (currentClip && currentClip != videoPlayer.clip)
            {
                yield return HideFadeCoroutine();

                videoPlayer.Stop();
                videoPlayer.clip = currentClip;
                videoPlayer.Prepare();

                while (!videoPlayer.isPrepared)
                    yield return null;

                videoPlayer.Play();
                yield return ShowFadeCoroutine();
            }
        }
    }

}