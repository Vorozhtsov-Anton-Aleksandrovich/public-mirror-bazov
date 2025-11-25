using System.Collections;
using UnityEngine;

public class WaitStep : QuestStep
{
    [SerializeField]
    public float delay;

    private void OnEnable()
    {
        StartCoroutine(DelayedCoroutine());
    }
    
    public IEnumerator DelayedCoroutine()
    {
        yield return new WaitForSeconds(delay);
        FinishStep();
    }
}
