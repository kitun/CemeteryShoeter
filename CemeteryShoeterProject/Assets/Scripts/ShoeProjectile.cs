using UnityEngine;
using System.Collections;

public class ShoeProjectile : MonoBehaviour {
    
    public float fullSpinTime;
    private Player player;

    public void Init(Player player){
        this.player = player;
    }

    

   

    private IEnumerator SpinCoroutine(){
        float currentTime = 0.0f;
        Vector3 current = transform.eulerAngles;
        Vector3 target = transform.eulerAngles + Vector3.forward * 90.0f;


        while(currentTime < fullSpinTime){
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(current),Quaternion.Euler(target),currentTime/fullSpinTime);
            currentTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(SpinCoroutine());
    }

    public void Fire(Transform originTransform, Vector3 targetPosition, float speed){
        StartCoroutine(FireCoroutine(originTransform,targetPosition,speed));
        StartCoroutine(SpinCoroutine());    
    }

    private IEnumerator FireCoroutine(Transform originTransform, Vector3 targetPosition, float speed){
        

        float distance = Vector3.Distance(originTransform.position, targetPosition);
        float elapsedTime = 0.0f;
        float timeToReachTarget = speed;
        Vector3 originPosition = originTransform.position;
        float sineValue = 0.0f;
        Vector3 sineVector = Vector3.one;


        //dolatujemy do celu
        while(elapsedTime < timeToReachTarget){
            transform.position = Vector3.Lerp(originPosition,targetPosition,elapsedTime/timeToReachTarget);
            

            sineValue = Mathf.Sin(elapsedTime/timeToReachTarget * Mathf.PI)/2.0f;
            sineVector = (player.mainCameraAnchor.right + player.mainCameraAnchor.up) * sineValue;
            transform.position += sineVector;

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //powrot do gracza
        elapsedTime = 0.0f;
        transform.position = targetPosition;
        distance = Vector3.Distance(originTransform.position, targetPosition);
        timeToReachTarget = speed;
        originPosition = transform.position;
        
        while (elapsedTime < timeToReachTarget) {
            transform.position = Vector3.Lerp(originPosition, originTransform.position, elapsedTime / timeToReachTarget);
            sineValue = Mathf.Sin(elapsedTime / timeToReachTarget * Mathf.PI);
            sineVector = (player.mainCameraAnchor.right + player.mainCameraAnchor.up) * sineValue;
            transform.position -= sineVector; 
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.position = originTransform.position;
        player.CatchShoe();
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other){
        if(other.gameObject.layer == 13){
            other.gameObject.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
            collider.enabled = false;
        }
    }

	
}
