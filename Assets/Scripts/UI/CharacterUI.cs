
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fluid
{
    public class CharacterUI : MonoBehaviour
    {
        [SerializeField] private Image _portrait;
        [SerializeField] private TMPro.TextMeshProUGUI _name;
        [SerializeField] private Image _job;
        [SerializeField] private Button _button;

        public string Name
        {
            get => _name.text;
            set => _name.text = value;
        }

        public Sprite Portrait
        {
            get => _portrait.sprite;
            set => _portrait.sprite = value;
        }

        public void UpdateJob(Sprite sprite)
        {
            if (sprite == null)
            {
                _job.sprite = sprite;
                _job.color = new Color(0,0,0,0);
            }
            else
            {
                _job.sprite = sprite;
                _job.color = new Color(1, 1, 1, 1);
            }
        }

        public void ListenClick(UnityAction listener)
        {
            _button.onClick.AddListener(listener);
        }
    }
}