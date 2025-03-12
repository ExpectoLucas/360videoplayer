using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneJump : MonoBehaviour 
{
   public void OnStartGame(int sceneNumber)
   {
      
      SceneManager.LoadScene(sceneNumber);
 
   }
   
}
