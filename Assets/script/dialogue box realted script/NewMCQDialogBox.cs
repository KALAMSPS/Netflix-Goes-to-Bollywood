
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
            public AudioClip wrongAnswerAudio;           // Audio to play on incorrect answer
            //public MCQImpactData[] mcqImpactValues;

            public bool requiresToggleMCQ;
            public int toggleMCQID;
            public GameObject toggleMCQBox;
            public TMP_Text toggleQuestionText; // Add this to Dialogue class
            public Toggle[] toggleOptions;
            public Button toggleSubmitButton;
            public AudioClip toggleClickAudio;


            // **New Public Lists for Correct & Incorrect Toggles**
            public List<Toggle> correctToggles = new List<Toggle>();
            public List<Toggle> incorrectToggles = new List<Toggle>();
        }
        public GameObject dialbox;
        public TMP_Text dialogueText;
        public TMP_Text speakerText;
        public AudioSource audioSource;
        // Base delays
        private float baseCharacterDelay;
        private float baseMessageDelay;

        // Speed control
        public GameObject sliderContainer;
        public Slider speedSlider;

        public float characterDelay = 0.05f;
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
        public TextAsset toggleJsonFile;
        public string sceneToLoad;                  // Assign in Inspector
        public GameObject voiceObjectToActivate;
        // If true, skip Player 2's response

        private void Start()
        {

            playerController = FindObjectOfType<ThirdPersonController>();
            playerAnimator = playerController.GetComponent<Animator>();
            // Save base values
            baseCharacterDelay = characterDelay;
            baseMessageDelay = messageDelay;

            // Initialize slider UI
            if (sliderContainer != null)
                sliderContainer.SetActive(false);

            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null) Debug.LogError("CameraController not found!");
            else Debug.Log("CameraController cached.");

            // Configure slider range and listener
            if (speedSlider != null)
            {
                speedSlider.minValue = 1;
                speedSlider.maxValue = 10;
                speedSlider.wholeNumbers = true;
                speedSlider.value = 5;
                speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
            }

            LoadMCQDatabase();
            LoadToggleQuestionDatabase();
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

                    //if (dialogue.correctAnswerButton != null)
                    //{
                    //    dialogue.correctAnswerButton.onClick.AddListener(() => OnCorrectAnswerSelected(dialogue));
                    //}


                    if (dialogue.requiresToggleMCQ && dialogue.toggleMCQBox != null)
                    {
                        dialogue.toggleMCQBox.SetActive(false);
                        if (dialogue.toggleSubmitButton != null)
                        {
                            dialogue.toggleSubmitButton.onClick.AddListener(() => OnToggleMCQSubmitted(dialogue));
                        }
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
        void OnSpeedSliderChanged(float value)
        {
            // Normalize slider to [0,1]
            float t = (value - speedSlider.minValue) / (speedSlider.maxValue - speedSlider.minValue);
            // Smooth map to speed factor
            float speed = Mathf.Lerp(0.5f, 2f, t);

            // Adjust text delays and audio pitch
            characterDelay = baseCharacterDelay / speed;
            messageDelay = baseMessageDelay / speed;

            if (audioSource != null)
                audioSource.pitch = speed;
        }
        IEnumerator StartConversation()
        {
            // Enable speed slider at conversation start
            if (sliderContainer != null)
                sliderContainer.SetActive(true);



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



                //if (isPlayer1Turn)
                //{
                //    // Handle multiple MCQs
                //    if (dialogue.requiresMultipleMCQs && dialogue.mcqIDs.Count > 0)
                //    {
                //        StartCoroutine(ShowMultipleMCQs(dialogue));
                //        yield break;
                //    }

                //    // Handle single MCQ
                //    if (dialogue.requiresMCQ)
                //    {
                //        ShowSingleMCQ(dialogue);
                //        yield break;
                //    }
                //}
                if (isPlayer1Turn)
                {
                    // NEW: Skip Player1 dialogue and directly show MCQ
                    if (dialogue.skipPlayer1OnlyShowMCQ && dialogue.requiresMCQ)
                    {
                        if (sliderContainer != null)
                            sliderContainer.SetActive(false);

                        ShowSingleMCQ(dialogue);
                        yield break;
                    }

                    // Handle multiple MCQs
                    if (dialogue.requiresMultipleMCQs && dialogue.mcqIDs.Count > 0)
                    {
                        if (sliderContainer != null)
                            sliderContainer.SetActive(false);

                        StartCoroutine(ShowMultipleMCQs(dialogue));
                        yield break;
                    }

                    // Handle single MCQ
                    if (dialogue.requiresMCQ)
                    {
                        if (sliderContainer != null)
                            sliderContainer.SetActive(false);

                        ShowSingleMCQ(dialogue);
                        yield break;
                    }
                }


                if (isPlayer1Turn && dialogue.requiresToggleMCQ)
                {
                    if (sliderContainer != null)
                        sliderContainer.SetActive(false);

                    if (dialogue.toggleMCQBox != null)
                    {
                        dialogue.toggleMCQBox.SetActive(true);

                        if (dialogue.player1Animator != null)
                            dialogue.player1Animator.Play("Idle"); // ✅ Add this line

                        if (dialogue.player2Animator != null)
                            dialogue.player2Animator.Play("Idle"); // Optional safety

                        if (toggleQuestionDatabase.ContainsKey(dialogue.toggleMCQID))
                        {
                            ToggleQuestionData toggleData = toggleQuestionDatabase[dialogue.toggleMCQID];

                            if (dialogue.toggleQuestionText != null)
                            {
                                dialogue.toggleQuestionText.text = toggleData.question;
                            }
                            else
                            {
                                Debug.LogWarning("Toggle question TMP_Text is not assigned.");
                            }

                            for (int i = 0; i < dialogue.toggleOptions.Length; i++)
                            {
                                if (i < toggleData.options.Length)
                                {
                                    Toggle toggle = dialogue.toggleOptions[i];

                                    Transform labelTransform = toggle.transform.Find("Label");
                                    if (labelTransform != null)
                                    {
                                        TMP_Text labelText = labelTransform.GetComponent<TMP_Text>();
                                        if (labelText != null)
                                        {
                                            labelText.text = toggleData.options[i]; // Set the option text from JSON
                                            Debug.Log($"Toggle {i} label set to: {toggleData.options[i]}");
                                        }
                                        else
                                        {
                                            Debug.LogError("TMP_Text missing on Toggle.Label");
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError("No child named 'Label' found in toggle at index " + i);
                                    }

                                    toggle.tag = toggleData.correctIndices.Contains(i) ? "CorrectAnswer" : "Untagged";
                                    toggle.gameObject.SetActive(true);
                                }
                                else
                                {
                                    dialogue.toggleOptions[i].gameObject.SetActive(false);
                                }
                            }



                        }
                        else
                        {
                            Debug.LogError("ToggleMCQ ID " + dialogue.toggleMCQID + " not found in database.");
                        }
                    }

                    yield break;
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
            if (sliderContainer != null)
                sliderContainer.SetActive(false);

            StartCoroutine(InvokeConversationCompleteWithDelay(1f));

            // onConversationComplete?.Invoke();
        }

        private IEnumerator InvokeConversationCompleteWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Diary();
            onConversationComplete?.Invoke();
            Debug.Log("us100");

            if (playerController != null)
                playerController.enabled = true;
            
        }

        void ShowSingleMCQ(Dialogue dialogue)
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


        //IEnumerator HandleAnswerFeedback(Button selectedButton, bool isCorrect, Dialogue dialogue, float delay, System.Action onComplete)
        //{
        //    Image btnImage = selectedButton.GetComponent<Image>();
        //    if (btnImage != null)
        //    {
        //        btnImage.color = isCorrect ? Color.green : Color.red;
        //    }

        //    foreach (var btn in dialogue.mcqAnswerButtons)
        //    {
        //        btn.interactable = false;
        //    }

        //    yield return new WaitForSeconds(delay);

        //    onComplete?.Invoke();
        //}
        //IEnumerator HandleAnswerFeedback(Button selectedButton, bool isCorrect, Dialogue dialogue, float delay, System.Action onComplete)
        //{
        //    Image btnImage = selectedButton.GetComponent<Image>();
        //    if (btnImage != null)
        //    {
        //        btnImage.color = isCorrect ? Color.green : Color.red;
        //    }

        //    foreach (var btn in dialogue.mcqAnswerButtons)
        //    {
        //        btn.interactable = false;
        //    }

        //    // Wait for 1 second to show feedback (green/red)
        //    yield return new WaitForSeconds(1f);

        //    // Set button color to white
        //    if (btnImage != null)
        //    {
        //        btnImage.color = Color.white;
        //    }

        //    // Wait for the remaining delay (if any)
        //    float remainingDelay = Mathf.Max(0, delay - 1f);
        //    if (remainingDelay > 0)
        //    {
        //        yield return new WaitForSeconds(remainingDelay);
        //    }

        //    onComplete?.Invoke();
        //}

        public bool ShowColorOption;
        public IEnumerator HandleAnswerFeedback(Button selectedButton, bool isCorrect, Dialogue dialogue, float delay, System.Action onComplete)
        {
            Image btnImage = selectedButton.GetComponent<Image>();

            if (ShowColorOption && btnImage != null)
            {
                btnImage.color = isCorrect ? Color.green : Color.red;
            }

            foreach (var btn in dialogue.mcqAnswerButtons)
            {
                btn.interactable = false;
            }

            yield return new WaitForSeconds(1f);

            // Set to white after 1 second
            if (btnImage != null)
            {
                btnImage.color = Color.white;
            }

            // Wait for remaining delay (if any)
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
        public bool delayMCQClose;
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

        /*//ye naya vala hai
         * public void OnCorrectAnswerSelected(Dialogue dialogue)
         {
             Upward.SetActive(true);
             PlayAudio(dialogue.correctAnswerAudio);

             if (mcqDatabase.TryGetValue(dialogue.mcqID, out var data))
             {
                 Button selectedButton = dialogue.mcqAnswerButtons[data.correctIndex];
                 StopAllCoroutines();
                 StartCoroutine(CloseMCQAndContinue(dialogue, selectedButton, true, 0.1f, PerformPostAnswerActivityCorrect));
             }
         }

         public void OnWrongAnswerSelected(Dialogue dialogue, Button selectedButton)
         {
             Downwards.SetActive(true);
             PlayAudio(dialogue.wrongAnswerAudio);
             StopAllCoroutines();
             StartCoroutine(CloseMCQAndContinue(dialogue, selectedButton, false, 1.0f, PerformPostAnswerActivityWrong));
         }

         private IEnumerator CloseMCQAndContinue(Dialogue dialogue, Button selectedButton, bool isCorrect, float feedbackDelay, Func<IEnumerator> postAction)
         {
             yield return new WaitForSeconds(2f); // Wait before closing the MCQ box
             dialogue.mcqBox.SetActive(false);

             yield return StartCoroutine(HandleAnswerFeedback(selectedButton, isCorrect, dialogue, feedbackDelay, () =>
             {
                 StartCoroutine(postAction());

                 // unfreeze camera
                 if (cameraController != null) cameraController.enabled = true;
                 Debug.Log("cameraController.enabled = true;");

                 isPlayer1Turn = false;
                 StartCoroutine(StartConversation());
             }));
         }   ye naya vala hai*/



        // ye purana hia
        //public void OnCorrectAnswerSelected(Dialogue dialogue)
        //{
        //    dialogue.mcqBox.SetActive(false);
        //    Upward.SetActive(true);
        //    PlayAudio(dialogue.correctAnswerAudio);

        //    if (mcqDatabase.TryGetValue(dialogue.mcqID, out var data))
        //    {
        //        Button selectedButton = dialogue.mcqAnswerButtons[data.correctIndex];
        //        StopAllCoroutines();
        //        StartCoroutine(HandleAnswerFeedback(selectedButton, true, dialogue, 0.1f, () =>
        //        {
        //            StartCoroutine(PerformPostAnswerActivityCorrect());

        //            // unfreeze camera
        //            if (cameraController != null) cameraController.enabled = true;
        //            Debug.Log("cameraController.enabled = true;");
        //            isPlayer1Turn = false;
        //            StartCoroutine(StartConversation());
        //        }));
        //    }
        //}

        //public void OnWrongAnswerSelected(Dialogue dialogue, Button selectedButton)
        //{
        //    Downwards.SetActive(true);
        //    dialogue.mcqBox.SetActive(false);
        //    PlayAudio(dialogue.wrongAnswerAudio);
        //    StopAllCoroutines();
        //    StartCoroutine(HandleAnswerFeedback(selectedButton, false, dialogue, 1.0f, () =>
        //    {
        //        StartCoroutine(PerformPostAnswerActivityWrong());

        //        // unfreeze camera
        //        if (cameraController != null) cameraController.enabled = true;
        //        Debug.Log("cameraController.enabled = true;");


        //        isPlayer1Turn = false;
        //        StartCoroutine(StartConversation());
        //    }));
        //}
        // ye purana hai
        private IEnumerator PerformPostAnswerActivityCorrect()
        {
            if (cameraController != null) cameraController.enabled = true;
            Debug.Log("🎬 Starting post-answer activity...");
            Upward.SetActive(true);
            // Place any animations, popups, etc. here
            yield return new WaitForSeconds(2f);
            Upward.SetActive(false);
            Debug.Log("✅ Post-answer activity finished.");
        }
        private IEnumerator PerformPostAnswerActivityWrong()
        {
            if (cameraController != null) cameraController.enabled = true;
            Debug.Log("🎬 Starting post-answer activity...");
            Downwards.SetActive(true);
            // Place any animations, popups, etc. here
            yield return new WaitForSeconds(2f);
            Downwards.SetActive(false);
            Debug.Log("✅ Post-answer activity finished.");
        }
        public void OnToggleMCQSubmitted(Dialogue dialogue)
        {
            PlayAudio(dialogue.toggleClickAudio);

            if (!toggleQuestionDatabase.TryGetValue(dialogue.toggleMCQID, out var toggleData))
            {
                Debug.LogError("ToggleMCQ ID not found in database.");
                return;
            }

            List<int> selectedIndices = new List<int>();
            for (int i = 0; i < dialogue.toggleOptions.Length; i++)
            {
                if (dialogue.toggleOptions[i].isOn)
                {
                    selectedIndices.Add(i);
                }
            }

            bool isCorrect = selectedIndices.Count == toggleData.correctIndices.Count &&
                             !selectedIndices.Except(toggleData.correctIndices).Any();
            Debug.Log("");

            if (isCorrect)
            {
                PerformCorrectPairSelection(dialogue);
                GlobalDataManager.instance.AddRevenue(toggleData.impact.Frequency);
                GlobalDataManager.instance.AddDemand(toggleData.impact.Alignment);
                GlobalDataManager.instance.AddRetailer(toggleData.impact.Accountability);
                Debug.Log("gb100");

                Debug.Log($"✅ Toggle Correct: Revenue={toggleData.impact.Frequency}, Demand={toggleData.impact.Alignment}, Reputation={toggleData.impact.Accountability}");
            }
            else
            {
                ShowIncorrectSelection(dialogue);
                Debug.Log("❌ Toggle Incorrect. No points awarded.");
            }

            dialogue.toggleMCQBox.SetActive(false);
            isPlayer1Turn = false;

            StopAllCoroutines();
            StartCoroutine(StartConversation());
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
                Debug.LogError("MCQ JSON file is not assigned in the inspector.");
                return;
            }
            string wrappedJson = "{\"mcqs\":" + mcqJsonFile.text + "}";
            MCQDataList dataList = JsonUtility.FromJson<MCQDataList>(wrappedJson);
            foreach (var mcq in dataList.mcqs)
            {
                mcqDatabase[mcq.id] = mcq;
             
            }
        }
        private Dictionary<int, ToggleQuestionData> toggleQuestionDatabase = new Dictionary<int, ToggleQuestionData>();
        private void LoadToggleQuestionDatabase()
        {
            if (toggleJsonFile == null)
            {
                Debug.LogError("Toggle JSON file is not assigned.");
                return;
            }
            string wrappedJson = "{\"toggleQuestions\":" + toggleJsonFile.text + "}";
            ToggleQuestionDataList dataList = JsonUtility.FromJson<ToggleQuestionDataList>(wrappedJson);

            foreach (var question in dataList.toggleQuestions)
            {
                toggleQuestionDatabase[question.id] = question;

            }
        }
        public void ApplyMCQImpact(MCQData data, int index)
        {
            if (data.impacts != null && index < data.impacts.Length)
            {
                var impact = data.impacts[index];
                GlobalDataManager.instance.AddRevenue(impact.Frequency);
                GlobalDataManager.instance.AddDemand(impact.Alignment);
                GlobalDataManager.instance.AddRetailer(impact.Accountability);
            }
        }
        [System.Serializable]
        public class ImpactData
        {
            public int Frequency;
            public int Alignment;
            public int Accountability;
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
    }
}

