using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour {
    public static List<Enemy> instances = new List<Enemy>();

    public float speed;
    public float attackDistance;
    public Animator animator;

    public Transform targetTransform;
    public Terrain terrain;
    private RaycastHit hitInfo;
	
	private int terrainLayer;
    private int graveLayer;

    private bool allowMovement;

    private Vector3 direction;


    private List<GameObject> obstacles;
    private List<GameObject> neighbours;

    private int currentHitPoints = 2;

    public GameObject deadSkeletonPrefab;

    private void Awake(){
        terrainLayer = LayerMask.NameToLayer("Terrain");
        graveLayer = LayerMask.NameToLayer("Grave");
        allowMovement = true;
        obstacles = new List<GameObject>();
        attackDistance *= transform.localScale.x;
        targetTransform = Player.instance.transform;
        StartCoroutine(CheckDistance());
        terrain = Player.instance.generator.currentTerrain;
        instances.Add(this);
    }
	

	 private void Update () {
        CalculateDirection();
        if(targetTransform){
            MoveTowardsTarget();
        }
        AlignToGround();
        RotateTowardsTarget();
     }


     private void CalculateDirection(){
         direction = targetTransform.position - transform.position;
         
         if(allowMovement){
             for (int i = obstacles.Count - 1; i >= 0; i--) {
                 GameObject obs = obstacles[i];

                 if (obs) {
                     Debug.DrawLine(transform.position + Vector3.up, obs.transform.position + Vector3.up, Color.red);
                     if (obs.layer == 13) {
                         direction += ((transform.position - obs.transform.position) / obstacles.Count) * 5.0f;
                     } else {
                         direction += (transform.position - obs.transform.position) * 5.0f;
                     }
                 } else {
                     obstacles.Remove(obs);
                 }
             }
         }

         

        
         
         direction.y = 0.0f;
         
         direction.Normalize();
     }

     private void MoveTowardsTarget(){
        if(Vector3.Distance(transform.position,targetTransform.position) <= attackDistance){
            animator.SetBool("attack",true);
        }
        else{
            animator.SetBool("attack", false);
            if(allowMovement){
               transform.position = Vector3.Lerp(transform.position,transform.position + direction*speed,Time.deltaTime);
            }
        }
     }

    private void RotateTowardsTarget(){
         Quaternion rot = Quaternion.identity;
         rot = Quaternion.LookRotation(direction);
         transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * 5.0f);
     }

    private void AlignToGround(){
        
        
        Vector3 tmp = transform.position;
        float y = terrain.SampleHeight(tmp);
        tmp.y = y;
        transform.position = tmp;
    }

    public void AttackAnimationStarted(){
        print("attack");
        allowMovement = false;
        Jukebox.PlaySound3d("enemy_attack",0.0f,transform.position);
    }

    public void AttackAnimationHit() {
       // print("jebs");
       RaycastHit hitInfo;
       
       if(Physics.Linecast(transform.position+Vector3.up,transform.position + Vector3.up + transform.forward*attackDistance,out hitInfo ,1<<11)){
            hitInfo.collider.gameObject.SendMessage("TakeDamage",1,SendMessageOptions.DontRequireReceiver);
       }
    }

    public void WalkAnimationStarted(){
      //  print("walk");

        allowMovement = true;
    }

    public void AddObstacle(GameObject obstacle){
        if (!obstacles.Contains(obstacle)) {
            obstacles.Add(obstacle);
        }
    }

    public void RemoveObstacle(GameObject obstacle){
        if (obstacles.Contains(obstacle)) {
            obstacles.Remove(obstacle);
        }
    }
    
    public void TakeDamage(int dmg){
        animator.SetTrigger("damageTaken");
        Jukebox.PlaySound3d("enemy_hit_2d", 0.0f, transform.position);
        allowMovement = false;
        currentHitPoints -= dmg;
        
    }

    public void Die(){
        targetTransform.GetComponent<Player>().generator.EnemyDied();
        instances.Remove(this);
        GameObject.Instantiate(deadSkeletonPrefab,transform.position + transform.forward * 2.5f,transform.rotation);
        Destroy(gameObject);
    }
 
    private IEnumerator CheckDistance(){
        while(true){
            if(Vector3.Distance(transform.position,targetTransform.position) > 75.0f){
                instances.Remove(this);
                Destroy(gameObject);
            }
            yield return new WaitForSeconds(5.0f);
        }
    }  

    public void CheckIfDead(){
        if(currentHitPoints <= 0){
            Die();
        }
    }

    public void Step(){
       //Jukebox.PlaySound3d("enemy_hit",0.0f,transform.position);
    }
}
