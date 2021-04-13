using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm_test : MonoBehaviour
{
	public Transform Upper_arm, Lower_arm;
	public float speed = 1;
	Transform Hand, finger1_1, finger1_2, finger1_3,
					finger2_1, finger2_2, finger2_3,
					finger3_1, finger3_2, finger3_3,
					finger4_1, finger4_2, finger4_3,
					finger5_1, finger5_2, finger5_3;
	Animator aim;
    // Start is called before the first frame update
    void Start()
    {
		aim = gameObject.GetComponent<Animator>();
		Transform[] X = gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform x in X) 
		{
			if (x.name.Equals("upperarm_r")) { Upper_arm = x; }
			if (x.name.Equals("lowerarm_r")) { Lower_arm = x; }
			if (x.name.Equals("hand_r")) { Hand = x; }
			if (x.name.Equals("index_01_r")) { finger1_1 = x; }
			if (x.name.Equals("index_02_r")) { finger1_2 = x; }
			if (x.name.Equals("index_03_r")) { finger1_3 = x; }
			if (x.name.Equals("middle_01_r")) { finger2_1 = x; }
			if (x.name.Equals("middle_02_r")) { finger2_2 = x; }
			if (x.name.Equals("middle_03_r")) { finger2_3 = x; }
			if (x.name.Equals("pinky_01_r")) { finger3_1 = x; }
			if (x.name.Equals("pinky_02_r")) { finger3_2 = x; }
			if (x.name.Equals("pinky_03_r")) { finger3_3 = x; }
			if (x.name.Equals("ring_01_r")) { finger4_1 = x; }
			if (x.name.Equals("ring_02_r")) { finger4_2 = x; }
			if (x.name.Equals("ring_03_r")) { finger4_3 = x; }
			if (x.name.Equals("thumb_01_r")) { finger5_1 = x; }
			if (x.name.Equals("thumb_02_r")) { finger5_2 = x; }
			if (x.name.Equals("thumb_03_r")) { finger5_3 = x; }
		}
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha0)) 
			{
			//Upper_arm.rotation*= Quaternion.Euler(0, 45.0f, 0);
			Holdcup();
		}
    }
	public void Holdcup() 
	{
		//set arm//did not work
		transform.rotation = Quaternion.Lerp(Upper_arm.localRotation, new Quaternion(29.739f, -42.565f, 66.888f,1) , Time.time * speed);
		transform.rotation = Quaternion.Lerp(Lower_arm.localRotation, new Quaternion(6.034f, -8.994f, 121.489f, 1), Time.time * speed);
	} 
}
