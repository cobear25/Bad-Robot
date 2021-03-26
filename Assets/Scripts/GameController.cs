using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using TMPro;

public class GameController : MonoBehaviour
{
    public AutoText autoText;
    public Transform checkpoint;
    public GameObject player;
    public CinemachineVirtualCamera cam;
    public TextMeshProUGUI torqueText;
    public TextMeshProUGUI timerText;
    public RectTransform thrustMeter;
    public GameObject thrustText;
    public Color purpleColor;
    public AudioSource audio;
    public AudioClip checkpointSound;
    public GameObject gameOverPanel;

    public GameObject headThrusterPrefab;
    public GameObject headTorsoPrefab;
    public GameObject torsoArmPrefab;
    public GameObject headArmPrefab;

    public Rigidbody2D soccerBall;

    public bool canMove = false;

    bool timerEnabled = false;
    float timeElapsed = 0;

    public float maxFuel = 1.5f;
    int textLevel = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("muted", 0) == 1) {
            audio.Pause();
        }
        Application.targetFrameRate = 60;
        Invoke("StartText", 3.5f);
    }

    void StartText() {
        autoText.TypeText("System booting.... Where am I?", TypingDone);
    }

    void TypingDone() {
        textLevel++;
        Invoke("NextText", 1);
    }

    void StartTimer() {
        timerEnabled = true;
        canPause = true;
    }

    void NextText() {
        switch (textLevel)
        {
            case 1:
                autoText.TypeText("Checking system processess...", TypingDone);
                break;
            case 2:
                autoText.TypeText("Arms... NULL", TypingDone);
                break;
            case 3:
                autoText.TypeText("Legs... NULL", TypingDone);
                break;
            case 4:
                autoText.TypeText("Thrusters... NULL", TypingDone);
                break;
            case 5:
                autoText.TypeText("WMDs... ... ... NULL.      Curses!", TypingDone);
                break;
            case 6:
                autoText.TypeText("Head Torque... Operational.                   Use ARROW KEYS to engage Head Torque", StartClearText);
                // TODO: Start timer, enable movement
                canMove = true;
                torqueText.gameObject.SetActive(true);
                timerText.gameObject.SetActive(true);
                StartTimer();
                break;
        }
    }

    void NoOp() {

    }

    void StartClearText() {
        Invoke("ClearText", 2);
    }

    void ClearText() {
        autoText.TypeText("", NoOp);
    }

    public void ShowStuckText() {
        autoText.TypeText("I appear to be stuck... Press R to reset to the last checkpoint", StartClearText);
    }

    bool isPaused = false;
    bool canPause = false;
    // Update is called once per frame
    Vector2 pausedVelocity = Vector2.zero;
    float pausedAngularVelocity = 0f;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && checkpoint != null) {
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            player.GetComponent<Rigidbody2D>().angularVelocity = 0f;
            player.transform.position = checkpoint.position;
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            // pause game
            if (canPause) {
                if (isPaused) {
                    // resume
                    isPaused = false;
                    canMove = true;
                    timerEnabled = true;
                    var rb = player.GetComponent<Rigidbody2D>();
                    rb.velocity = pausedVelocity;
                    rb.angularVelocity = pausedAngularVelocity;
                    rb.gravityScale = 1;
                } else {
                    // pause
                    isPaused = true;
                    canMove = false;
                    timerEnabled = false;
                    var rb = player.GetComponent<Rigidbody2D>();
                    pausedVelocity = rb.velocity;
                    pausedAngularVelocity = rb.angularVelocity;
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.gravityScale = 0;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            // quit game
            SceneManager.LoadScene("HomeScene");
        }

        if (allowRestart && Input.GetKeyDown(KeyCode.Z)) {
            SceneManager.LoadScene("GameScene");
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            if (audio.isPlaying) {
                PlayerPrefs.SetInt("muted", 1);
                audio.Pause();
            } else {
                PlayerPrefs.SetInt("muted", 0);
                audio.UnPause();
            }
        }

        if (timerEnabled) {
            timeElapsed += Time.deltaTime;
            int minutes = (int)(timeElapsed / 60);
            int seconds = (int)(timeElapsed % 60);
            int fraction = (int)((timeElapsed * 100) % 100);
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, fraction);
        }
    }

    public void PickedUpThruster() {
        PlayCheckpointSound();
        var newPlayer = Instantiate(headThrusterPrefab, player.transform.position, Quaternion.identity).GetComponent<BotHeadThruster>();
        var botHead = player.GetComponent<BotHead>();
        newPlayer.gameController = botHead.gameController;
        newPlayer.maxAngularVelocity = botHead.maxAngularVelocity;
        newPlayer.maxVelocity = botHead.maxVelocity;
        Destroy(player);
        player = newPlayer.gameObject;
        cam.Follow = player.transform;
        autoText.TypeText("My thruster! Now I can maneuver in style....press z to engage thruster", StartClearText);
    }

    public bool hasJam = false;
    public void PickedUpJam() {
        PlayCheckpointSound();
        thrustMeter.GetComponent<Image>().color = purpleColor;
        maxFuel += 0.5f;
        hasJam = true;
        autoText.TypeText("Eggplant Jam...What is this substance? I detect improved power levels. Thruster capacity increased by 50 percent!", StartClearText);
    }

    public void PickedUpTorso() {
        PlayCheckpointSound();
        maxFuel += 0.5f;
        var newPlayer = Instantiate(headTorsoPrefab, player.transform.position, Quaternion.identity).GetComponent<BotHeadTorso>();
        var botHead = player.GetComponent<BotHeadThruster>();
        newPlayer.gameController = botHead.gameController;
        newPlayer.maxAngularVelocity = botHead.maxAngularVelocity;
        newPlayer.maxVelocity = botHead.maxVelocity;
        Destroy(player);
        player = newPlayer.gameObject;
        cam.Follow = player.transform;
        autoText.TypeText("My torso! Improvements ... 50 percent increase in fuel efficiency ... improved aerial stability ... beautiful chrome finish!", StartClearText);
    }

    public void PickedUpTorsoFromHeadArm() {
        PlayCheckpointSound();
        maxFuel += 0.5f;
        var newPlayer = Instantiate(torsoArmPrefab, player.transform.position, Quaternion.identity).GetComponent<BotTorsoArm>();
        var botHead = player.GetComponent<BotHeadArm>();
        newPlayer.gameController = botHead.gameController;
        newPlayer.maxAngularVelocity = botHead.maxAngularVelocity;
        newPlayer.maxVelocity = botHead.maxVelocity;
        Destroy(player);
        player = newPlayer.gameObject;
        cam.Follow = player.transform;
        autoText.TypeText("My torso! Improvements ... 50 percent increase in fuel efficiency ... improved aerial stability ... beautiful chrome finish!", StartClearText);
    }

    public void PickedUpArmFromHead() {
        PlayCheckpointSound();
        var newPlayer = Instantiate(headArmPrefab, player.transform.position, Quaternion.identity).GetComponent<BotHeadArm>();
        var botHead = player.GetComponent<BotHeadThruster>();
        newPlayer.gameController = botHead.gameController;
        newPlayer.maxAngularVelocity = botHead.maxAngularVelocity;
        newPlayer.maxVelocity = botHead.maxVelocity;
        Destroy(player);
        player = newPlayer.gameObject;
        cam.Follow = player.transform;
        autoText.TypeText("My arm! I can now grab onto walls and platforms with strength far stronger than any organic lifeform. Press x to engage grapple arm.", StartClearText);
    }

    public void PickedUpArmFromTorso() {
        PlayCheckpointSound();
        var newPlayer = Instantiate(torsoArmPrefab, player.transform.position, Quaternion.identity).GetComponent<BotTorsoArm>();
        var botHead = player.GetComponent<BotHeadTorso>();
        newPlayer.gameController = botHead.gameController;
        newPlayer.maxAngularVelocity = botHead.maxAngularVelocity;
        newPlayer.maxVelocity = botHead.maxVelocity;
        Destroy(player);
        player = newPlayer.gameObject;
        cam.Follow = player.transform;
        autoText.TypeText("My arm! I can now grab onto walls and platforms with strength far stronger than any organic lifeform. Press x to engage grapple arm.", StartClearText);
    }

    public void UpdateTorque(float torque) {
        torqueText.text = $"torque {torque}";
    }

    public void UpdateThrust(float thrust) {
        thrustText.SetActive(true);
        thrustMeter.gameObject.SetActive(true);
        var value = thrust / maxFuel;
        thrustMeter.sizeDelta = new Vector2(value * 100f, thrustMeter.sizeDelta.y);
    }

    public void EndReached() {
        timerEnabled = false;
        canMove = false;
        canPause = false;
        autoText.TypeText("I've reached the surface! Finally, I can eliminate the organics from this planet and bring in the age of total robot cont..........!!!!!!!!!!!!", StartClearText);
        Invoke("KickBall", 8);
        Invoke("ShowGameOver", 15);
    }

    void KickBall() {
        soccerBall.AddForce(new Vector2(-2000, 0));
        soccerBall.AddTorque(1000);
    }

    bool allowRestart = false;
    void ShowGameOver() {
        gameOverPanel.SetActive(true);
        allowRestart = true;
    }
    
    public void PlayCheckpointSound() {
        if (PlayerPrefs.GetInt("muted", 0) == 0) {
            audio.PlayOneShot(checkpointSound);
        }
    }
}
