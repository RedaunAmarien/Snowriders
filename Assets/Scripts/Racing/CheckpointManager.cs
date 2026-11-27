using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public List<Checkpoint> checkpoints;

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
        }
    }
}
