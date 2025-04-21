using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class UserRequest
{
    public string name;
    public string password;
}
public class LoginMenu : MonoBehaviour
{
    private string loginUrl = "/login";
    private string registerUrl = "/register-user";

    public InputField usernameInput;
    public InputField passwordInput;
    public Text responseText;
    public Button loginButton;
    public Button registerButton;
    private Requests requestHandler;

    private void Start()
    {
        requestHandler = new Requests();
    }

    private void Update()
    {
        if (usernameInput.text.Length <= 0 || passwordInput.text.Length <= 0)
        {
            loginButton.interactable = false;
            registerButton.interactable = false;
        }
        else
        {
            loginButton.interactable = true;
            registerButton.interactable = true;
        }

    }

    public void OnLoginButtonClick()
    {
        responseText.text = "";
        UserRequest userRequest = new UserRequest
        {
            name = usernameInput.text,
            password = passwordInput.text
        };
        string json = JsonUtility.ToJson(userRequest);
        StartCoroutine(requestHandler.PostRequest(loginUrl, json, OnLoginResponse));
    }

    private void OnLoginResponse(string response)
    {
        if (response.Contains("ERROR: 401"))
        {
            responseText.text = "Invalid username or password.";
            responseText.color = Color.red;
        }
        else if (response.Contains("ERROR: 500"))
        {
            responseText.text = "Server error. Please try again later.";
            responseText.color = Color.red;
        }
        else if (response.Contains("ERROR: 404"))
        {
            responseText.text = "Endpoint not found.";
            responseText.color = Color.red;
        }
        else if (response.Contains("ERROR: 400"))
        {
            responseText.text = "Bad request. Please check your input.";
            responseText.color = Color.red;
        }
        else
        {
            PlayerPrefs.SetString("username", usernameInput.text);
            responseText.text = "Login successful!";
            responseText.color = Color.green;
            SceneManager.LoadScene("MainMenu");
        }
        usernameInput.text = "";
        passwordInput.text = "";
    }

    public void OnRegisterButtonClick()
    {
        responseText.text = "";
        UserRequest userRequest = new UserRequest
        {
            name = usernameInput.text,
            password = passwordInput.text
        };
        string json = JsonUtility.ToJson(userRequest);
        StartCoroutine(requestHandler.PostRequest(registerUrl, json, OnRegisterResponse));
    }

    private void OnRegisterResponse(string response)
    {
        usernameInput.text = "";
        passwordInput.text = "";
        if (response.Contains("ERROR: 400"))
        {
            responseText.text = "User already registered.";
            responseText.color = Color.red;
        }
        else if (response.Contains("ERROR: 500"))
        {
            responseText.text = "Server error. Please try again later.";
            responseText.color = Color.red;
        }
        else
        {
            responseText.text = "Registration successful!";
            responseText.color = Color.green;
        }
    }

    public void OnBackButton()
    {
        Debug.Log("Back button clicked");
    }
}
