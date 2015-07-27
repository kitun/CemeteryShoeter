using UnityEngine;
using System.Collections;

public class GameOverSwitch : MonoBehaviour {
    public static GameOverSwitch instance;

    public GameObject missingShoe;
    public bool gameOver;

    private void Awake(){
        if(instance){
            Destroy(instance.gameObject);
            instance = this;
        }
        else{
            instance = this;
        }
        gameOver = false;
        Jukebox.PlaySound2d("game_over_ready", 0.0f);
    }
	

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.layer == 11){
            
            for(int i = Enemy.instances.Count-1; i >= 0; i-- ){
                if(Enemy.instances[i]){
                    Enemy.instances[i].Die();
                }
            }

            missingShoe.renderer.enabled = true;
            Player.instance.WinSequence();
            
            gameOver = true;
        }
    }
}
