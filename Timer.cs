

using UnityEngine;
using UnityEngine.UI;

namespace MuonhoryoLibrary.Unity
{
    public class Timer : MonoBehaviour
    {
        [SerializeField]private Text TimerText;

        public string CurrentTimerTime_ => TimerText.text;
        public float StartTime_ { get; private set; }

        protected string GetTimeText(float elapsedTime)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60) % 60;
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            int milliseconds = Mathf.FloorToInt((elapsedTime * 1000) % 1000);

            return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
        private void Update()
        {
            float elapsedTime = Time.time - StartTime_;
            TimerText.text= GetTimeText(elapsedTime);
        }

        private void Start()
        {
            OnEnable();
        }

        private void OnEnable()
        {
            StartTime_ = Time.time;
        }
    }
}
