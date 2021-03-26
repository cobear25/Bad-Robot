using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class BotHeadTorso : MonoBehaviour
{
    Rigidbody2D rb;
    public float maxAngularVelocity;
    public float maxVelocity;
    public GameController gameController;
    public float thrusterPower;
    public float torque;
    public float fuel = 0;
    public ParticleSystem thrusterParticles;
    public Gradient purpleGradient;
    public AudioSource audio;
    public AudioSource thrusterAudio;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine("CheckTorque");
        thrusterParticles.Stop();
        if (gameController.hasJam) {
            var colorModule = thrusterParticles.colorOverLifetime;
            colorModule.color = new ParticleSystem.MinMaxGradient(purpleGradient);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.canMove == false) { return; }
        var h = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown(KeyCode.Z)) {
            thrusterParticles.Play();
            if (PlayerPrefs.GetInt("muted", 0) == 0)
            {
                thrusterAudio.Play();
            }
        }

        if (Input.GetKeyUp(KeyCode.Z)) {
            thrusterParticles.Stop();
            thrusterAudio.Stop();
        }

        if (Input.GetKey(KeyCode.Z)) {
            if (fuel > 0) {
                rb.AddRelativeForce(new Vector2(0, thrusterPower * Time.deltaTime));
                fuel -= Time.deltaTime;
                // if (fuel <= 0) {
                //     Invoke("ResetFuel", 2f);
                // }
                if (thrusterParticles.isStopped) {
                    thrusterParticles.Play();
                    if (PlayerPrefs.GetInt("muted", 0) == 0)
                    {
                        thrusterAudio.Play();
                    }
                }
            } else {
                thrusterParticles.Stop();
                thrusterAudio.Stop();
            }
            rb.angularVelocity = 0;
            if (h > 0)
            {
                // transform.Rotate(new Vector3(0, 0, -1), Space.Self);
                rb.MoveRotation(rb.rotation - 1);
            }
            else if (h < 0)
            {
                // transform.Rotate(new Vector3(0, 0, 1), Space.Self);
                rb.MoveRotation(rb.rotation + 1);
            }
            else
            {
            }
            // rb.AddTorque(-torqueControl * aerialTorque * Time.deltaTime);
        }
        else
        {
            if (fuel < gameController.maxFuel) {
                fuel += Time.deltaTime;
            }
            if (Mathf.Abs(rb.angularVelocity) < maxAngularVelocity)
            {
                rb.AddTorque(-h * torque * Time.deltaTime);
            }
            if (Mathf.Abs(rb.velocity.x) < maxVelocity)
            {
                rb.AddForce(new Vector2(h * 200 * Time.deltaTime, 0));
            }
        }
        gameController.UpdateThrust(fuel);
    }

    void ResetFuel() {
        fuel = 0.1f;
    }

    IEnumerator CheckTorque() {
        while (true) {
            yield return new WaitForSeconds(0.1f);
            var torque = System.Math.Round(rb.angularVelocity / 10, 2);
            gameController.UpdateTorque(-(float)torque);
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Jam") {
            Destroy(col.gameObject);
            gameController.PickedUpJam();
            var colorModule = thrusterParticles.colorOverLifetime;
            colorModule.color = new ParticleSystem.MinMaxGradient(purpleGradient);
        }

        if (col.tag == "Arm") {
            Destroy(col.gameObject);
            gameController.PickedUpArmFromTorso();
        }

        if (col.tag == "checkpoint") {
            var renderer = col.GetComponent<SpriteShapeRenderer>();
            if (renderer.color != Color.green) {
                renderer.color = Color.green;
                gameController.checkpoint = col.transform;
                gameController.PlayCheckpointSound();
            }
        }

        if (col.tag == "end" && gameController.canMove) {
            gameController.EndReached();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            transform.rotation = Quaternion.identity;
        }
    }

    bool stuckOnce = false;
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
            transform.rotation = Quaternion.identity;
        }
    }
}
