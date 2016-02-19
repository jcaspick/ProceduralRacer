using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{

    Transform pivot;
    Transform player;
    public float smoothing = 5f;

    Vector3 offset;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pivot = GetComponentInParent<Transform>();
    }

    void Update()
    {
        pivot.position = player.position;
        pivot.rotation = player.rotation;
    }
}