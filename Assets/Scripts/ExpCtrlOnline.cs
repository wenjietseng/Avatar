using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Klak.Motion;
using UnityEngine.UIElements;
using RootMotion.Demos;

public class ExpCtrlOnline : MonoBehaviour
{
    [Header("Participant Info")]
    public int participantID = 0;
    public enum GenderMatchedAvatar { woman = 0, man = 1 };
    public GenderMatchedAvatar genderMatchedAvatar;

    [Header("Conditions")]
    public float currentTime;
    /// <summary> a Latin square for 4 conditions
    /// 4 layouts of physical targets
    /// A C B D | P0, P4, P8 
    /// B A D C | P1, P5, P9
    /// C D A B | P2, P6, P10
    /// D B C A | P3, P7, P11
    ///int[,] latinSquare4x4 = new int[4, 4] { {1, 3, 2, 4},
    ///                                        {2, 1, 4, 3},
    ///                                        {3, 4, 1, 2},
    ///                                        {4, 2, 3, 1} };
    /// </summary>
    public enum VisuomotorType { D1M = 1, D1F = 2, D2M = 3, D2F = 4, D3M = 5, D3F = 6 };
    public VisuomotorType vmType;
    private bool isAvatarRunning;
    private bool isQuestionnaireRunning;
    public float exposureDuration = 180f;
    private int durationFactor;
    private float asyncStartTime;
    private float asyncEndTime;

    [Header("Avatars")]
    public GameObject femaleAvatar;
    public GameObject maleAvatar;
    public GameObject syncAvatar;
    private SkinnedMeshRenderer rocketboxSMR;
    // for async avatar
    public GameObject leftArmBM;
    public GameObject rightArmBM;
    [Header("Hand-Tracking Quality")]
    public OVRHand leftHand;
    public OVRHand rightHand;
    private List<int> leftHandTrackingQuality;
    private List<int> rightHandTrackingQuality;
    public float leftOut;
    public float rightOut;
    private bool isCalculatedQuality;

    [Header("Procedure")]
    public GameObject pointer;
    public GameObject startBox;
    public TMP_Text mainInstructions;
    public bool isStartFlagOn;
    private bool isCountDown;
    public VRIKCalibrationBasic ikCalibration;
    private List<string> codeWordList = new List<string> {"code1", "code2"};
    public string codeWord;

    void Start()
    {
        startBox.SetActive(false);
        pointer.GetComponent<Renderer>().enabled = false;

        syncAvatar = (genderMatchedAvatar == GenderMatchedAvatar.woman) ? femaleAvatar : maleAvatar;
        ikCalibration = syncAvatar.GetComponent<VRIKCalibrationBasic>();
        if (genderMatchedAvatar == GenderMatchedAvatar.woman) maleAvatar.SetActive(false);
        else femaleAvatar.SetActive(false);

        rocketboxSMR = syncAvatar.GetComponentInChildren<SkinnedMeshRenderer>();

        leftArmBM.transform.localPosition = Vector3.zero;
        leftArmBM.GetComponent<BrownianMotion>().enabled = false;
        rightArmBM.transform.localPosition = Vector3.zero;
        rightArmBM.GetComponent<BrownianMotion>().enabled = false;

        if (vmType == VisuomotorType.D1M || vmType == VisuomotorType.D1F)
        {
            durationFactor = 1;
        }
        else if (vmType == VisuomotorType.D2M || vmType == VisuomotorType.D2F)
        {
            durationFactor = 2;
        }
        else if (vmType == VisuomotorType.D3M || vmType == VisuomotorType.D3F)
        {
            durationFactor = 3;
        }
        asyncStartTime = (exposureDuration/2) - (durationFactor*(exposureDuration/10));
        asyncEndTime = exposureDuration/2;
        Debug.LogWarning("Condition Info: " + vmType.ToString() + "\nasync starts at: " + asyncStartTime + ", ends at: " + asyncEndTime);
        Helpers.Shuffle(codeWordList);
        codeWord = codeWordList[0];

        leftHandTrackingQuality = new List<int> {};
        rightHandTrackingQuality = new List<int> {};

        rocketboxSMR.enabled = false;
        mainInstructions.text = mainInstructions.text + "\n\n" + vmType.ToString();
    }

