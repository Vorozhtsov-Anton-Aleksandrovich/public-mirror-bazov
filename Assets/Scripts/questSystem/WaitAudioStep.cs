using System.Collections;
using UnityEngine;

public class WaitAudioStep : WaitStep
{
    [SerializeField]
    private AudioSource _audio;

    [SerializeField]
    private float StartDelay = 0.0f;

    public IEnumerator DelayedCoroutineStart()
    {
        yield return new WaitForSeconds(StartDelay);
        delay = _audio.clip.length;
        _audio.Play();
        StartCoroutine(DelayedCoroutine());
    }
    
    private void OnEnable()
    {
        StartCoroutine(DelayedCoroutineStart());
    }
}
