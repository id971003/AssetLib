using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dk.QA
{
    public class UI_QaButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Button _button;

        private ButtonCommand _command;

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        public void Setup(ButtonCommand command)
        {
            _text.text = command.btnText;
            _command = command;
        }

        private void OnClick()
        {
            _command.Execute();
        }
    }
}
