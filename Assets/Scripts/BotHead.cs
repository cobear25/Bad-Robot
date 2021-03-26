using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class BotHead : MonoBehaviour
{
    Rigidbody2D rb;
    public float maxAngularVelocity;
    public float maxVelocity;
    public GameController gameController;
    bool stuckOnce = false;
    public AudioSource audio;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine("CheckTorque");
        rb.AddTorque(-20);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.canMove == false) { return; }
        var h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(rb.angularVelocity) < maxAngularVelocity) {
            rb.AddTorque(-h * 300f * Time.deltaTime);
        }
        if (Mathf.Abs(rb.velocity.x) < maxVelocity) {
            rb.AddForce(new Vector2(h * 200 * Time.deltaTime, 0));
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Thruster") {
            Destroy(col.gameObject);
            gameController.PickedUpThruster();
        }

        if (col.tag == "checkpoint") {
            var renderer = col.GetComponent<SpriteShapeRenderer>();
            if (renderer.color != Color.green) {
                renderer.color = Color.green;
                gameController.checkpoint = col.transform;
                gameController.PlayCheckpointSound();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        float impulse = 0F;

        foreach (ContactPoint2D point in col.contacts)
        {
            impulse += point.normalImpulse;
        }
        audio.volume = Mathf.Min(impulse / 20f, 1f);
        if (PlayerPrefs.GetInt("muted", 0) == 0) {
            audio.Play();
        }
        if (stuckOnce == false && col.gameObject.tag == "Stuck") {
            stuckOnce = true;
            Invoke("Stuck", 1.5f);
        }
        
        if (col.gameObject.tag == "end" && gameController.canMove) {
            gameController.EndReached();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
    }

    void Stuck() {
        gameController.ShowStuckText();
    }

    IEnumerator CheckTorque() {
        while (true) {
            yield return new WaitForSeconds(0.1f);
            var torque = System.Math.Round(rb.angularVelocity / 10, 2);
            gameController.UpdateTorque(-(float)torque);
        }
    }
}
