using System.Diagnostics;
using UnityEngine;

public static class LogUtility
    {
        [Conditional("UNITY_EDITOR")]
        public static void InfoEditor(object message, Object context = null) => Info(message, context);

        [Conditional("UNITY_EDITOR")]
        public static void InfoFormatEditor(string format, params object[] messages) => InfoFormat(format, messages);

        [Conditional("UNITY_EDITOR")]
        public static void InfoFormatEditor(Object context, string format, params object[] messages) => InfoFormat(context, format, messages);

        public static void Info(object message, Object context = null) => Info_Debug(message, context);

        public static void InfoFormat(string format, params object[] messages) => Info_Debug_Format(null, format, messages);

        public static void InfoFormat(Object context, string format, params object[] messages) => Info_Debug_Format(context, format, messages);





        [Conditional("UNITY_EDITOR")]
        public static void WarningEditor(object message, Object context = null) => Warning(message, context);

        [Conditional("UNITY_EDITOR")]
        public static void WarningFormatEditor(string format, params object[] messages) => WarningFormat(format, messages);

        [Conditional("UNITY_EDITOR")]
        public static void WarningFormatEditor(Object context, string format, params object[] messages) => WarningFormat(context, format, messages);

        public static void Warning(object message, Object context = null) => Warring_Debug(message, context);

        public static void WarningFormat(string format, params object[] messages) => Warring_Debug_Format(null, format, messages);

        public static void WarningFormat(Object context, string format, params object[] messages) => Warring_Debug_Format(context, format, messages);





        [Conditional("UNITY_EDITOR")]
        public static void ErrorEditor(object message, Object context = null) => Error(message, context);

        [Conditional("UNITY_EDITOR")]
        public static void ErrorFormatEditor(string format, params object[] messages) => ErrorFormat(format, messages);

        [Conditional("UNITY_EDITOR")]
        public static void ErrorFormatEditor(Object context, string format, params object[] messages) => ErrorFormat(context, format, messages);

        public static void Error(object message, Object context = null) => Error_Debug(message, context);

        public static void ErrorFormat(string format, params object[] messages) => Error_Debug_Format(null, format, messages);

        public static void ErrorFormat(Object context, string format, params object[] messages) => Error_Debug_Format(context, format, messages);


        private static void Info_Debug(object message, Object context)
        {
            if (context)
                UnityEngine.Debug.Log(message, context);
            else
                UnityEngine.Debug.Log(message);
        }

        private static void Info_Debug_Format(Object context, string format, params object[] messages)
        {
            if (context)
                UnityEngine.Debug.LogFormat(context, format, messages);
            else
                UnityEngine.Debug.LogFormat(format, messages);
        }

        private static void Warring_Debug(object message, Object context)
        {
            if (context)
                UnityEngine.Debug.LogWarning(message, context);
            else
                UnityEngine.Debug.LogWarning(message);
        }

        private static void Warring_Debug_Format(Object context, string format, params object[] messages)
        {
            if (context)
                UnityEngine.Debug.LogWarningFormat(context, format, messages);
            else
                UnityEngine.Debug.LogWarningFormat(format, messages);
        }

        private static void Error_Debug(object message, Object context)
        {
            if (context)
                UnityEngine.Debug.LogError(message, context);
            else
                UnityEngine.Debug.LogError(message);
        }

        private static void Error_Debug_Format(Object context, string format, params object[] messages)
        {
            if (context)
                UnityEngine.Debug.LogErrorFormat(context, format, messages);
            else
                UnityEngine.Debug.LogErrorFormat(format, messages);
        }
    }