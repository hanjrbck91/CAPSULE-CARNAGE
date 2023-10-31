using UnityEngine;

public class Scope : MonoBehaviour
{
    #region ScopeOverlay Field

    [SerializeField] GameObject scopeOverlay;     
    
    #endregion

    public bool isScoped = false;

    [SerializeField] Animator scopeAnimator;

    // Start is called before the first frame update
    void Start()
    {
        // No need to set up input actions in the old input system
    }

    // Update is called once per frame
    void Update()
    {
        // Check for right mouse button click
        if (Input.GetMouseButtonDown(1))
        {
            isScoped = !isScoped;

            scopeAnimator.SetBool("Scope",isScoped);
        }
        if(isScoped)
        {
            scopeOverlay.SetActive(true);
        }
        else
        {
            scopeOverlay.SetActive(false);
        }
    }
}
