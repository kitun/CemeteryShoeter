using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour {
    
    public static Utils instance;

    private void Awake(){
        instance = this;
    }

	public static void ShakeTransform(Transform transform, Vector3 axesIntensity, float time, bool localPosition, float delay){
        instance.StartCoroutine(instance.ShakeTransformCoroutine(transform, axesIntensity, time, localPosition,delay));
            
    }

    private IEnumerator ShakeTransformCoroutine(Transform transform, Vector3 axesIntensity, float time, bool localPosition, float delay){
        yield return new WaitForSeconds(delay);
        Vector3 startPosition = transform.localPosition;
        float timeElapsed = 0.0f;

        while(timeElapsed < time){
            Vector3 tmp = Random.onUnitSphere;
            tmp.x *= axesIntensity.x;
            tmp.y *= axesIntensity.y;
            tmp.z *= axesIntensity.z;

            tmp *= (1.0f - timeElapsed/time);
            
            transform.localPosition = startPosition + tmp;
            
                
            
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = startPosition;
    }

    public static void ReloadGame(){
        Application.LoadLevel(Application.loadedLevel);
    }

    
}
