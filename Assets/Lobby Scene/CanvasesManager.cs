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
        mainMenuCanvas.SetActive(false);
        createRoomCanvas.SetActive(false);
        currentRoomCanvas.SetActive(true);
//  mainCam.SetActive(false);
    }
    public void ShowCreateRoomCanvas()
    {
        if (playerField.text == "")
        {
            errorText.text = "PLayer most have a Nickname.";
            return;
        }
        PhotonNetwork.NickName = playerField.text;

        mainMenuCanvas.SetActive(false);
        currentRoomCanvas.SetActive(false);
        createRoomCanvas.SetActive(true);
      // mainCam.SetActive(false);
    }

    public void ShowMainMenuCanavs()
    {
        currentRoomCanvas.SetActive(false);
        createRoomCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
      //  mainCam.SetActive(true);
    }
}
