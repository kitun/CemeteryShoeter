using UnityEngine;
using System.Collections;

public class AvoidTrigger : MonoBehaviour {
    public Enemy enemy;

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.layer == 14 || other.gameObject.layer == 13){
            enemy.AddObstacle(other.gameObject);
        }
        
    }

    private void OnTriggerExit(Collider other){
       enemy.RemoveObstacle(other.gameObject);
    }
	
}
