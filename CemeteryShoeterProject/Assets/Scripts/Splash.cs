using UnityEngine;
using System.Collections;

public class Splash : MonoBehaviour {
    public static Splash instance;
    public GUIText prompt;
    public GUITexture bg;
    public GUITexture logo;
    public GUIText info;

    private bool started;
    private bool canStart;

    private void Awake(){
        instance = this;
        started = false;
        canStart = false;
    }

    private IEnumerator Start(){
        yield return StartCoroutine(StartCoroutineC());
        canStart = true;
    }

    private void Update(){
        if(Input.anyKeyDown && !started && canStart){
            StartCoroutine(HideAll());
        }    
    }

    private IEnumerator StartCoroutineC(){
        yield return StartCoroutine(Fade(1.0f,2.0f,logo));
        yield return StartCoroutine(Fade(0.4f,0.5f,prompt));
    }

    private IEnumerator HideAll(){
        started = true;
        
        StartCoroutine(Fade(0.0f, 1.0f, logo));
        StartCoroutine(Fade(0.0f, 0.2f, prompt));
        yield return StartCoroutine(Fade(0.0f, 1.0f, bg));
        Player.instance.StartMoving();
    }

    private IEnumerator Fade(float targetAlpha, float time, GUITexture texture){
        
        float curTime = 0.0f;
        float curAlpha = texture.color.a;
        
        Color tmp = texture.color;

        while(curTime <= time){
            
            tmp.a = Mathf.SmoothStep(curAlpha,targetAlpha,curTime/time);
            texture.color = tmp;
            curTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator Fade(float targetAlpha, float time, GUIText texture) {

        float curTime = 0.0f;
        float curAlpha = texture.color.a;

        Color tmp = texture.color;

        while (curTime <= time) {

            tmp.a = Mathf.SmoothStep(curAlpha, targetAlpha, curTime / time);
            texture.color = tmp;
            curTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void GameOver(){
        StartCoroutine(GameOverCoroutine());
    }

    public IEnumerator GameOverCoroutine(){
        yield return StartCoroutine(Fade(1.0f, 1.0f, bg));
        UserInterface.SetText("Press Esc to quit, or anything else to try again...");
    }


	
}
