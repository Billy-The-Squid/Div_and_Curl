using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [SerializeField]
    public Transform playerEyes;

    // Update is called once per frame
    void Update()
    {
        transform.forward = (transform.position - playerEyes.position).normalized;
    }
}
