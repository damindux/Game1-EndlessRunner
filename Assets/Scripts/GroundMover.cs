using UnityEngine;

public class GroundMover : MonoBehaviour
{
    private void Update()
    {
        transform.Translate(new Vector3(-3f, 0) * Time.deltaTime);
    }
}