    void Update()
    {
        if (isStartFlagOn)
        {
            isStartFlagOn = false;
            startBox.SetActive(false);
            pointer.GetComponent<Renderer>().enabled = false;
            StartCoroutine(StartCondition());
        }
        else
        {
            if (OVRPlugin.GetHandTrackingEnabled() && !isCountDown && !isAvatarRunning && !isQuestionnaireRunning)
            {
                // switch between controller and hand tracking.
                startBox.SetActive(true);
                pointer.GetComponent<Renderer>().enabled = true;
            }
        }

        if (isCountDown)
        {
            currentTime += Time.deltaTime;
            mainInstructions.text = "Stretch out your arms in front of your body. The study will begin in " + (5f - currentTime).ToString("F0") + ".";
        }

        if (isAvatarRunning)
        { 

            // Debug.LogWarning(OVRPlugin.GetHandTrackingEnabled() + "hand");
            //// holding controllers --> false
            //// untracked like occlusion --> false
            //// hands in front of the HMD --> true
            // Debug.Log("left: " + leftHand.IsTracked);
            // Debug.Log("left confidence: " + leftHand.HandConfidence);

            LeftHandQualityRecorder();
            RightHandQualityRecorder();

            currentTime += Time.deltaTime;

            if (currentTime < exposureDuration)
            {
                if (asyncStartTime < currentTime && currentTime < asyncEndTime)
                {
                    if (!leftArmBM.GetComponent<BrownianMotion>().enabled)
                        leftArmBM.GetComponent<BrownianMotion>().enabled = true;

                    if (!rightArmBM.GetComponent<BrownianMotion>().enabled)
                        rightArmBM.GetComponent<BrownianMotion>().enabled = true;
                }
                else if (asyncEndTime < currentTime)
                {
                    if (leftArmBM.GetComponent<BrownianMotion>().enabled)
                    {
                        leftArmBM.transform.localPosition = Vector3.zero;
                        leftArmBM.GetComponent<BrownianMotion>().enabled = false;
                    }
                    if (rightArmBM.GetComponent<BrownianMotion>().enabled)
                    {
                        rightArmBM.transform.localPosition = Vector3.zero;
                        rightArmBM.GetComponent<BrownianMotion>().enabled = false;
                    }
                }
            }
            else
            {
                // reset random motions
                if (!isCalculatedQuality)
                {
                    float leftSum = 0;
                    foreach (var i in leftHandTrackingQuality) leftSum += i;
                    leftOut =  leftSum / leftHandTrackingQuality.Count;
                    Debug.LogWarning(leftOut);
                
                    float rightSum = 0;
                    foreach (var i in rightHandTrackingQuality) rightSum += i;
                    rightOut = rightSum / rightHandTrackingQuality.Count;
                    isCalculatedQuality = true;
                }

                // stop showing avatar, disable avatars
                rocketboxSMR.enabled = false;
                isAvatarRunning = false;
                isQuestionnaireRunning = true;

                mainInstructions.text = "The VR experience has ended. Please write down the following details for the survey." + 
                    "\nCode Word: " + codeWord +
                    "\nLeft Hand-Tracking Quality: " + leftOut.ToString("F3") +
                    "\nRight Hand-Tracking Quality: " + rightOut.ToString ("F3");
            }
        }
    }


    void LeftHandQualityRecorder()
    {
        if (leftHand.IsTracked) 
        {
            if (leftHand.HandConfidence == OVRHand.TrackingConfidence.High)
            {
                leftHandTrackingQuality.Add(1);
                // mainInstructions.text = "Please tilt your head downwards as if looking down at your body.";
            }
            else
            {
                leftHandTrackingQuality.Add(0);
            }
        }
        else
        {
            leftHandTrackingQuality.Add(0);
        }
    }

    void RightHandQualityRecorder()
    {
        if (rightHand.IsTracked) 
        {
            if (rightHand.HandConfidence == OVRHand.TrackingConfidence.High)
            {
                rightHandTrackingQuality.Add(1);
                // mainInstructions.text += "\nright good."; ////////// to fix
            }
            else
            {
                rightHandTrackingQuality.Add(0);
            }
        }
        else
        {
            rightHandTrackingQuality.Add(0);
        }
    }

    IEnumerator StartCondition()
    {
        isCountDown = true;
        ikCalibration.calibrateAvatar = true;
        yield return new WaitForSeconds(5f);
        ikCalibration.calibrateAvatar = true;
        isCountDown = false;
        currentTime = 0f;
        isAvatarRunning = true;
        mainInstructions.text = "Please tilt your head downwards as if looking down at your body.";
        rocketboxSMR.enabled = true;
        yield return 0;
    }
    
}
