using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CanvasesManager : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject createRoomCanvas;
    public GameObject mainCam;
    public GameObject currentRoomCanvas;
    [SerializeField] Text errorText;

    [SerializeField] InputField playerField;

    public void ShowCurrentRoomCanvas()
    {
        errorText.text = "";
        mainMenuCanvas.SetActive(false);
        createRoomCanvas.SetActive(false);
        currentRoomCanvas.SetActive(true);
    }
    public void ShowCreateRoomCanvas()
    {
        if (playerField.text == "")
        {
            errorText.text = "PLayer most have a Nickname.";
            return;
        }
        errorText.text = "";
        PhotonNetwork.NickName = playerField.text;
        mainMenuCanvas.SetActive(false);
        currentRoomCanvas.SetActive(false);
        createRoomCanvas.SetActive(true);
    }

    public void ShowMainMenuCanavs()
    {
        errorText.text = "";
        currentRoomCanvas.SetActive(false);
        createRoomCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }
}
