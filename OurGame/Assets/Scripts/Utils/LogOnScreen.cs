using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Consolation
{
    /// <summary>  
    /// A console to display Unity's debug logs in-game.  
    /// </summary>  
    class LogOnScreen : MonoBehaviour
    {
        struct Log
        {
            public string message;
            public string stackTrace;
            public LogType type;
        }

        public bool acceptErrorAndException = true;
        #region Inspector Settings  
        /// <summary>  
        /// Whether to open the window by shaking the device (mobile-only).  
        /// </summary>  
        public bool shakeToOpen = true;

        /// <summary>  
        /// The (squared) acceleration above which the window should open.  
        /// </summary>  
        public float shakeAcceleration = 3f;

        /// <summary>  
        /// Number of logs to keep before removing old ones.  
        /// </summary>  
        int maxLogs = 100;

        #endregion

        readonly List<Log> logs = new List<Log>();
        Vector2 scrollPosition;
        bool visible;

        // Visual elements:  

        static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
        {
            { LogType.Assert, Color.white },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
            { LogType.Log, Color.white },
            { LogType.Warning, Color.yellow },
        };

        const string windowTitle = "Console";
        const int margin = 20;
        static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");

        readonly Rect titleBarRect = new Rect(0, 0, 10000, 20);
        Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));

        
        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }
        static float switchInterval = 2;
        static float switchElapsed = 2;
        float fpsAccumulate = 0;
        float fps = 0;
        float fpsTime = 0;
        float pressTime;

        void Update()
        {
            if (Input.GetMouseButton(0) && pressTime < 2)
// #if UNITY_EDITOR || UNITY_EDITOR_WIN
//             if (Input.GetMouseButton(0) && pressTime < 2)
// #else
//             if (Input.touchCount > 0 && pressTime < 2)
// #endif
            {
                pressTime += Time.unscaledDeltaTime;
            }
            else
            {
                pressTime = 0;
            }

            fpsAccumulate++;
            fpsTime += Time.deltaTime;
            if (fpsTime > 1)
            {
                fpsTime -= 1;
                fps = fpsAccumulate;
                fpsAccumulate = 0;
            }
            switchElapsed += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                visible = !visible;
                acceptErrorAndException = true;
            }

            if (shakeToOpen && Input.acceleration.sqrMagnitude > shakeAcceleration && Input.touchCount == 1)
            {
                if (switchElapsed > switchInterval)
                {
                    switchElapsed = 0;
                    visible = !visible;
                    acceptErrorAndException = true;
                }
            }
        }

        void OnGUI()
        {
            if (!visible)
            {
                return;
            }
            windowRect = GUILayout.Window(123456, windowRect, DrawConsoleWindow, windowTitle);
        }

        /// <summary>  
        /// Displays a window that lists the recorded logs.  
        /// </summary>  
        /// <param name="windowID">Window ID.</param>  
        void DrawConsoleWindow(int windowID)
        {
            if (labelStytle == null)
            {
                labelStytle = new GUIStyle(GUI.skin.label);
                labelStytle.fontSize = 30;
            }
            GUILayout.Label($"FPS:{fps}", labelStytle);
            DrawLogsList();
            DrawToolbar();

            // Allow the window to be dragged by its title bar.  
            GUI.DragWindow(titleBarRect);
        }

        /// <summary>  
        /// Displays a scrollable list of logs.  
        /// </summary>  

        GUIStyle labelStytle = null;
        void DrawLogsList()
        {
            float originValue = GUI.skin.verticalScrollbar.fixedWidth;
            GUI.skin.verticalScrollbar.fixedWidth = 60;
            GUI.skin.verticalScrollbarThumb.fixedWidth = 60;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // Iterate through the recorded logs.  
            for (var i = logs.Count - 1; i >= 0; i--)
            {
                var log = logs[i];
                labelStytle.normal.textColor = logTypeColors[log.type];
                GUILayout.Label(log.message, labelStytle);
                if (log.type == LogType.Error || log.type == LogType.Assert || log.type == LogType.Exception)
                {
                    GUILayout.Label(log.stackTrace, labelStytle);
                }
            }

            GUILayout.EndScrollView();
            GUI.skin.verticalScrollbar.fixedWidth = originValue;
            GUI.skin.verticalScrollbarThumb.fixedWidth = originValue;
            // Ensure GUI colour is reset before drawing other components.  
            GUI.contentColor = Color.white;
        }

        /// <summary>  
        /// Displays options for filtering and changing the logs list.  
        /// </summary>  
        void DrawToolbar()
        {
            GUILayout.BeginHorizontal();

            float originValue = GUI.skin.button.fixedHeight;
            GUI.skin.button.fixedHeight = 60;
            if (GUILayout.Button(clearLabel))
            {
                logs.Clear();
            }
            GUI.skin.button.fixedHeight = originValue;

            GUILayout.EndHorizontal();
        }

        /// <summary>  
        /// Records a log from the log callback.  
        /// </summary>  
        /// <param name="message">Message.</param>  
        /// <param name="stackTrace">Trace of where the message came from.</param>  
        /// <param name="type">Type of message (error, exception, warning, assert).</param>  
        void HandleLog(string message, string stackTrace, LogType type)
        {
            if (acceptErrorAndException)
            {
                logs.Add(new Log
                {
                    message = message,
                    stackTrace = stackTrace,
                    type = type,
                });
            }
            if (type == LogType.Error || type == LogType.Exception)
            {
                visible = true;
                acceptErrorAndException = false;
            }
            TrimExcessLogs();
        }

        /// <summary>  
        /// Removes old logs that exceed the maximum number allowed.  
        /// </summary>  
        void TrimExcessLogs()
        {
            var amountToRemove = Mathf.Max(logs.Count - maxLogs, 0);
            if (amountToRemove == 0)
            {
                return;
            }

            logs.RemoveRange(0, amountToRemove);
        }
    }
}