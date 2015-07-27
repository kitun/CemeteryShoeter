using UnityEngine;
using System.Collections;

public class UserInterface : MonoBehaviour {
    public static UserInterface instance;
    
    
    public GUIText guiText;
    public GUIText guiText2;
    public GUIText guiHealth;

    private void Awake(){
        instance = this;
    }

    public static void SetText(string newText){
        instance.guiText.text = newText;
    }

    public static void SetText2(string newText) {
        instance.guiText2.text = newText;
    }

    public static void SetHealth(int hp){
        instance.guiHealth.text = "HEALTH: "+hp+"/3";
    }
	
}
