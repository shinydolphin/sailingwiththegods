using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controls the way liquid drips are thrown and animation
public class Throw : MonoBehaviour
{
    private Vector3 pointToTravel;
	private ThrowRadius tr;

    public Animator animate;

    //Controls how far the liquid drips will be thrown
    public float power = 1.0f;

	public bool Launch { get; set; } = false;
	public Rigidbody Rb { get; set; }

	// Start is called before the first frame update
	void Start()
    {
        tr = gameObject.GetComponent<ThrowRadius>();
        Rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        pointToTravel = new Vector3(tr.trajectory.GetPosition(0).x, tr.trajectory.GetPosition(0).y, tr.trajectory.GetPosition(0).z);
        power = Mathf.Clamp(power, 0.5f, 2.0f);
        pointToTravel = (pointToTravel * power);

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Launch = true;
            animate.SetBool("isFlinged", true);

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.CompareTag("TrajectSystem"))
                    transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            power -= 0.1f;
        }else if (Input.GetKeyUp(KeyCode.E))
        {
            power += 0.1f;
        }
    }

    private void FixedUpdate()
    {
        if (Launch)
        {
            Rb.useGravity = true;
            Rb.MovePosition(transform.position + pointToTravel * Time.fixedDeltaTime);
        }
    }
}