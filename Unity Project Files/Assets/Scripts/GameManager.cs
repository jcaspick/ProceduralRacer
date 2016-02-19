using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    float deathTimer = 0;

    public static GameManager instance;
    ArcSegment currentArc;
    public Player player;
    public Animator screenFader;
    public Text distanceText;

    public float maxDistance = 0;
    float playerHeight = 0;

    public bool paused = true;

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
    }

    void Start()
    {
        TrackManager.instance.InitializeTrack();
        currentArc = TrackManager.instance.currentArc;
        player.transform.position = currentArc.ArcToWorld(new ArcPoint(20, currentArc.radius)).getFlat();
        player.transform.rotation = Quaternion.AngleAxis(currentArc.flipped ? 20 : -20, Vector3.up);
        player.Initialize();
        paused = false;
    }

    void Update()
    {
        if (player.transform.position.y < -0.5f)
        {
            deathTimer += Time.deltaTime;
            screenFader.SetTrigger("Fade");

            if(deathTimer > 2f)
            {
                if(maxDistance > MenuFunctions.hiScore)
                {
                    MenuFunctions.hiScore = Mathf.RoundToInt(maxDistance);
                }
                SceneManager.LoadScene("menu");
            }
        }
    }

    public void UpdateScore(float distance)
    {
        if(distance > maxDistance)
        {
            maxDistance = distance;
            distanceText.text = "DISTANCE : " + Mathf.RoundToInt(maxDistance);
        }
    }
}
