using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour {

    public static TrackManager instance;

    public ArcSegment arcPrefab;
    public Transform player;

    Vector3 nextArcPos = Vector3.zero;
    Vector3 nextArcHeightOffset = Vector3.zero;
    float nextArcAngle = 0;
    float nextArcUVOffset = 0;
    float textureScale = 12;

    public ArcSegment currentArc;
    private ArcSegment furthestArc;
    public float furthestDistance = 0;
    public float slope = -0.08f;

    List<ArcSegment> track;
    Transform trackHolder;
    Vector3 trackYOffset = Vector3.zero;

    float minRadius = 7;
    float maxRadius = 22;
    float minAngle = 30;
    float maxAngle = 160;

    float meshQuality = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void InitializeTrack()
    {
        trackHolder = new GameObject("Track").transform;

        track = new List<ArcSegment>();
        while((nextArcPos.getFlat() - player.position.getFlat()).magnitude < 50)
        {
            AddArc();
        }

        currentArc = track[0];
    }

    void Update()
    {
        for(int i = 0; i < track.Count; i++)
        {
            if(track[i].distanceOffset + track[i].totalDistance < GameManager.instance.maxDistance - 2)
            {
                track[i].transform.SetParent(null);
                track[i].ResetArc();
                track[i].gameObject.SetActive(false);
            }
        }

        if((nextArcPos.getFlat() - player.position).magnitude < 35)
        {
            AddArc();
        }
    }

    void AddArc()
    {
        ArcSegment newSegment = GetPooledArc();

        newSegment.transform.position = nextArcPos;
        newSegment.startAngle = nextArcAngle;
        newSegment.width = 1.5f;
        newSegment.slope = slope;

        float randomRadius = Random.Range(minRadius, maxRadius);
        float randomAngle = Random.Range(minAngle, maxAngle);
        newSegment.radius = randomRadius;
        newSegment.angle = randomAngle;
        if(Random.value > 0.5)
        {
            newSegment.flipped = true;
        }
        else
        {
            newSegment.flipped = false;
        }

        newSegment.Initialize(); // calculates the total length of the arc and identifies it's end position and center of rotation

        newSegment.distanceOffset = furthestDistance;
        newSegment.uvOffset = nextArcUVOffset;
        newSegment.textureScale = textureScale;

        furthestDistance += newSegment.totalDistance;
        nextArcHeightOffset.Set(0, furthestDistance * slope, 0);
        nextArcAngle = newSegment.endAngle;
        nextArcPos = newSegment.endPos;
        nextArcUVOffset += newSegment.totalDistance / textureScale - Mathf.Floor(newSegment.totalDistance / textureScale);
        if(nextArcUVOffset >= 1)
        {
            nextArcUVOffset = nextArcUVOffset - 1;
        }

        if (furthestArc != null)
        {
            furthestArc.nextArc = newSegment; // each arc contains a reference to the next arc in the track
        }
        furthestArc = newSegment;

        newSegment.arcDivisions = Mathf.RoundToInt(newSegment.totalDistance / meshQuality); // adjust mesh divisions based on the length of the arc
        newSegment.GenerateMesh();

        track.Add(newSegment);
        trackHolder.position = Vector3.zero;
        newSegment.transform.SetParent(trackHolder);
        trackHolder.position = trackYOffset;

        newSegment.gameObject.SetActive(true);
    }

    ArcSegment GetPooledArc()
    {
        for (int i = 0; i < track.Count; i++)
        {
            if (!track[i].gameObject.activeInHierarchy)
            {
                return track[i];
            }
        }

        // if none is found create a new object only if the pool has not exceeded its hard cap

        ArcSegment newSegment = Instantiate(arcPrefab) as ArcSegment;
        return newSegment;
    }

    public void OffsetHeight(float offset) // the entire track actually moves up in the Y axis, instead of the player descending.  Keeps player from straying ever further from origin.
    {
        trackYOffset.Set(0, -offset, 0);
        trackHolder.position = trackYOffset;
    }

    public void IncrementArc()
    {
        ArcSegment nextArc = currentArc.nextArc;
        currentArc = nextArc;
    }
}
