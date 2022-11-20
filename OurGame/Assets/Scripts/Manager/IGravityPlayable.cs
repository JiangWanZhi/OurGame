
using System;
using UnityEngine.Events;

namespace Gravity.Utility
{
    public enum PlayableAction
    {
        Play,
        Stop,
        Pause,
        Resume,
        Callback,
        MapEffectPlay,
    }

    [Serializable]
    public class PlayableTrigger : UnityEvent<PlayableAction>
    {

    }

    public interface IPlayableState
    {
        void SetState(int state);
    }

    public interface IGravityPlayable
    {
        void Play();
        void Stop();

        void Resume();

        void Pause();
    }
}
