using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed;
    [SerializeField] private float left, right, top, bottom;

    private void Update()
    {
        Vector3 p = new Vector3(target.position.x, target.position.y, transform.position.z);

        float x = Mathf.Clamp(target.position.x, left, right);
        float y = Mathf.Clamp(target.position.y, bottom, top);

        p = new Vector3(x, y, p.z);

        transform.position = Vector3.Lerp(transform.position, p, speed * Time.deltaTime);


    }
}
