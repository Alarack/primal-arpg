using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;


public class FloatingText : MonoBehaviour
{

    public float maxYForce;
    public float minYForce;
    public float maxXForce;
    public float minXForce;


    [Header("Required Setup Fields")]
    public TextMeshProUGUI valueText;
    public Rigidbody2D myBody;


    public ParticleSystem particles;
    public TextMeshPro textMesh;


    private Vector3 initialScale;
    private Vector3 overloadScale;


    private void Awake() {
     
    }

    public void Setup(string displayText, float lifetime = 2f, bool overload = false) {
        valueText.text = displayText;
        textMesh.text = displayText;


        initialScale = transform.localScale;
        overloadScale = initialScale * 2f;

        SetParticleMesh();

        PopOff();

        if(overload == true) {
            transform.DOScale(overloadScale, 0.3f)
                .SetEase(Ease.InBounce)
                .OnComplete(() => {
                    transform.DOScale(initialScale, 0.2f)
                    .SetEase(Ease.OutBounce);
                });

        }


        Destroy(gameObject, lifetime);
    }

    public void SetParticleMesh() {
        ParticleSystemRenderer particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        particleSystemRenderer.mesh = textMesh.mesh;
        textMesh.transform.localPosition = new Vector3(0f, 0f, -100f);

        //SetColor(Color.blue, Color.green);

    }

    public void SetColor(Color color1, Color color2) {
        //valueText.faceColor = color;

        Gradient grad = new Gradient();

        GradientColorKey gradKey1 = new GradientColorKey(color1, 0f);
        GradientColorKey gradKey2 = new GradientColorKey(color2, 1f);

        GradientAlphaKey alpha1 = new GradientAlphaKey(1.0f, 0f);
        GradientAlphaKey alpha2 = new GradientAlphaKey(0f, 1f);

        grad.SetKeys(new GradientColorKey[] { gradKey1, gradKey2 }, new GradientAlphaKey[] {alpha1, alpha2 });

        var colorGrad = particles.colorOverLifetime;

        colorGrad.color = grad;
        
    }

    public void SetColor(Gradient gradient) {
        var colorGrad = particles.colorOverLifetime;

        colorGrad.color = gradient;
    }

    private void PopOff() {
        float yForce = Random.Range(minYForce, maxYForce);
        float xForce = Random.Range(minXForce, maxXForce);

        Vector2 motion = new Vector2(xForce, yForce);


        myBody.AddForce(motion, ForceMode2D.Impulse);

    }

}
