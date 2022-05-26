using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AutoText : MonoBehaviour
{
    TextMeshProUGUI textMesh;
    private string text = "";
    private TypingDone doneFun;
    public AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void TypeText(string text, TypingDone fun) {
        this.text = text;
        StopCoroutine("StartTyping");
        Type(fun);
    }

    public delegate void TypingDone();

    void Type(TypingDone fun) {
        doneFun = fun;
        textMesh.text = "";
        StartCoroutine("StartTyping");
    }

    IEnumerator StartTyping()
    {
	    foreach (char c in text)
	    {
		    if (PlayerPrefs.GetInt("muted", 0) == 0 && c != ' ') {
			    audio.Play();
		    }
		    textMesh.text += c;
		    yield return new WaitForSeconds (0.07f);
	    }
	    doneFun();
    }
}
