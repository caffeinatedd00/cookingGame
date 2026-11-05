using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public enum Phase
{
    Prep,
    Open,
    Closed
}
public class TimeManager : MonoBehaviour
{
    [SerializeField] private GameObject spawner;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text clock;
    [SerializeField] private GameObject dayOverview;
    [SerializeField] private TMP_Text profitText;
    [SerializeField] private TMP_Text salesText;
    [SerializeField] private float timeScale = 0.8f;

    private float currentTime;
    private float openTime = 6f * 60f;
    private float closeTime = 18f * 60f;
    public int profits = 0;
    public int sales = 0;

    public static TimeManager Instance;
    public Phase currentPhase { get; private set; }

    private void Awake()
    {
        Instance = this;
        currentPhase = Phase.Prep;
        closeButton.interactable = false;
    }

    private void Update() //starts the clock at 6am when shop open button is clicked and stops and set time to 00:00 when shop is closed or close time is reached
    {
        if (currentPhase != Phase.Open)
        {
            clock.text = "00:00";
            return;
        }
        currentTime += Time.deltaTime * timeScale;

        int totalMinutes = Mathf.FloorToInt(currentTime);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        clock.text = $"{hours:00}:{minutes:00}";

        if (currentTime >= closeTime)
        {
            CloseShop();
        }

    }

    public void OpenShop() //starts timer and changes the current phase
    {
        currentTime = openTime;
        currentPhase = Phase.Open;
        openButton.interactable = false;
        closeButton.interactable = true;
        spawner.SetActive(true);
    }

    public void CloseShop() //stops timer, show daily profit and resets daily profits
    {
        currentPhase = Phase.Closed;
        spawner.SetActive(false);

        closeButton.interactable = false;
        openButton.interactable = true;

        profitText.text = profits.ToString();
        salesText.text = sales.ToString();

        profits = 0;
        sales = 0;

        if (overviewRoutine != null)
            StopCoroutine(overviewRoutine);

        overviewRoutine = StartCoroutine(ShowDayOverview(3.5f));
    }

    private Coroutine overviewRoutine;

    private IEnumerator ShowDayOverview(float duration) //shows daily profit for a set duration
    {
        dayOverview.SetActive(true);

        yield return new WaitForSeconds(duration);

        dayOverview.SetActive(false);
    }

}
