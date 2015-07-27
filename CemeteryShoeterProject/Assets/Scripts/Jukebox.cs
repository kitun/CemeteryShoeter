using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AudioInfo {
    public string id;
    public AudioClip clip;
}

public class Jukebox : MonoBehaviour {
    private static Jukebox instance;

   
    
    
    public AudioSource ambientForestSource;
    public AudioSource ambientMoodSource;
    public AudioSource footstepsSource;
    public AudioSource shoeAudioSource;
     

    public AudioClip[] footsteps;
    private List<AudioClip> footstepsRandomList;
    public AudioInfo[] audioList;

    public GameObject audioEffectPrefab;
    


    private void Awake(){
        if(instance == null){
            instance = this;
            footstepsRandomList = new List<AudioClip>();
        }
        else{
            GameObject.Destroy(this);
        }
    }

    public static void PlayFootstepSound(){
        AudioClip clipToPlay = null;
        
        if(instance.footstepsRandomList.Count > 0){
            clipToPlay = instance.footstepsRandomList[Random.Range(0,instance.footstepsRandomList.Count)];
            instance.footstepsRandomList.Remove(clipToPlay);    
        }
        else{
            instance.footstepsRandomList.AddRange(instance.footsteps);
            PlayFootstepSound();
        }
        
        instance.footstepsSource.PlayOneShot(clipToPlay);
    }

   public static void PlayAttackSound(float delay){
        instance.shoeAudioSource.PlayDelayed(delay);        
   }

   public static void PlaySound2d(string clipName, float delay){
        GameObject go = GameObject.Instantiate(instance.audioEffectPrefab) as GameObject;
        AudioEffect ae = go.GetComponent<AudioEffect>();
        ae.Init(clipName,Vector3.zero,instance,delay);
   }

   public static void PlaySound3d(string clipName, float delay, Vector3 pos) {
       GameObject go = GameObject.Instantiate(instance.audioEffectPrefab) as GameObject;
       AudioEffect ae = go.GetComponent<AudioEffect>();
       ae.Init(clipName, pos, instance, delay);
   }
}
