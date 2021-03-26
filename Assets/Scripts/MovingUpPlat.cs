using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingUpPlat : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + (10 * Time.deltaTime));
        if (transform.position.y > 15) {
            transform.position = new Vector2(transform.position.x, -15);
        }
    }
}

