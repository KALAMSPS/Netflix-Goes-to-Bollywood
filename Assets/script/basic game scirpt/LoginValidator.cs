using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions; // To handle special characters check

public class LoginValidator : MonoBehaviour
{
    public TMP_InputField usernameField; // TextMesh Pro input fields
    public TMP_InputField passwordField;
    public float warningDuration = 2f;
    public GameObject Lobby;

    void Start()
    {
        passwordField.contentType = TMP_InputField.ContentType.Password;
        passwordField.ForceLabelUpdate(); // Ensures the visual update
    }

    public void ValidateLogin()
    {
        string username = usernameField.text.Trim();
        string password = passwordField.text.Trim();

        // Username validation
        if (string.IsNullOrEmpty(username))
        {
            ShowWarning("Username cannot be empty.");
            return;
        }

        if (username.Length < 4)
        {
            ShowWarning("Username must be at least 4 characters.");
            return;
        }

        if (username.Length > 20)
        {
            ShowWarning("Username must be 20 characters or less.");
            return;
        }

        if (ContainsSpecialCharacters(username))
        {
            ShowWarning("Username cannot contain special characters.");
            return;
        }

        // Password validation
        if (string.IsNullOrEmpty(password))
        {
            ShowWarning("Password cannot be empty.");
            return;
        }

        if (password.Length < 8)
        {
            ShowWarning("Password must be at least 8 characters.");
            return;
        }

        SavePlayerInfo();
        Debug.Log("Login valid!");
        Lobby.SetActive(true);
        gameObject.SetActive(false);

    }

    // Check if the username contains special characters
    bool ContainsSpecialCharacters(string input)
    {
        string pattern = @"[^a-zA-Z0-9]";  // Regex to match anything that's not a letter or number
        return Regex.IsMatch(input, pattern);
    }

    void ShowWarning(string message)
    {
        StopAllCoroutines();
        StartCoroutine(HideWarningAfterDelay());
    }

    IEnumerator HideWarningAfterDelay()
    {
        yield return new WaitForSeconds(warningDuration);
    }
    public void SavePlayerInfo()
    {
        GlobalDataManager.instance.globalData.playerName = usernameField.text;
        GlobalDataManager.instance.globalData.password= passwordField.text;

    }
}
