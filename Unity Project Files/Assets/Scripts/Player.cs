using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    ArcSegment currentArc;
    Vector3 movement;
    Rigidbody rb;

    Quaternion steering;
    Vector3 velocity = Vector3.zero;
    Vector3 gravity = Vector3.zero;
    float fallSpeed = 0;

    float distance = 0;
    public float groundHeight;
    public bool onGround = true;

    float turnGoal;
    float turnRate = 0;
    public float maxTurnRate;
    public float turnAcceleration;

    float speed = 0;
    float minSpeed = 2f;
    public float maxSpeed;
    public float acceleration;

    float h, v;

    public void Initialize()
    {
        currentArc = TrackManager.instance.currentArc;
    }

    void Update()
    {
        if (!GameManager.instance.paused) {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");

            Move(h, v);

            CheckBounds();
            UpdateDistance();
        }
    }

    void Move(float h, float v)
    {
        turnGoal = h * maxTurnRate;
        if(Mathf.Abs(turnGoal) < 0.2f)
        {
            turnGoal = 0;
        }

        if (onGround)
        {
            if (turnRate < turnGoal)
            {
                turnRate += turnAcceleration;
            }
            else if (turnRate > turnGoal)
            {
                turnRate -= turnAcceleration;
            }
            if (turnGoal == 0)
            {
                turnRate /= 1.5f;
            }

            turnRate = Mathf.Clamp(turnRate, -maxTurnRate, maxTurnRate);

            steering = Quaternion.AngleAxis(turnRate, Vector3.up);
        }

        if(v > 0)
        {
            if (speed <= maxSpeed)
            {
                speed += v * acceleration;
            }
        } else if (speed > minSpeed)
        {
            speed -= acceleration/2;
            if(Mathf.Abs(speed) < minSpeed + 0.05f)
            {
                speed = minSpeed;
            }
        }

        velocity = (transform.rotation * Vector3.forward).normalized * speed;
        if (!onGround)
        {
            fallSpeed -= 0.04f;
            gravity.Set(0, fallSpeed, 0);
            velocity = velocity + gravity;
            transform.rotation = transform.rotation * Quaternion.AngleAxis(0.6f, Vector3.right);
        }

        transform.rotation = transform.rotation * steering;
        transform.position = transform.position + velocity * Time.deltaTime;
    }

    void CheckBounds()
    {
        if (currentArc.ContainsPoint(transform.position, 0.05f))
        {
            //onGround = true;
        }
        else
        {
            if (currentArc.nextArc != null && currentArc.nextArc.ContainsPoint(transform.position, 0.05f))
            {
                TrackManager.instance.IncrementArc();
                currentArc = TrackManager.instance.currentArc;
            }
            else
            {
                onGround = false;
            }
        }
    }

    void UpdateDistance()
    {
        if (!onGround)
        {
            return;
        }
        distance = currentArc.GetDistance(currentArc.WorldToArc(transform.position).t);
        groundHeight = distance * TrackManager.instance.slope;
        TrackManager.instance.OffsetHeight(groundHeight);
        GameManager.instance.UpdateScore(distance);
    }
}