using UnityEngine;

public class SpinScript : MonoBehaviour
{
    [SerializeField] float spinRate;

    void Update()
    {
        transform.Rotate(Vector3.up * spinRate * Time.deltaTime);
    }
}
