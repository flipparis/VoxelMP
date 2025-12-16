using UnityEngine;
using Mirror;

public class SpawnOnGround : NetworkBehaviour
{
    public float castHeight = 50f;
    public float maxDrop = 200f;
    public float groundClearance = 1.5f;

    public override void OnStartServer()
    {
        // Only the server should correct spawn position
        Vector3 start = transform.position + Vector3.up * castHeight;

        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, maxDrop))
        {
            transform.position = hit.point + Vector3.up * groundClearance;
        }
        else
        {
            // Fallback: at least pop up
            transform.position += Vector3.up * 10f;
        }
    }
}
