using UnityEngine;

public class StaticObjZCalculator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * .01f);
    }
}
