using UnityEngine;
using System.Collections;

public class GameStartTrigger : MonoBehaviour {

	void OnTriggerExit(Collider other){
        if(other.gameObject.layer == 11){
            collider.isTrigger = false;
            Player.instance.generator.StartSpawning();
            
        }
    }
}
