using UnityEngine;
using System.Collections;

public class DeadSkeleton : MonoBehaviour {
    public GameObject[] parts;


    void Start(){
        foreach(GameObject go in parts){
            go.rigidbody.isKinematic = false;
            go.rigidbody.AddForce(Random.onUnitSphere * 15.0f,ForceMode.Impulse);
        }
    }
}
