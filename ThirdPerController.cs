
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.ThirdPerson;

public class ThirdPerController : MonoBehaviour
{
	public static bool isRunning = false;


    protected ThirdPersonUserControl Control;
	
	public FixedJoystick LeftJoystick;
	public FixedButton Button;
	public FixedTouchField TouchField;
	
	protected Actions Actions;
	protected PlayerController PlayerController;
	protected Rigidbody Rigidbody;
	
	protected float CameraAngleY;
	protected float CameraAngleSpeed = 0.1f;
	protected float CameraPosY;
	protected float CameraPosSpeed = 0.01f;
	
	protected ParticleSystem ShootParticle;
	protected float CoolDown;

	public Transform barrel;
	public Rigidbody bullet;

	// Start is called before the first frame update

	void Start()
    {
        Actions = GetComponent<Actions>();
		PlayerController = GetComponent<PlayerController>();
		Rigidbody = GetComponent<Rigidbody>();
		 Control = GetComponent<ThirdPersonUserControl>();
		 PlayerController.SetArsenal("Rifle");
		 ShootParticle = GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
		Control.m_Jump = Button.Pressed;
		Control.Hinput = LeftJoystick.inputVector.x;
		Control.Vinput = LeftJoystick.inputVector.y;
		var input = new Vector3(LeftJoystick.inputVector.x, 0, LeftJoystick.inputVector.y);
		var vel = Quaternion.AngleAxis(CameraAngleY + 180,Vector3.up) * input * 5f;
		
		Rigidbody.velocity = new Vector3(vel.x,Rigidbody.velocity.y,vel.z);
		if(Button.Pressed)
		transform.rotation = Quaternion.AngleAxis(CameraAngleY + 180 + Vector3.SignedAngle(Vector3.forward,input.normalized + Vector3.forward * 0.001f,Vector3.up), Vector3.up);
		
		CameraAngleY += TouchField.TouchDist.x * CameraAngleSpeed;
		CameraPosY = Mathf.Clamp(CameraPosY - TouchField.TouchDist.y * CameraPosSpeed, 0, 5f);

		
		Camera.main.transform.position = transform.position + Quaternion.AngleAxis(CameraAngleY,Vector3.up)* new Vector3(0,CameraPosY,2);
       Camera.main.transform.rotation = Quaternion.LookRotation(transform.position + Vector3.up * 1f - Camera.main.transform.position,Vector3.up);
	   
	   var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
	   Debug.DrawRay(ray.origin,ray.direction,Color.red);
	   
	   CoolDown -= Time.deltaTime;
	   if(Button.Pressed)
	   {
		 Actions.Attack();  
		 if(CoolDown <= 0f)
		 {
			 CoolDown = 0.3f;
			 StartCoroutine(Shoot());
			 
			 
			 RaycastHit hitInfo;
			 if(Physics.Raycast(ray, out hitInfo));
			 {
				 var other = hitInfo.collider.GetComponent<Shootable>();
					var firedBullet = Instantiate(bullet, barrel.position, barrel.rotation);
					firedBullet.AddForceAtPosition((hitInfo.point - ShootParticle.transform.position).normalized * 500f, hitInfo.point);
					if (other != null)
				 {
						
						other.GetComponent<Rigidbody>().AddForceAtPosition((hitInfo.point - ShootParticle.transform.position).normalized * 500f,hitInfo.point);
				 }
			 }
		 }
	   }
	   else
	   {

			if (Rigidbody.velocity.magnitude > 3f)
			{
				ThirdPerController.isRunning = true;
				Actions.Run();
			}
			else if (Rigidbody.velocity.magnitude > 0.5f)
			{
				ThirdPerController.isRunning = true;
				Actions.Walk();
			}
			else
			{
				ThirdPerController.isRunning = false;
				Actions.Stay();
			}
	   }
    }
	
	 IEnumerator Shoot()
    {
        yield return new WaitForSeconds(0.5f);
		ShootParticle.Play();
       
    }
	
}
