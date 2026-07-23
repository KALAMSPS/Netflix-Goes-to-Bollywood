using UnityEngine;
using System.Collections;
//using LuBoarding_FirstWeek;

public class PlayerTriggerActivity : MonoBehaviour
{
    public GameObject activityObject;
    public GameObject activityObjectOff;
    public GameObject activityObjectHR;
    public GameObject activityObjectOffHR;
    public GameObject sractivityObjectOff;
    public GameObject srCanavas;
    public GameObject srCanavasSecond;
    public GameObject srHightSecond;
    public GameObject CeoRoomGate;
    public GameObject HRRoomGate;
    public GameObject infomationHeading;
    public GameObject HRGameObject;
    public GameObject ColleagueHighlighterOff;
    public GameObject ColleagueObject;
    public GameObject WorkerHighlighterOff;
    public GameObject WorkerObject;


    public Animator HRAnimator;
    public Animator SrAnimator;
    public Animator FactoryAnimator;
    public Animator FactoryWorkerAnimator;
    public Animator ceoAnimator;
    public Animator animator;
    public Animator animatorColleague;


    private bool isCeoGateOpen = false;
    private bool isHRGateOpen = false;
    private Coroutine ceoGateCoroutine;
    private Coroutine hrGateCoroutine;

    public static PlayerTriggerActivity instance;

    private void Start()
    {
        instance = this;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            Debug.Log("Opening CEO Gate...");
            if (!isCeoGateOpen)
            {
                if (ceoGateCoroutine != null) StopCoroutine(ceoGateCoroutine);
                ceoGateCoroutine = StartCoroutine(RotateGate(CeoRoomGate, 90));
                isCeoGateOpen = true;
            }
        }

        if (other.CompareTag("HRGate"))
        {

            Debug.Log("Opening HR Gate...");
            if (!isHRGateOpen)
            {
                if (hrGateCoroutine != null) StopCoroutine(hrGateCoroutine);
                hrGateCoroutine = StartCoroutine(RotateGate(HRRoomGate, 90));
                isHRGateOpen = true;
            }
        }

        if (other.CompareTag("SRGate"))
        {

            animator.Play("HrDoor");
        }

        if (other.CompareTag("CEOSpeech"))
        {
            infomationHeading.SetActive(false);
            Debug.Log("Player entered CEO Speech trigger!");
            ceoAnimator.Play("Sit To Stand");
            activityObjectOff.SetActive(false);
            StartCoroutine(DelayAnimation(ceoAnimator, "Talking", activityObject));
        }
        if (other.CompareTag("HRSpeech"))
        {
            HRGameObject.transform.localRotation = Quaternion.Euler(0, 30, 0);
            infomationHeading.SetActive(false);
            Debug.Log("Player entered HR Speech trigger!");
            HRAnimator.Play("Sit To Stand");
            activityObjectOffHR.SetActive(false);
            StartCoroutine(DelayAnimation(HRAnimator, "Talking", activityObjectHR));
        }
        if (other.CompareTag("SRSpeech"))
        {
            infomationHeading.SetActive(false);

            SrAnimator.Play("Sit To Stand");
            sractivityObjectOff.SetActive(false);
            StartCoroutine(DelayAnimation(SrAnimator, "Talking", srCanavas));
        }
        if (other.CompareTag("SRSpeechTask"))
        {
            infomationHeading.SetActive(false);

            SrAnimator.Play("Sit To Stand");
            srHightSecond.SetActive(false);
            StartCoroutine(DelayAnimation(SrAnimator, "Talking", srCanavasSecond));
        }
        if (other.CompareTag("FactoryManager"))
        {
            infomationHeading.SetActive(false);

            FactoryAnimator.Play("Talking");
            activityObjectOff.SetActive(false);
            StartCoroutine(DelayAnimation(FactoryAnimator, "Talking", activityObject));
        }
        if (other.CompareTag("FactoryWorker"))
        {
            infomationHeading.SetActive(false);

            FactoryWorkerAnimator.Play("Talking");
            WorkerHighlighterOff.SetActive(false);
            StartCoroutine(DelayAnimation(FactoryWorkerAnimator, "Talking", WorkerObject));
        }
        if (other.CompareTag("Colleague"))
        {
            infomationHeading.SetActive(false);
            Debug.Log("Player entered CEO Speech trigger!");
           // ceoAnimator.Play("Sit To Stand");
            ColleagueHighlighterOff.SetActive(false);
            StartCoroutine(DelayAnimation(animatorColleague, "Talking", ColleagueObject));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            Debug.Log("Closing CEO Gate...");
            if (isCeoGateOpen)
            {
                if (ceoGateCoroutine != null) StopCoroutine(ceoGateCoroutine);
                ceoGateCoroutine = StartCoroutine(RotateGate(CeoRoomGate, 0));
                isCeoGateOpen = false;
            }
        }

        if (other.CompareTag("HRGate"))
        {
            Debug.Log("Closing HR Gate...");
            if (isHRGateOpen)
            {
                if (hrGateCoroutine != null) StopCoroutine(hrGateCoroutine);
                hrGateCoroutine = StartCoroutine(RotateGate(HRRoomGate, 0));
                isHRGateOpen = false;
            }
        }
    }

    IEnumerator RotateGate(GameObject gate, float targetYRotation)
    {
        float duration = 1.0f;
        float timeElapsed = 0f;
        Quaternion startRotation = gate.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, targetYRotation, 0);

        while (timeElapsed < duration)
        {
            gate.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        gate.transform.rotation = targetRotation;
    }

    IEnumerator DelayAnimation(Animator animator, string animationName, GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);
        animator.Play(animationName);
        if (obj != null) obj.SetActive(true);
    }
}
