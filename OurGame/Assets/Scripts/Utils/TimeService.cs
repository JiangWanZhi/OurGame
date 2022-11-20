using System;
using System.Collections;
using UnityEngine;

    public class TimeService : MonoBehaviour
    {
        public Action OnSecondEvent;

        public long ServerTimestamp { get; set; }

        #region Singleton

        private static TimeService instance;

        public static TimeService Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType(typeof(TimeService)) as TimeService;

                    if (!instance)
                    {
                        LogUtility.Info("Can not find TimeService instance");
                    }

                    if (instance != null)
                    {
                        //ILRuntimeService.Instance.OnServerTime = instance.OnServerTime;
                    }
                }

                return instance;
            }
        }

        private void Awake()
        {
            ServerTimestamp = (long)((DateTime.UtcNow - TimeUtility.baeTime).TotalSeconds);
        }

        private void Start()
        {
            InvokeRepeating("OnSecondReached", 1, 1);
        }

        void OnDisable()
        {
            instance = null;
        }

        private void OnSecondReached()
        {
            ServerTimestamp++;
            if (OnSecondEvent != null)
            {
                OnSecondEvent.Invoke();
            }
        }

        private void OnServerTime(long timestamp)
        {
            ServerTimestamp = timestamp;
        }

        #endregion

        public Coroutine StartTask(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        public void StopTask(IEnumerator routine)
        {
            StopCoroutine(routine);
        }

        public void StopTask(object coroutine)
        {
            StopCoroutine((Coroutine)coroutine);
        }

        public Coroutine StartDelayAction(Action call, float delay)
        {
            return StartTask(CreateScheduledAction(call, delay));
        }

        public IEnumerator CreateScheduledAction(Action action, TimeSpan delay, bool realTime = true)
        {
            // wait until
            float s = (float)delay.TotalSeconds;
            if (realTime)
                yield return new WaitForSeconds(s);
            else
                yield return new WaitForSeconds(s);

            // invoke action
            action.Invoke();
        }

        public IEnumerator CreateScheduledAction(Action action, float seconds, bool realTime = true)
        {
            if (realTime)
                yield return new WaitForSeconds(seconds);
            else
                yield return new WaitForSeconds(seconds);

            // invoke action
            action.Invoke();
        }

        public IEnumerator CreateScheduledAction(IEnumerator handler, Action action)
        {
            // wait until
            yield return handler;

            // invoke action
            action.Invoke();
        }
    }

