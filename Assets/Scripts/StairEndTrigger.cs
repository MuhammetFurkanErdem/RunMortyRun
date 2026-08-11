using UnityEngine;

public class StairEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (FinishLine.Instance != null)
        {
            FinishLine.Instance.CompleteRunAfterStairs();
        }
    }
}