using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public static Player instance;

	public Camera mainCamera;
    public Transform mainCameraAnchor;

	public float mouseSensitivity;
	public float movementSpeed;
	
	public float headBobCycle;
    public float headBobAmplitude;

    public Transform weaponAnchor;
    public Animation weaponAnimation;

    public GameObject shoeProjectilePrefab;
    public Transform shoeTransform;
    public float shoeFlightTime; //czy czas moze?
    public float shoeMinRange;
    public float shoeMaxRange;

    public Transform compassTransform;

    public float meleeRange;

    public GUIText displayedText;

    public Generator generator;

    private int currentHitPoints;

    private float bobValue;

    private float elapsedTimeSinceMovmentStart;
	private Rigidbody playerRigidbody;
    
    private float inputHorizontalAxisRaw;
	private float inputVerticalAxisRaw;
	private float currentFrameSpeed;
	private float lastFrameSpeed;


	private Vector3 velocityChange;
	private Vector3 targetVelocity;

	private Transform lookTransform;

	private float inputMouseXAxis;
	private float inputMouseYAxis;
	private Vector3 mouseLookDelta;
    private Vector3 desiredWeaponAnchorRotation;

    public bool lockInput;


    private void Awake(){
		instance = this;
        playerRigidbody = rigidbody;
		lookTransform = mainCamera.transform;
        lockInput = true;
    }

    public void StartMoving(){
        lockInput = false;
        rigidbody.isKinematic = false;
        Screen.lockCursor = true;
        Screen.showCursor = false;
        UserInterface.SetHealth(currentHitPoints);
    }

    private void Start(){
        //UserInterface.SetText("Press W, A, S, D to move\nuse your mouse to look around.");
        StartCoroutine(CheckCurrentTerrain());
        currentHitPoints = 3;
       
    }

    private void PrepareAnimationLayers(){
        weaponAnimation["Shoe@Hit"].layer = 0;
        weaponAnimation["Shoe@Throw"].layer = 0;
        weaponAnimation["Shoe@ThrowIdle"].layer = 0;
        weaponAnimation["Shoe@Walk"].layer = 1;
        weaponAnimation["Shoe@Idle"].layer = 1;
    }

    private void ToggleMouseLook(){
        Screen.lockCursor = !Screen.lockCursor;
        Screen.showCursor = !Screen.showCursor;
    }

    private void Update(){
        MouseLook();
        
        if(lockInput){
            return;
        }
        
        
        
		CalculateMovement();
		HeadBob();
        WeaponSway();

        if(Input.GetKeyDown(KeyCode.Tab)){
            ToggleMouseLook();
        }

        if(Input.GetMouseButtonDown(0)){
            if(!IsAttacking()){
                StartCoroutine(AttackWithShoeCoroutine());
            }    
        }

        if(Input.GetMouseButtonDown(1)){
            if (!IsAttacking()) {
                StartCoroutine(ThrowShoeCoroutine());
            }
        }

        
	}

    private IEnumerator ThrowShoeCoroutine(){
        weaponAnimation.Play("Shoe@Throw");
        yield return new WaitForSeconds(weaponAnimation["Shoe@Throw"].length);
        ThrowShoe();
        Utils.ShakeTransform(mainCamera.transform, new Vector3(0.1f, 0.0f, 0.0f), 0.2f, true, 0.0f);
        weaponAnimation.Play("Shoe@ThrowIdle");
    }

    public void CatchShoe(){
        StartCoroutine(CatchShoeCoroutine());
    }

    private IEnumerator CatchShoeCoroutine(){
        weaponAnimation.Play("Shoe@Catch");
        Utils.ShakeTransform(mainCamera.transform, new Vector3(0.1f, 0.0f, 0.0f), 0.1f, true, 0.0f);
        Jukebox.PlaySound2d("shoe_catch",0.0f);
        yield return null;
        shoeTransform.renderer.enabled = true;
    }

    private IEnumerator AttackWithShoeCoroutine(){
        yield return null;
        weaponAnimation.Play("Shoe@Hit");
        Jukebox.PlayAttackSound(0.25f);
        StartCoroutine(DealDamage(0.3f));
        Utils.ShakeTransform(mainCamera.transform, new Vector3(0.1f, 0.0f, 0.0f), 0.2f, true, 0.4f);
    }

	private void FixedUpdate(){
		if(lockInput)
            return;
        calculateSlope();
        ApplyMovement();
    }

    private void WeaponSway(){
        float verticalSwayRatio = Mathf.Clamp(Input.GetAxis("Mouse Y")/3.33f,-1.0f,1.0f);
        float horizontalSwayRatio = Mathf.Clamp(Input.GetAxis("Mouse X") / 6.33f, -1.0f, 1.0f);
       
        desiredWeaponAnchorRotation.x = verticalSwayRatio * 10.0f;
        desiredWeaponAnchorRotation.y = horizontalSwayRatio * 10.0f;
        desiredWeaponAnchorRotation.z  = 0.0f;
       
        weaponAnchor.localRotation = Quaternion.Lerp(weaponAnchor.localRotation,Quaternion.Euler(desiredWeaponAnchorRotation),Time.deltaTime * 5.0f);
    }
    
    private bool IsAttacking(){
        return weaponAnimation.IsPlaying("Shoe@Hit") || weaponAnimation.IsPlaying("Shoe@Throw") || weaponAnimation.IsPlaying("Shoe@Catch") || weaponAnimation.IsPlaying("Shoe@ThrowIdle");
    }

    private void HeadBob(){
        if(SpeedRatio() > 0.1f){
            if(elapsedTimeSinceMovmentStart > headBobCycle){
                elapsedTimeSinceMovmentStart = 0.0f;
                Jukebox.PlayFootstepSound();
            }
            else{
                elapsedTimeSinceMovmentStart += Time.deltaTime;
            }
            if(!IsAttacking()){
                weaponAnimation.CrossFade("Shoe@Walk");
            }
        }
        else{
            if(elapsedTimeSinceMovmentStart > 0.0f){
                elapsedTimeSinceMovmentStart -= Time.deltaTime;
            }
            else{
                elapsedTimeSinceMovmentStart = 0.0f;
            }
            if (!IsAttacking()) {
                weaponAnimation.CrossFade("Shoe@Idle");
            }
        }

        float arg = elapsedTimeSinceMovmentStart / headBobCycle * Mathf.PI;
        bobValue = Mathf.Sin(arg) * headBobAmplitude;
        mainCameraAnchor.localPosition = new Vector3(0.0f, bobValue + 1.0f, 0.0f);
    }


	private void MouseLook(){
		inputMouseXAxis = Input.GetAxisRaw("Mouse X");
		inputMouseYAxis = Input.GetAxisRaw("Mouse Y");
		mouseLookDelta.y = inputMouseXAxis * mouseSensitivity * 1.0f / 60.0f;
        mouseLookDelta.x = -inputMouseYAxis * mouseSensitivity * 1.0f / 60.0f;
		mainCameraAnchor.localEulerAngles += mouseLookDelta;
	}

	private void CalculateMovement(){
		inputHorizontalAxisRaw = Input.GetAxisRaw("Horizontal");
		inputVerticalAxisRaw = Input.GetAxisRaw("Vertical");
		targetVelocity = (inputHorizontalAxisRaw * lookTransform.right) + (inputVerticalAxisRaw * lookTransform.forward);
		targetVelocity.y = 0.0f;
		targetVelocity = targetVelocity.normalized * movementSpeed;
	}

	private void ApplyMovement(){
		velocityChange = targetVelocity - playerRigidbody.velocity;
		velocityChange.y = 0.0f;
		playerRigidbody.AddForce(velocityChange,ForceMode.VelocityChange);
    }

    private void calculateSlope(){
        RaycastHit hitInfo;
        if(Physics.Linecast(transform.position,transform.position - Vector3.up,out hitInfo)){
            float anglesInRadians = Mathf.Acos(Mathf.Clamp(hitInfo.normal.y, -1f, 1f));
        }
    }
    
    private float SpeedRatio(){
        Vector3 tmp = playerRigidbody.velocity;
        tmp.y = 0.0f;
        return Mathf.Clamp01(tmp.sqrMagnitude/(movementSpeed*movementSpeed));
    }

    private void ThrowShoe(){
        RaycastHit hitInfo;
        Ray targetRay = new Ray(mainCamera.transform.position,mainCamera.transform.forward);
        Vector3 targetPosition = Vector3.zero;
        int layerMask = LayerMask.GetMask("Player");
        layerMask = ~layerMask;

        if(Physics.Raycast(targetRay,out hitInfo,shoeMaxRange,layerMask)){
            
            print(hitInfo.collider.name);
            if(hitInfo.distance < shoeMinRange){
                targetPosition = mainCameraAnchor.position + mainCameraAnchor.forward * shoeMinRange;
            }
            else{
               targetPosition = hitInfo.point;  
            }
        }
        else{
            targetPosition = mainCameraAnchor.position + mainCameraAnchor.forward * shoeMaxRange;
        }


        GameObject go = GameObject.Instantiate(shoeProjectilePrefab, shoeTransform.position, shoeTransform.rotation) as GameObject;
        ShoeProjectile sp = go.GetComponent<ShoeProjectile>();
        sp.Init(this);
        sp.Fire(shoeTransform, targetPosition, shoeFlightTime);
    }
    
    private void PlayerStartedMoving(){
		//print("Start movement");
	}

	private void PlayerStoppedMoving(){
		//print("StopMovement");
	}

    private void ShowDebugData(string propertyName, object propertyValue) {
        GUILayout.Label(propertyName + " : " + propertyValue);
    }

    private IEnumerator CheckCurrentTerrain(){
        RaycastHit hitInfo;
        while(true){

            if(Physics.Linecast(transform.position,transform.position - Vector3.up * 2.0f, out hitInfo, 1<<9)){
                GameObject go = hitInfo.collider.gameObject;
                generator.ChangeCurrentTerrain(go);
            }
            

            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator DealDamage(float delay){
        yield return new WaitForSeconds(delay);
        RaycastHit hitInfo;

        if(Physics.Linecast(mainCamera.transform.position,mainCamera.transform.position +  mainCamera.transform.forward * meleeRange, out hitInfo,1<<13)){
            hitInfo.collider.gameObject.SendMessage("TakeDamage",2,SendMessageOptions.DontRequireReceiver);
            print("lolszto "+hitInfo.collider.name);
        }
    }

    private void TakeDamage(int dmg){
        Utils.ShakeTransform(mainCamera.transform, new Vector3(0.0f, 0.3f, 0.0f), 0.2f, true, 0.0f);
        Jukebox.PlaySound2d("player_pain",0.0f);
        currentHitPoints--;
        UserInterface.SetHealth(currentHitPoints);
        if(currentHitPoints == 2){
            StartCoroutine(LifeRegen());
        }
        else if(currentHitPoints <= 0){
            StopAllCoroutines();
            Die();
        }
        
       
        
    }

    private IEnumerator LifeRegen(){
        while(currentHitPoints < 3){
            
            yield return new WaitForSeconds(5.0f);
            currentHitPoints ++;
            UserInterface.SetHealth(currentHitPoints);
        }
    }

    private void Die(){
        lockInput = true;
        rigidbody.isKinematic = true;
        collider.enabled = false;
        animation.Play("Player@Die");
        Splash.instance.GameOver();
        
        //UserInterface.SetText("Press any key...");
        StartCoroutine(WaitForReset());
    }

    private IEnumerator WaitForReset(){
        yield return new WaitForSeconds(1.0f);
        while(true){
           if(Input.anyKeyDown){
               if(Input.GetKeyDown(KeyCode.Escape)){
                    Application.Quit();
               }
               else{
                   Utils.ReloadGame();
               }
               
           }
            yield return null;
        }
    }

    public void WinSequence(){
        weaponAnimation.renderer.enabled = false;
        lockInput = true;
        rigidbody.isKinematic = true;
        StartCoroutine(WinSequenceCoroutine());
    }

    private IEnumerator WinSequenceCoroutine(){
        yield return new WaitForSeconds(2.0f);
        UserInterface.SetText2("You are free");
        Jukebox.PlaySound2d("game_over",0.0f);
        Vector3 curPos = transform.position;
        Vector3 desiredPos = transform.position + Vector3.up * 50.0f;
        float time = 5.0f;
        float curTime = 0.0f;

        while(curTime <= time){
            transform.position = Vector3.Lerp(curPos,desiredPos,curTime/time);
            curTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Splash.instance.GameOver();
        StartCoroutine(WaitForReset());
    }
    /*
    private void UpodateCompass(){
        if(GameOverSwitch.instance){
            Vector3 v1 = GameOverSwitch.instance.transform.position;
            Vector3 v2 = transform.position;
           
            float eulerZ = Mathf.Atan2(v1.z - v2.z, v1.x - v2.x) * Mathf.Rad2Deg;
            float desiredZ = mainCameraAnchor.transform.localEulerAngles.y - eulerZ;

           
            
            Vector3 euler = compassTransform.localEulerAngles;
            euler.z = desiredZ;
            compassTransform.localEulerAngles = euler;
        }
    }
    */
}
