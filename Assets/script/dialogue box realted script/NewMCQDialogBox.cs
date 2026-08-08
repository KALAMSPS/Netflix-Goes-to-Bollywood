
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace secondWeek_Lu
{
    public class NewMCQDialogBox : MonoBehaviour
    {
        [System.Serializable]
        public class Dialogue
        {
            public bool skipPlayer1OnlyShowMCQ;
            public string player1Role;
            public string player1Text;
            public AudioClip player1Audio;
            public GameObject playerAnimEnable;
            public Animator player1Animator;

            public bool skipPlayer2;
            public string player2Role;
            public string player2Text;
            public AudioClip player2Audio;
            public GameObject playerAnimEnable2;
            public Animator player2Animator;

            public bool requiresMCQ;
            public int mcqID;
            public bool requiresMultipleMCQs;
            public List<int> mcqIDs = new List<int>(); // List of multiple MCQ IDs
            public GameObject mcqBox;                    // UI container for MCQ
            public TMP_Text mcqQuestionText;             // Text field for displaying the question
            public Button[] mcqAnswerButtons;            // Buttons for each answer option
            public AudioClip correctAnswerAudio;         // Audio to play on correct answer
            public AudioClip wrongAnswerAudio;
            public bool requires3OptionMCQ;
            public int mcq3ID;
            public GameObject mcq3Box;
            public TMP_Text mcq3QuestionText;
            public Button[] mcq3AnswerButtons;
            public AudioClip correctAnswerAudio3;
            public AudioClip wrongAnswerAudio3;
        }
        public GameObject dialbox;
        public TMP_Text dialogueText;
        public TMP_Text speakerText;
        public AudioSource audioSource;
        // Base delays
        private float baseCharacterDelay;
        private float baseMessageDelay;

        public float characterDelay;
        public float messageDelay = 1.0f;

        public Dialogue[] dialogues;
        public UnityEvent onConversationComplete;

        private int currentDialogueIndex = 0;
        private bool isPlayer1Turn = true;
        public GameObject Upward;
        public GameObject Downwards;

        [Header("Player Control")]
        public ThirdPersonController playerController;
        public Animator playerAnimator;

        private CameraController cameraController;
        public string currentTaskString;
        public TMP_Text CurrentTask;
        public TextAsset mcqJsonFile;
        public string sceneToLoad;                  // Assign in Inspector
        public GameObject voiceObjectToActivate;
        public bool delayMCQClose;
        public bool ShowColorOption;
        public bool SceneLoad;
        // If true, skip Player 2's response

        private void Start()
        {

            playerController = FindObjectOfType<ThirdPersonController>();
            playerAnimator = playerController.GetComponent<Animator>();
            // Save base values
            baseCharacterDelay = characterDelay;
            baseMessageDelay = messageDelay;
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null) Debug.LogError("CameraController not found!");
            LoadMCQDatabase();
            if (dialogueText != null && speakerText != null && dialogues.Length > 0)
            {
                dialogueText.text = "";
                speakerText.text = "";

                foreach (var dialogue in dialogues)
                {
                    if (dialogue.mcqBox != null)
                    {
                        dialogue.mcqBox.SetActive(false);
                    }
                    if (dialogue.mcq3Box != null)
                    {
                        dialogue.mcq3Box.SetActive(false);
                    }

                }
                if (playerController != null)
                    playerController.enabled = false;
                if (playerAnimator != null)
                    playerAnimator.SetBool("run", false);
                //playerAnimator.Play("Idle");
                StartCoroutine(StartConversation());
            }
        }
        void Diary()
        {
            // canMove = canMove;
            if (CurrentTask != null)
            {
                CurrentTask.text = currentTaskString;
                Debug.Log("Diary called!");
            }
        }

        IEnumerator StartConversation()
        {
            while (currentDialogueIndex < dialogues.Length)
            {
                var dialogue = dialogues[currentDialogueIndex];

                // 🔁 EARLY EXIT IF WE NEED TO SKIP PLAYER 2
                if (!isPlayer1Turn && dialogue.skipPlayer2)
                {
                    currentDialogueIndex++;
                    isPlayer1Turn = true;
                    continue;
                }


                string speaker = isPlayer1Turn ? dialogue.player1Role : dialogue.player2Role;
                string message = isPlayer1Turn ? dialogue.player1Text : dialogue.player2Text;
                AudioClip audioClip = isPlayer1Turn ? dialogue.player1Audio : dialogue.player2Audio;

                GameObject activeAnim = isPlayer1Turn ? dialogue.playerAnimEnable : dialogue.playerAnimEnable2;
                GameObject inactiveAnim = isPlayer1Turn ? dialogue.playerAnimEnable2 : dialogue.playerAnimEnable;
                Animator activeAnimator = isPlayer1Turn ? dialogue.player1Animator : dialogue.player2Animator;
                Animator inactiveAnimator = isPlayer1Turn ? dialogue.player2Animator : dialogue.player1Animator;

                if (activeAnim != null) activeAnim.SetActive(true);
                if (inactiveAnim != null) inactiveAnim.SetActive(false);

                if (!(isPlayer1Turn && dialogue.skipPlayer1OnlyShowMCQ))
                    yield return StartCoroutine(TypeText(speaker, message, audioClip, activeAnimator, inactiveAnimator));

                if (isPlayer1Turn)
                {
                    // NEW: Skip Player1 dialogue and directly show MCQ
                    if (dialogue.skipPlayer1OnlyShowMCQ && dialogue.requiresMCQ)
                    {
                        ShowSingleMCQ(dialogue);
                        yield break;
                    }

                    // Handle multiple MCQs
                    if (dialogue.requiresMultipleMCQs && dialogue.mcqIDs.Count > 0)
                    {
                        StartCoroutine(ShowMultipleMCQs(dialogue));
                        yield break;
                    }

                    // Handle single MCQ
                    if (dialogue.requiresMCQ)
                    {
                        ShowSingleMCQ(dialogue);
                        yield break;
                    }
                    if (dialogue.requires3OptionMCQ)
                    {
                        Show3OptionMCQ(dialogue);
                        yield break;
                    }
                }

                yield return new WaitForSeconds(messageDelay);

                if (!isPlayer1Turn)
                {
                    // If we are on Player 2's turn and skipPlayer2 is true, skip this turn
                    if (dialogue.skipPlayer2)
                    {
                        currentDialogueIndex++;
                        isPlayer1Turn = true; // jump back to Player 1
                    }
                    else
                    {
                        currentDialogueIndex++;
                        isPlayer1Turn = true;
                    }
                }
                else
                {
                    // If Player 1 just spoke and dialogue indicates Player 2 is skipped
                    if (dialogue.skipPlayer2)
                    {
                        currentDialogueIndex++;
                        isPlayer1Turn = true; // jump to next Player 1 dialogue
                    }
                    else
                    {
                        isPlayer1Turn = false; // allow Player 2 to speak
                    }
                }

            }

            if (dialogues.Length > 0 && dialogues[dialogues.Length - 1].player2Animator != null)
            {
                dialogues[dialogues.Length - 1].player2Animator.Play("Idle");
            }
            StartCoroutine(InvokeConversationCompleteWithDelay(1f));

            // onConversationComplete?.Invoke();
        }

        private IEnumerator InvokeConversationCompleteWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Diary();
            onConversationComplete?.Invoke();
            Debug.Log("us100");
            if(SceneLoad)
            {
                SceneManager.LoadScene(sceneToLoad);
            }

            if (playerController != null)
                playerController.enabled = true;

        }

        void ShowSingleMCQ(Dialogue dialogue)
        {
            Debug.Log("ShowSingleMCQ Start");
            ResetSingleMCQ(dialogue);
            if (dialogue.player1Animator != null)
                dialogue.player1Animator.Play("Idle");

            if (cameraController != null) cameraController.enabled = false;

            dialogueText.text = "";
            speakerText.text = "";

            if (dialogue.playerAnimEnable != null)
                dialogue.playerAnimEnable.SetActive(false);
            if (dialogue.playerAnimEnable2 != null)
                dialogue.playerAnimEnable2.SetActive(false);

            if (dialogue.player1Animator != null)
                dialogue.player1Animator.Play("Idle");
            if (dialogue.player2Animator != null)
                dialogue.player2Animator.Play("Idle");
            if (dialogue.player1Animator != null)
                dialogue.player1Animator.Play("Idle");
            //gameObject.transform.GetChild(0).gameObject.SetActive(false);// Gaurav
            if (dialogue.mcqBox != null)
            {
                dialbox.SetActive(false);
                dialogue.mcqBox.SetActive(true);
            }

            if (mcqDatabase.ContainsKey(dialogue.mcqID))
            {
                MCQData data = mcqDatabase[dialogue.mcqID];

                if (dialogue.mcqQuestionText != null)
                    dialogue.mcqQuestionText.text = data.question;

                for (int i = 0; i < dialogue.mcqAnswerButtons.Length; i++)
                {
                    TMP_Text btnText = dialogue.mcqAnswerButtons[i].GetComponentInChildren<TMP_Text>();
                    if (i < data.options.Length)
                    {
                        if (btnText != null) btnText.text = data.options[i];
                        Button btn = dialogue.mcqAnswerButtons[i];
                        btn.onClick.RemoveAllListeners();
                        int index = i;

                        btn.onClick.AddListener(() =>
                        {
                            ApplyMCQImpact(data, index);
                            if (index == data.correctIndex)
                                OnCorrectAnswerSelected(dialogue);
                            else
                                OnWrongAnswerSelected(dialogue, btn);
                        });
                    }
                }
                //gameObject.transform.GetChild(0).gameObject.SetActive(true);//Gaurav
            }
            else
            {
                Debug.LogError("MCQ ID " + dialogue.mcqID + " not found in database.");
            }
            //dialogue.mcqBox.SetActive(false);
        }
        void Show3OptionMCQ(Dialogue dialogue)
        {
            Debug.Log("Show3OptionMCQ Start");
            Reset3OptionMCQ(dialogue);
            if (dialogue.player1Animator != null)
                dialogue.player1Animator.Play("Idle");

            if (cameraController != null) cameraController.enabled = false;

            dialogueText.text = "";
            speakerText.text = "";

            if (dialogue.playerAnimEnable != null)
                dialogue.playerAnimEnable.SetActive(false);
            if (dialogue.playerAnimEnable2 != null)
                dialogue.playerAnimEnable2.SetActive(false);

            if (dialogue.player1Animator != null)
                dialogue.player1Animator.Play("Idle");
            if (dialogue.player2Animator != null)
                dialogue.player2Animator.Play("Idle");
            if (dialogue.player1Animator != null)
                dialogue.player1Animator.Play("Idle");
            //gameObject.transform.GetChild(0).gameObject.SetActive(false);// Gaurav
            if (dialogue.mcq3Box != null)
            {
                dialbox.SetActive(false);
                dialogue.mcq3Box.SetActive(true);
            }

            if (mcqDatabase.ContainsKey(dialogue.mcq3ID))
            {
                MCQData data = mcqDatabase[dialogue.mcq3ID];

                if (dialogue.mcq3QuestionText != null)
                    dialogue.mcq3QuestionText.text = data.question;

                for (int i = 0; i < dialogue.mcq3AnswerButtons.Length; i++)
                {
                    TMP_Text btnText = dialogue.mcq3AnswerButtons[i].GetComponentInChildren<TMP_Text>();
                    if (i < data.options.Length)
                    {
                        if (btnText != null) btnText.text = data.options[i];
                        Button btn = dialogue.mcq3AnswerButtons[i];
                        btn.onClick.RemoveAllListeners();
                        int index = i;

                        btn.onClick.AddListener(() =>
                        {
                            ApplyMCQImpact(data, index);
                            if (index == data.correctIndex)
                                OnCorrectAnswerSelected3(dialogue);
                            else
                                OnWrongAnswerSelected3(dialogue, btn);
                        });
                    }
                    else
                    {
                        dialogue.mcq3AnswerButtons[i].gameObject.SetActive(false);
                    }
                }
                //gameObject.transform.GetChild(0).gameObject.SetActive(true);//Gaurav
            }
            else
            {
                Debug.LogError("MCQ ID " + dialogue.mcqID + " not found in database.");
            }
            //dialogue.mcqBox.SetActive(false);
        }

        IEnumerator ShowMultipleMCQs(Dialogue dialogue)
        {
            if (dialogue.player1Animator != null)
                dialogue.player1Animator.Play("Idle");



            if (cameraController != null) cameraController.enabled = false;
            dialogueText.text = "";
            speakerText.text = "";

            if (dialogue.playerAnimEnable != null)
                dialogue.playerAnimEnable.SetActive(false);
            if (dialogue.playerAnimEnable2 != null)
                dialogue.playerAnimEnable2.SetActive(false);

            if (dialogue.player1Animator != null)
                dialogue.player1Animator.Play("Idle");
            if (dialogue.player2Animator != null)
                dialogue.player2Animator.Play("Idle");
            if (dialogue.player1Animator != null)
                dialogue.player1Animator.Play("Idle");
            dialbox.SetActive(false);

            dialogue.mcqBox.SetActive(true); // Keep visible across all MCQs

            for (int i = 0; i < dialogue.mcqIDs.Count; i++)
            {
                int mcqID = dialogue.mcqIDs[i];

                if (!mcqDatabase.ContainsKey(mcqID))
                {
                    Debug.LogError($"MCQ ID {mcqID} not found.");
                    continue;
                }

                MCQData data = mcqDatabase[mcqID];

                if (dialogue.mcqQuestionText != null)
                    dialogue.mcqQuestionText.text = data.question;

                bool answered = false;

                for (int j = 0; j < dialogue.mcqAnswerButtons.Length; j++)
                {
                    Button btn = dialogue.mcqAnswerButtons[j];

                    if (j < data.options.Length)
                    {
                        TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
                        if (btnText != null) btnText.text = data.options[j];

                        btn.gameObject.SetActive(true);
                        btn.interactable = true;

                        int index = j;

                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() =>
                        {
                            ApplyMCQImpact(data, index);

                            if (index == data.correctIndex)
                            {
                                if (cameraController != null) cameraController.enabled = true;
                                Upward.SetActive(true);
                                PlayAudio(dialogue.correctAnswerAudio);
                                StartCoroutine(HandleAnswerFeedback(btn, true, dialogue, 0.2f, () => { answered = true; }));
                            }
                            else
                            {
                                if (cameraController != null) cameraController.enabled = true;
                                Downwards.SetActive(true);
                                PlayAudio(dialogue.wrongAnswerAudio);
                                StartCoroutine(HandleAnswerFeedback(btn, false, dialogue, 0.7f, () => { answered = true; }));
                            }
                        });
                    }
                    else
                    {
                        btn.gameObject.SetActive(false);
                    }
                }

                yield return new WaitUntil(() => answered);
            }

            dialogue.mcqBox.SetActive(false);
            isPlayer1Turn = false;
            StartCoroutine(StartConversation());
        }



        IEnumerator TypeText(string speaker, string message, AudioClip audioClip, Animator activeAnimator, Animator inactiveAnimator)
        {
            dialogueText.text = "";
            speakerText.text = speaker + "";

            if (inactiveAnimator != null)
            {
                inactiveAnimator.Play("Idle");
            }

            if (activeAnimator != null)
            {
                activeAnimator.Play("Talking");
            }

            if (audioSource != null && audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }

            float typingTime = message.Length * characterDelay;
            foreach (char letter in message)
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(characterDelay);
            }

            if (audioClip != null)
            {
                float remainingAudioTime = Mathf.Max(0, audioClip.length - typingTime);
                yield return new WaitForSeconds(remainingAudioTime);
            }

            if (activeAnimator != null)
            {
                activeAnimator.Play("Typing");
            }

            yield return new WaitForSeconds(messageDelay);
        }

        public IEnumerator HandleAnswerFeedback(Button selectedButton, bool isCorrect, Dialogue dialogue, float delay, System.Action onComplete)
        {
            Image btnImage = selectedButton.GetComponent<Image>();

            if (ShowColorOption && btnImage != null)
            {
                btnImage.color = isCorrect ? Color.green : Color.red;
            }

            foreach (var btn in dialogue.mcqAnswerButtons)
            {
                if (btn == null)
                {
                    continue;
                }

                btn.interactable = false;
            }

            yield return new WaitForSeconds(1f);
            if (btnImage != null)
            {
                btnImage.color = Color.white;
            }

            float remainingDelay = Mathf.Max(0, delay - 1f);
            if (remainingDelay > 0)
            {
                yield return new WaitForSeconds(remainingDelay);
            }

            onComplete?.Invoke();
        }
        public IEnumerator HandleAnswerFeedback3(Button selectedButton, bool isCorrect, Dialogue dialogue, float delay, System.Action onComplete)
        {
            Image btnImage = selectedButton.GetComponent<Image>();

            if (ShowColorOption && btnImage != null)
            {
                btnImage.color = isCorrect ? Color.green : Color.red;
            }

            foreach (var btn in dialogue.mcq3AnswerButtons)
            {
                btn.interactable = false;
            }

            yield return new WaitForSeconds(1f);

            if (btnImage != null)
            {
                btnImage.color = Color.white;
            }

            float remainingDelay = Mathf.Max(0, delay - 1f);
            if (remainingDelay > 0)
            {
                yield return new WaitForSeconds(remainingDelay);
            }

            onComplete?.Invoke();
        }
        // OLD: Keeps compatibility with existing calls
        IEnumerator HandleAnswerFeedback(Button selectedButton, bool isCorrect, Dialogue dialogue, float delay)
        {
            yield return HandleAnswerFeedback(selectedButton, isCorrect, dialogue, delay, null);
        }
        private IEnumerator HideMCQBoxAfterDelay(GameObject mcqBox, float delay)
        {
            yield return new WaitForSeconds(delay);
            mcqBox.SetActive(false);
        }

        public void OnWrongAnswerSelected(Dialogue dialogue, Button selectedButton)
        {
            if (delayMCQClose)
                StartCoroutine(HideMCQBoxAfterDelay(dialogue.mcqBox, 2f));
            else
                dialogue.mcqBox.SetActive(false);

            Downwards.SetActive(true);
            PlayAudio(dialogue.wrongAnswerAudio);
            StopAllCoroutines();
            StartCoroutine(HandleAnswerFeedback(selectedButton, false, dialogue, 1.0f, () =>
            {
                StartCoroutine(PerformPostAnswerActivityWrong());

                if (cameraController != null) cameraController.enabled = true;
                Debug.Log("cameraController.enabled = true;");

                isPlayer1Turn = false;
                StartCoroutine(StartConversation());
            }));
        }
        public void OnWrongAnswerSelected3(Dialogue dialogue, Button selectedButton)
        {
            if (delayMCQClose)
                StartCoroutine(HideMCQBoxAfterDelay(dialogue.mcq3Box, 2f));
            else
                dialogue.mcq3Box.SetActive(false);

            Downwards.SetActive(true);
            PlayAudio(dialogue.wrongAnswerAudio);
            StopAllCoroutines();
            StartCoroutine(HandleAnswerFeedback3(selectedButton, false, dialogue, 1.0f, () =>
            {
                StartCoroutine(PerformPostAnswerActivityWrong());

                if (cameraController != null) cameraController.enabled = true;
                Debug.Log("cameraController.enabled = true;");

                isPlayer1Turn = false;
                StartCoroutine(StartConversation());
            }));
        }
        public void OnCorrectAnswerSelected(Dialogue dialogue)
        {
            if (delayMCQClose)
                StartCoroutine(HideMCQBoxAfterDelay(dialogue.mcqBox, 2f));
            else
                dialogue.mcqBox.SetActive(false);

            Upward.SetActive(true);
            PlayAudio(dialogue.correctAnswerAudio);

            if (mcqDatabase.TryGetValue(dialogue.mcqID, out var data))
            {
                Button selectedButton = dialogue.mcqAnswerButtons[data.correctIndex];
                StopAllCoroutines();
                StartCoroutine(HandleAnswerFeedback(selectedButton, true, dialogue, 0.1f, () =>
                {
                    StartCoroutine(PerformPostAnswerActivityCorrect());

                    if (cameraController != null) cameraController.enabled = true;
                    Debug.Log("cameraController.enabled = true;");
                    isPlayer1Turn = false;
                    StartCoroutine(StartConversation());
                }));
            }
        }
        public void OnCorrectAnswerSelected3(Dialogue dialogue)
        {
            if (delayMCQClose)
                StartCoroutine(HideMCQBoxAfterDelay(dialogue.mcq3Box, 2f));
            else
                dialogue.mcq3Box.SetActive(false);

            Upward.SetActive(true);
            PlayAudio(dialogue.correctAnswerAudio);

            if (mcqDatabase.TryGetValue(dialogue.mcq3ID, out var data))
            {
                Button selectedButton = dialogue.mcq3AnswerButtons[data.correctIndex];
                Debug.Log("Selected Button: " + selectedButton.name);
                StopAllCoroutines();
                StartCoroutine(HandleAnswerFeedback3(selectedButton, true, dialogue, 0.1f, () =>
                {
                    StartCoroutine(PerformPostAnswerActivityCorrect());

                    if (cameraController != null) cameraController.enabled = true;
                    Debug.Log("cameraController.enabled = true;");
                    isPlayer1Turn = false;
                    StartCoroutine(StartConversation());
                }));
            }
        }
        private IEnumerator PerformPostAnswerActivityCorrect()
        {
            if (cameraController != null) cameraController.enabled = true;
            Upward.SetActive(true);
            // Place any animations, popups, etc. here
            yield return new WaitForSeconds(2f);
            Upward.SetActive(false);
            Debug.Log("✅ Post-answer activity finished.");
        }
        private IEnumerator PerformPostAnswerActivityWrong()
        {
            if (cameraController != null) cameraController.enabled = true;
            Downwards.SetActive(true);
            // Place any animations, popups, etc. here
            yield return new WaitForSeconds(2f);
            Downwards.SetActive(false);
        }


        public void PerformCorrectPairSelection(Dialogue dialogue)
        {
            PlayAudio(dialogue.correctAnswerAudio);
        }

        public void ShowIncorrectSelection(Dialogue dialogue)
        {
            Debug.Log("Incorrect toggle pair selected! Highlighting wrong choices.");

        }
        private void PlayAudio(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
        public void StartSceneLoadProcess()
        {
            StartCoroutine(PlayVoiceThenLoadScene(sceneToLoad, voiceObjectToActivate));
        }

        private IEnumerator PlayVoiceThenLoadScene(string sceneName, GameObject voiceObject)
        {
            if (voiceObject != null)
            {
                voiceObject.SetActive(true);
                AudioSource audio = voiceObject.GetComponent<AudioSource>();
                Debug.Log("793");
                if (audio != null && audio.clip != null)
                {
                    audio.Play();
                    yield return new WaitForSeconds(audio.clip.length);
                }
            }
            Debug.Log("799");
            SceneManager.LoadScene(sceneName);
            Debug.Log("802");
        }
        public void ChangeInformation(string changeInformationText)
        {
            if (PlayerTriggerActivity.instance.infomationHeading != null)
            {

                PlayerTriggerActivity.instance.infomationHeading.SetActive(true);
            }
            Text_Animation.instance.textInfomation.text = changeInformationText;
        }
        private Dictionary<int, MCQData> mcqDatabase = new Dictionary<int, MCQData>();
        private void LoadMCQDatabase()
        {
            if (mcqJsonFile == null)
            {
                return;
            }

            string wrappedJson = "{\"mcqs\":" + mcqJsonFile.text + "}";
            MCQDataList dataList = JsonUtility.FromJson<MCQDataList>(wrappedJson);

            Debug.Log("Total MCQs loaded: " + dataList.mcqs.Count);

            foreach (var mcq in dataList.mcqs)
            {
                mcqDatabase[mcq.id] = mcq;
            }
        }
        private Dictionary<int, ToggleQuestionData> toggleQuestionDatabase = new Dictionary<int, ToggleQuestionData>();

        public void ApplyMCQImpact(MCQData data, int index)
        {
            Debug.Log("Data Null : " + (data == null));
            Debug.Log("Impacts Null : " + (data.impacts == null));
            Debug.Log("Impact Length : " + (data.impacts != null ? data.impacts.Length : -1));
            Debug.Log("Global Instance : " + GlobalDataManager.instance);
            if (data.impacts != null && index < data.impacts.Length)
            {
                var impact = data.impacts[index];
                GlobalDataManager.instance.AddSGI(impact.SGI);
                GlobalDataManager.instance.AddFHI(impact.FHI);
                GlobalDataManager.instance.AddCGI(impact.CGI);
            }
        }
        [System.Serializable]
        public class ImpactData
        {
            public int SGI;
            public int FHI;
            public int CGI;
        }
        [System.Serializable]
        public class MCQData
        {
            public int id;
            public string question;
            public string[] options;
            public int correctIndex;
            public ImpactData[] impacts;  // New!
        }
        [System.Serializable]
        public class MCQDataList
        {
            public List<MCQData> mcqs;
        }
        [System.Serializable]
        public class ToggleQuestionData
        {
            public int id;
            public string question;
            public string[] options;
            public List<int> correctIndices;
            public ImpactData impact;
        }
        [System.Serializable]
        public class ToggleQuestionDataList
        {
            public List<ToggleQuestionData> toggleQuestions;
        }

        void ResetSingleMCQ(Dialogue dialogue)
        {
            dialogue.mcqQuestionText.text = "";

            foreach (Button btn in dialogue.mcqAnswerButtons)
            {
                if (btn == null)
                {
                    Debug.LogError("NULL Button mila");
                    continue;
                }
                btn.interactable = true;
                btn.gameObject.SetActive(true);
                Image img = btn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.white;
                }
                TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.text = "";
                }
                btn.onClick.RemoveAllListeners();
            }
        }

        void Reset3OptionMCQ(Dialogue dialogue)
        {
            dialogue.mcq3QuestionText.text = "";

            foreach (Button btn in dialogue.mcq3AnswerButtons)
            {
                if (btn == null)
                {
                    continue;
                }
                btn.interactable = true;
                btn.gameObject.SetActive(true);

                Image img = btn.GetComponent<Image>();

                if (img != null)
                {
                    img.color = Color.white;
                }

                TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();

                if (txt != null)
                {
                    txt.text = "";
                }
                btn.onClick.RemoveAllListeners();
            }
        }
    }
}