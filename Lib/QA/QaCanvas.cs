using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using dk.UI;

namespace dk.QA
{
    public class QaCanvas : MovableUI
    {
        [Header("Buttons")]
        [SerializeField] private Transform _buttonParent;
        [SerializeField] private UI_QaButton _buttonPrefab;

        [Header("Input Field")]
        [SerializeField] private TextMeshProUGUI text_stringCommandHistory;
        [SerializeField] private TMP_InputField inputField_stringCommand;
        [SerializeField] private Button btn_submitStringCommand;
        [SerializeField] private Button btn_help;

        [Header("Toast Message")]
        [SerializeField] private TextMeshProUGUI text_toastMessage;

        [Header("FPS")]
        [SerializeField] private TMP_Text fpsText; // FPS를 표시할 UI 텍스트
        private float deltaTime = 0.0f;

        [Header("Resolution")]
        [SerializeField] private TMP_Text resolutionText; // FPS를 표시할 UI 텍스트

        [Header("QaCanvas")]
        [SerializeField] GameObject _qaCanvas;
        [SerializeField] Button _qaCanvasButton;
        [SerializeField] Button _qaCanvasCloseButton;

        [SerializeField] Button _buttonLogManager;

        private List<UI_QaButton> _buttons = new List<UI_QaButton>();

        public GameObject LogManager { get; set; }

        protected override void Awake()
        {
            base.Awake();

            btn_submitStringCommand.onClick.AddListener(OnClick_SubmitStringCommand);
            btn_help.onClick.AddListener(OnClick_Help);
            _qaCanvasButton.onClick.AddListener(OnClickToggleQA);
            _qaCanvasCloseButton.onClick.AddListener(OnClickCloseQA);
            _buttonLogManager.onClick.AddListener(OnClickToggleLogManager);

            inputField_stringCommand.onEndEdit.AddListener((string str) => { SubmitStringCommand(); });
        }

        private void Update()
        {
            if (QaManager.Instance.IsCommandChangedThisFrame)
            {
                Setup(QaManager.Instance.GetButtonCommands());
            }

            QaManager.Instance.CheckKeyCodeInput = !inputField_stringCommand.isFocused;
            if (inputField_stringCommand.isFocused == false && Input.GetKeyDown(KeyCode.Return))
            {
                inputField_stringCommand.ActivateInputField();
            }

            // fps
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText.text = $"FPS: {Mathf.Round(fps)}"; // FPS 값을 반올림해서 표시
            resolutionText.text = $"Resolution: {Screen.width} x {Screen.height}"; // 해상도 표시
        }

        private void OnEnable()
        {
            //Setup(QaManager.Instance.GetButtonCommands());
        }

        public void Setup(List<ButtonCommand> commands)
        {
            for (int i = _buttons.Count - 1; i >= 0; i--)
            {
                Destroy(_buttons[i].gameObject);
            }
            _buttons.Clear();

            for (int i = 0; i < commands.Count; i++)
            {
                UI_QaButton button = Instantiate(_buttonPrefab, _buttonParent);
                button.Setup(commands[i]);
                _buttons.Add(button);
            }
        }

        public void WriteLog(string msg)
        {
            if (text_stringCommandHistory != null)
            {
                text_stringCommandHistory.text += (msg + "\n");
            }

            Debug.Log(msg);
        }

        private void SubmitStringCommand()
        {
            string command = inputField_stringCommand.text;
            if (command == string.Empty)
                return;

            inputField_stringCommand.text = "";
            WriteLog(command);

            QaManager.Instance.ExecuteStringCommand(command);
        }

        private void OnClick_SubmitStringCommand()
        {
            SubmitStringCommand();
        }

        private void OnClick_Help()
        {
            string log = QaManager.Instance.GetAllCommandsLog();
            WriteLog(log);
        }

        public void ShowToastMessage(string message)
        {
            text_toastMessage.text = message;
        }

        private void OnClickToggleQA()
        {
            _qaCanvas.gameObject.SetActive(!_qaCanvas.gameObject.activeInHierarchy);
        }

        private void OnClickCloseQA()
        {
            _qaCanvas.gameObject.SetActive(false);
        }

        private void OnClickToggleLogManager()
        {
            if(LogManager != null)
                LogManager.SetActive(!LogManager.activeInHierarchy);
        }
    }
}