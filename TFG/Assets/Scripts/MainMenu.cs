using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button playOnlineButton;
    public Button freePracticeButton;
    public Button newTerrainButton;
    public Button backButton;

    public Text usernameText;

    private void Start()
    {
        string username = PlayerPrefs.GetString("username", "Guest");
        usernameText.text = "Welcome, " + username + "!";
    }



    public void onBackButtonClick()
    {
        PlayerPrefs.SetString("username", "");
        SceneManager.LoadScene("Login");
    }

    public void onPlayOnlineClick()
    {
        PlayerPrefs.SetInt("isOnline", 1);
        SceneManager.LoadScene("TerrainSelector");
    }

    public void onFreePracticeClick()
    {
        PlayerPrefs.SetInt("isOnline", 0);
        SceneManager.LoadScene("TerrainSelector");
    }

    public void onNewTerrainClick()
    {
        SceneManager.LoadScene("NewTerrain");
    }
}
