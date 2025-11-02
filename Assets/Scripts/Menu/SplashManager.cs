using UnityEngine;
using System.Collections;

public class SplashSequence : MonoBehaviour
{

  [SerializeField] private GameObject thirdWardCanvas;
  [SerializeField] private GameObject namesCanvas;
  [SerializeField] private GameObject mainMenuCanvas;


  [SerializeField] private float logoDisplayTime = 2f;
  [SerializeField] private float namesDisplayTime = 2f;

  private IEnumerator Start()
  {

    thirdWardCanvas.SetActive(true);
    namesCanvas.SetActive(false);
    mainMenuCanvas.SetActive(false);

    yield return new WaitForSeconds(logoDisplayTime);


    thirdWardCanvas.SetActive(false);
    namesCanvas.SetActive(true);

    yield return new WaitForSeconds(namesDisplayTime);

    namesCanvas.SetActive(false);
    mainMenuCanvas.SetActive(true);
  }
}
