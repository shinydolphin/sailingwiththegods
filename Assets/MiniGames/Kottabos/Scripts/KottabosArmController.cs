using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KottabosArmController : MonoBehaviour
{
	public Animator animArm;

    // Start is called before the first frame update
    void Start()
    {
		animArm.SetBool("Grab_cup", true);

		animArm.SetFloat("Upper_arm_angle_UP_Down", -1f);
		//0 to 1
		animArm.SetFloat("Upper_arm_angle_forward", 1f);
    }

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyUp(KeyCode.Space)) {
			animArm.SetTrigger("Fling");
		}
	}

	public void ArmReset() 	{
		animArm.SetTrigger("Reset");
	}
}
