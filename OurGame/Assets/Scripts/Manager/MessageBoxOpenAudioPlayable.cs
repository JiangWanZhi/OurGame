using Gravity.Utility;
using UnityEngine;
public class MessageBoxOpenAudioPlayable : MonoBehaviour, IGravityPlayable
{
    public AudioName audioName;

    private void Start()
    {

    }

    public void Play()
    {
        GameAudioManager.Instance.PlaySingleAudio(audioName);
    }

    public void Stop()
    {

    }

    public void Resume()
    {

    }

    public void Pause()
    {

    }
}
