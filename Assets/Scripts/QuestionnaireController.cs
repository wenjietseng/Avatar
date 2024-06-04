using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class QuestionnaireController : MonoBehaviour
{
    private ExperimentController experimentController;
    public GameObject questionnaireCanvas;
    public TMP_Text mainText;
    public TMP_Text smallInstruction;
    public TMP_Text largeInstruction;
    public TMP_Text lowAnchorText;
    public TMP_Text highAnchorText;
    public GameObject scale;
    public List<GameObject> scales;
    public List<int> responses;
    private List<QuestionnaireData> items = new List<QuestionnaireData>();
    // SPES: All items were designed to be answered on a 5-point Likert scale ranging from 1 (= I do not agree at all) to 5 (= I fully agree).

    private QuestionnaireData r03;
    private QuestionnaireData r05;
    private QuestionnaireData r10;
    private QuestionnaireData r11;
    private QuestionnaireData r12;
    private QuestionnaireData r13;
    private QuestionnaireData r14;
    private QuestionnaireData goodness;

    private GameObject currentScaleGO;
    private bool isStart;
    private bool isAllowedCheck;
    private bool isEnd;
    private int currentScale;
    public int currentItem;
    private StreamWriter questionnaireWriter;
    // public FadeEffect fadeEffect;


    void Start()
    {
        experimentController = this.GetComponent<ExperimentController>();
        questionnaireCanvas.SetActive(false);
        //InitializeQuestionnaire();

    }

    void Update()
    {
        if (!isStart)
        {
            if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.A))
            {
                if (!isEnd)
                {
                    isStart = true;
                    Debug.LogWarning(1);
                    mainText.text = items[currentItem].item; // seems to have an error? ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection.
                    lowAnchorText.text = items[currentItem].lowAnchor;
                    highAnchorText.text = items[currentItem].highAnchor;
                    scale.SetActive(true);
                    currentScaleGO.SetActive(true);
                    smallInstruction.text = (currentItem + 1).ToString("F0") + "/" + items.Count;
                    largeInstruction.text = "Use Left/Right to select and press A to confirm.";
                }
            }
        }
        else
        {
            // collecting questionnaire data
            if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (currentScale > 1)
                {
                    currentScaleGO.SetActive(false);
                    currentScale -= 1;
                    scales[currentScale - 1].SetActive(true);
                    currentScaleGO = scales[currentScale - 1];
                }
            }
            else if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (currentScale < 7)
                {
                    currentScaleGO.SetActive(false);
                    currentScale += 1;
                    scales[currentScale - 1].SetActive(true);
                    currentScaleGO = scales[currentScale - 1];
                }
            }
            else if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.A))
            {
                if (currentItem < items.Count)
                {
                    responses[currentItem] = currentScale;
                    currentScaleGO.SetActive(false);
                    Debug.LogWarning(
                        currentItem                                 + ", " +
                        experimentController.participantID          + ", " +
                        experimentController.vmType                 + ", " +
                        items[currentItem].item                     + ", " +
                        responses[currentItem]                      + ", " +
                        experimentController.currentTime.ToString("F3") );

                    if (!isAllowedCheck)
                    {
                        currentItem += 1;
                        if (currentItem < items.Count-1)
                        {
                            currentScale = 4;
                            currentScaleGO = scales[currentScale - 1];
                        }
                    }
                    else
                    {
                        foreach (var s in scales) s.SetActive(false);
                        scales[responses[currentItem] - 1].SetActive(true);
                        currentScale = responses[currentItem];
                        currentScaleGO = scales[responses[currentItem] - 1];
                    }
                    currentScaleGO.SetActive(true);

                    if (currentItem < items.Count)
                    {
                        mainText.text = items[currentItem].item;
                        lowAnchorText.text = items[currentItem].lowAnchor;
                        highAnchorText.text = items[currentItem].highAnchor;
                        smallInstruction.text = (currentItem + 1).ToString("F0") + "/" + items.Count.ToString();
                    }
                    else
                    {
                        smallInstruction.text = "Use Left/Right to select and press A to confirm.\n" + currentItem.ToString("F0") + "/" + items.Count.ToString();
                        largeInstruction.text = "Use Up/Down to check your responses and press B to proceed.";
                        isAllowedCheck = true;
                        currentItem = items.Count-1;
                    }
                }
            }

            if (isAllowedCheck)
            {
                // once fill out everything
                if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (currentItem > 0)
                    {
                        currentItem -= 1;
                        smallInstruction.text = "Use Left/Right to select and press A to confirm.\n" + (currentItem + 1).ToString("F0") + "/" + items.Count.ToString();
                        mainText.text = items[currentItem].item;
                        lowAnchorText.text = items[currentItem].lowAnchor;
                        highAnchorText.text = items[currentItem].highAnchor;
                        foreach (var s in scales) s.SetActive(false);
                        scales[responses[currentItem] - 1].SetActive(true);
                        currentScale = responses[currentItem];
                        currentScaleGO = scales[responses[currentItem] - 1];
                    }
                }
                else if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (currentItem < items.Count-1)
                    {
                        currentItem += 1;
                        smallInstruction.text = "Use Left/Right to select and press A to confirm.\n" + (currentItem + 1).ToString("F0") + "/" + items.Count.ToString();
                        mainText.text = items[currentItem].item;
                        lowAnchorText.text = items[currentItem].lowAnchor;
                        highAnchorText.text = items[currentItem].highAnchor;
                        foreach (var s in scales) s.SetActive(false);
                        scales[responses[currentItem] - 1].SetActive(true);
                        currentScale = responses[currentItem];
                        currentScaleGO = scales[responses[currentItem] - 1];
                    }

                }
                else if (OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.B))
                {
                    StartCoroutine(WriteQuestionnaireData());
                    Debug.LogWarning("End, Write data");
                    isStart = false;
                    isEnd = true;
                    scale.SetActive(false);
                    smallInstruction.text = "";
                    largeInstruction.text = "";
                    experimentController.isQuestionnaireDone = true;
                    // need to fix this one, it should change with the conditions.
                    // show this bit somewhere else...
                    mainText.text = "Complete one condition.\nPlease do XXX to continue.";
                }
            }
        }
    }


    public void InitializeQuestionnaire()
    {

        string questionnairePath = Helpers.CreateDataPath(experimentController.participantID, "_" + experimentController.vmType.ToString());
        questionnaireWriter = new StreamWriter(questionnairePath, true);
        mainText.text = "Please fill out the questionnaire based on the last experience.";
        isStart = false;
        isEnd = false;
        scale.SetActive(false);

        r03 = new QuestionnaireData("I felt as if the movements of the virtual body were influencing my own movements.", "never", "always");
        r05 = new QuestionnaireData("At some point it felt as if my real body was starting to take on the posture or shape of the virtual body that I saw.", "never", "always");
        r10 = new QuestionnaireData("I felt as if the virtual body was my body.", "never", "always");
        r11 = new QuestionnaireData("At some point it felt that the virtual body resembled my own (real) body in terms of shape skin tone or other visual features.", "never", "always");
        r12 = new QuestionnaireData("I felt as if my body was located where I saw the virtual body.", "never", "always");
        r13 = new QuestionnaireData("I felt like I could control the virtual body as if it was my own body.", "never", "always");
        r14 = new QuestionnaireData("It seemed as if I felt the touch of the floor in the location where I saw the virtual feet touched.", "never", "always");
        goodness = new QuestionnaireData("Please rate the goodness of the experience.", "bad", "good");

        items = new List<QuestionnaireData>();
        items.Add(r03);
        items.Add(r05);
        items.Add(r10);
        items.Add(r11);
        items.Add(r12);
        items.Add(r13);
        items.Add(r14);
        Helpers.Shuffle(items);
        items.Add(goodness);

        responses = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
        currentItem = 0;
        currentScale = 4;
        currentScaleGO = scales[currentScale - 1];
        smallInstruction.text = "";
        largeInstruction.text = "Press A to begin.";
        isAllowedCheck = false;

        foreach (var s in scales) s.SetActive(false);
    }

    IEnumerator WriteQuestionnaireData()
    {
        questionnaireWriter.Write(
            "ParticipantID"         + "," +
            "Visuomotor_type"       + "," +
            "Item"                  + "," +
            "Response"              + "\n");
        //"TimeStamp"             + "\n");
        for (int i = 0; i < responses.Count; i++)
        {
            questionnaireWriter.Write(
                experimentController.participantID                  + "," +
                experimentController.vmType                         + "," +
                items[i].item                                       + "," +
                responses[i]                                        + "\n");
            //experimentController.currentTime.ToString("F3")     + "\n");
        }
        questionnaireWriter.Flush();
        questionnaireWriter.Close();
        yield return 0;
    }

    public struct QuestionnaireData
    {
        public string item;
        public string lowAnchor;
        public string highAnchor;

        public QuestionnaireData(string item, string lowAnchor, string highAnchor)
        {
            this.item = item;
            this.lowAnchor = lowAnchor;
            this.highAnchor = highAnchor;
        }
    }
}


public static class Helpers
{
    public static void Shuffle<T>(this IList<T> list)
    {
        // https://forum.unity.com/threads/randomize-array-in-c.86871/
        // https://stackoverflow.com/questions/273313/randomize-a-listt
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int n = 0; n < list.Count; n++)
        {
            T tmp = list[n];
            int r = UnityEngine.Random.Range(n, list.Count);
            list[n] = list[r];
            list[r] = tmp;
        }
    }

    public static string CreateDataPath(int id, string note = "")
    {
        string fileName = "P" + id.ToString() + note + ".csv";
#if UNITY_EDITOR
        return Application.dataPath + "/Data/" + fileName;
#elif UNITY_ANDROID
        return Application.persistentDataPath + fileName;
#elif UNITY_IPHONE
        return Application.persistentDataPath + "/" + fileName;
#else
        return Application.dataPath + "/" + fileName;
#endif
    }

    public static float RandomGaussian(float minValue = 0f, float maxValue = 1.0f)
    {
        //https://discussions.unity.com/t/normal-distribution-random/66530/4
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }

    public static float DegreeToRadian(float deg)
    {
        return deg * Mathf.PI / 180;
    }
}