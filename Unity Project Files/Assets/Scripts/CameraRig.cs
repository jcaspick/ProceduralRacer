using UnityEngine;
using System.Collections;

public class CameraRig : MonoBehaviour {

    Transform player;
    public float smoothing = 2f;

    Vector3 offset;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, player.position, smoothing * Time.deltaTime);
        transform.rotation = player.rotation;
    }
}
