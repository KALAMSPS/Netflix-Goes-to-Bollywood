using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TMP_TabNavigation : MonoBehaviour
{
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public GameObject loginButton; // Optional: add login button focus

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (usernameField.isFocused)
            {
                EventSystem.current.SetSelectedGameObject(passwordField.gameObject);
            }
            else if (passwordField.isFocused)
            {
                EventSystem.current.SetSelectedGameObject(loginButton); // optional
            }
        }
    }
}
