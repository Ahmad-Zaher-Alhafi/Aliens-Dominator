using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMotionBehavior : MonoBehaviour
{
    public TextMeshProUGUI damadeTakenText;
    [HideInInspector] public bool IsItBusy;
    public float decreaseTextSizeSpeed;
    private Transform playerLookAtPoint;
    private Vector3 initialTextPosition;
    private Animator animator;


    void Awake()
    {
        IsItBusy = false;
        playerLookAtPoint = GameObject.FindGameObjectWithTag(Constants.PlayerLookAtPoint).transform;
        animator = damadeTakenText.GetComponent<Animator>();
        initialTextPosition = damadeTakenText.transform.position;
    }

    private void Update()
    {
        damadeTakenText.transform.LookAt(playerLookAtPoint);
        if (damadeTakenText.transform.localScale.x > 0)
        {
            damadeTakenText.transform.localScale = (damadeTakenText.transform.localScale - (Vector3.one * decreaseTextSizeSpeed * Time.deltaTime));
        }
    }

    public void ShowDamageTakenText(float dmg)
    {
        IsItBusy = true;

        damadeTakenText.enabled = true;
        damadeTakenText.text = (dmg * 10).ToString();
        animator.SetBool("hasToShowText", true);
        damadeTakenText.transform.localScale = Vector3.one * Vector3.Distance(transform.position, playerLookAtPoint.position) / 10;
        if (damadeTakenText.transform.localScale.x < 1)
        {
            damadeTakenText.transform.localScale = Vector3.one * 2;
        }

    }

    public void HideDamageTakenText()
    {

        animator.SetBool("hasToShowText", false);
        damadeTakenText.enabled = false;
        IsItBusy = false;
    }
}
