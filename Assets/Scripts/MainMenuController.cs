using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public AudioSource audio;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("muted", 0) == 1) {
            audio.Pause();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) {
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
    }
}
