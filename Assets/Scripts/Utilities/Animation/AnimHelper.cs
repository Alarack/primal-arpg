using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimHelper : MonoBehaviour
{

    public Animator animator;





    public void SetBool(string name, bool value) {
        animator.SetBool(name, value);
    }

    


}
