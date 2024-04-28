using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Debris : MonoBehaviour {

    private SpriteRenderer[] renderers;
    public float fadeModifier = 1f;

    public UnityEvent onFadeComplete;

    private Task fadeTask;

    private void Awake() {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Start() {
        Sprite random = GameManager.Instance.tileDatabase.GetRandomDebris();
        SetSprites(random);
    }

    public void SetSprites(Sprite sprite) {
        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].sprite = sprite;
        }
    }

    public void BeginFadout() {
        fadeTask = new Task(FadeOut());
    }


    private IEnumerator FadeOut() {

        foreach (SpriteRenderer renderer in renderers) {


            while (renderer.color.a != 0f) {
                
                float desiredAlpha = Mathf.MoveTowards(renderer.color.a, 0f, Time.deltaTime * fadeModifier);
                Color newColor = new Color(renderer.color.r,renderer.color.g,renderer.color.b,desiredAlpha);
                renderer.color = newColor;
                yield return new WaitForEndOfFrame();
            }

        }

        onFadeComplete?.Invoke();

        Destroy(gameObject);

    }

}
