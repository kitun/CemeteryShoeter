using UnityEngine;
using System.Collections;

public class AudioEffect : MonoBehaviour {
    
    private Jukebox jukebox;
    

    public void Init(string clipName, Vector3 position, Jukebox jukebox, float delay) {
        this.jukebox = jukebox;
        AudioClip clipToPlay = null;
        foreach (AudioInfo ai in jukebox.audioList){
		    if(ai.id.Equals(clipName)){
                clipToPlay = ai.clip;
                break;
            }
	    }
        
        audio.clip = clipToPlay;
        
        StartCoroutine(PlaySound(delay));
    }

    
    private IEnumerator PlaySound(float delay){
        yield return new WaitForSeconds(delay);
        audio.Play();
        while(audio.isPlaying){
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

	
}
