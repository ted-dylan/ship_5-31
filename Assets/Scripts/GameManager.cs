using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Ready, Playing, Win, Lose }
    public GameState CurrentState { get; private set; } = GameState.Ready;

    [Header("Game Settings")]
    [SerializeField] private float totalTime = 120f;
    [SerializeField] private int crashLimit = 1;

    [Header("Boat Skin Settings")]
    [SerializeField] private GameObject[] boatModels;
    private int currentBoatIndex = 0;

    [Header("UI Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject skinShopPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI collisionText;
    [SerializeField] private TextMeshProUGUI winScoreText;
    [SerializeField] private TextMeshProUGUI loseScoreText;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform goal;

    private float remainingTime;
    private int score = 1000;
    private int collisionCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // [변수 선언부 Header("Game Settings") 아래에 추가]
    [SerializeField] private bool startImmediately = false; // 켜면 시작 화면 없이 바로 게임 시작

    // [기존 Start 함수를 아래 내용으로 완전 대체]
    void Start()
    {
        remainingTime = totalTime;
        UpdateBoatSkin();

        // 팩트: 즉시 시작 기능이 켜져 있다면 대기하지 않고 바로 StartGame 실행
        if (startImmediately)
        {
            StartGame();
        }
        else
        {
            SetState(GameState.Ready);
            ShowStartPanel();
        }
    }

    void Update()
    {
        if (CurrentState != GameState.Playing) return;
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f) { remainingTime = 0f; Lose(); return; }
        UpdateUI();
    }

    // --- [UI 제어 로직: 카메라 개입 완전 삭제] ---
    public void OpenSkinShop()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (skinShopPanel != null) skinShopPanel.SetActive(true);
    }

    public void CloseSkinShop()
    {
        if (skinShopPanel != null) skinShopPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
    }

    // --- [배 선택 로직] ---
    public void NextBoat()
    {
        currentBoatIndex = (currentBoatIndex + 1) % boatModels.Length;
        UpdateBoatSkin();
    }

    public void PrevBoat()
    {
        currentBoatIndex--;
        if (currentBoatIndex < 0) currentBoatIndex = boatModels.Length - 1;
        UpdateBoatSkin();
    }

    private void UpdateBoatSkin()
    {
        if (boatModels == null || boatModels.Length == 0) return;
        for (int i = 0; i < boatModels.Length; i++)
        {
            if (boatModels[i] != null) boatModels[i].SetActive(i == currentBoatIndex);
        }
    }

    // --- [기존 게임 제어 로직] ---
    public void StartGame()
    {
        Time.timeScale = 1f;
        score = 1000;
        collisionCount = 0;
        remainingTime = totalTime;
        SetState(GameState.Playing);
        HideAllPanels();
    }

    // [기존 Win 함수를 대체하는 코드]
    public void Win()
    {
        // 중복 실행 방지
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.Win);

        // 주의: Time.timeScale = 0f; 를 쓰지 않습니다. (배가 멈추지 않고 자연스럽게 전진하도록)
        // 승리 UI를 띄우지 않고 곧바로 전환 대기열(코루틴)을 실행합니다.
        StartCoroutine(AutoNextStageRoutine());
    }

    // [새로 추가하는 코루틴(대기열) 함수]
    private System.Collections.IEnumerator AutoNextStageRoutine()
    {
        // 팩트: 1.5초 동안 대기합니다. (원하는 시간으로 수정 가능)
        yield return new WaitForSeconds(1.5f);

        // 현재 씬 번호 + 1 계산
        int nextSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;

        // 다음 씬이 유니티에 등록되어 있는지 검사
        if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // 마지막 스테이지를 깼을 경우, 메인 화면(0번 씬)으로 강제 복귀
            Debug.Log("모든 스테이지 클리어! 메인 화면으로 돌아갑니다.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public void Lose()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.Lose);
        if (losePanel != null) losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RegisterCollision()
    {
        if (CurrentState != GameState.Playing) return;
        collisionCount++;
        score = Mathf.Max(0, score - 100);
        if (collisionCount >= crashLimit) Lose();
    }

    private void UpdateUI()
    {
        if (distanceText != null) distanceText.text = $"Distance: {Vector3.Distance(player.position, goal.position):F1}m";
        if (timerText != null) timerText.text = $"Time: {(int)remainingTime / 60:00}:{(int)remainingTime % 60:00}";
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (collisionText != null) collisionText.text = $"Collisions: {collisionCount}";
    }

    private void ShowStartPanel()
    {
        HideAllPanels();
        if (startPanel != null) startPanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (skinShopPanel != null) skinShopPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void RestartGame() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    private void SetState(GameState newState) { CurrentState = newState; }
}