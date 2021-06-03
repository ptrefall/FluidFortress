using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fluid
{
    public class UI : MonoBehaviour
    {
        public static UI Instance { get; private set; }

        [SerializeField] private RectTransform _actionMenu;
        [SerializeField] private Button _build;
        [SerializeField] private Button _dig;
        [SerializeField] private Button _gather;
        [SerializeField] private Button _decorate;
        [SerializeField] private Button _cancel;
        [SerializeField] private TMPro.TextMeshProUGUI _layer;
        [SerializeField] private VerticalLayoutGroup _characterGroup;
        [SerializeField] private CharacterUI _characterUiPrefab;

        public bool IsModal { get; set; } = false;

        public void UpdateLayer(int layer)
        {
            _layer.text = $"L:{layer}";
        }

        public void ShowSelectionActions()
        {
            _actionMenu.gameObject.SetActive(true);
            IsModal = true;
        }

        public void HideSelectionActions()
        {
            _actionMenu.gameObject.SetActive(false);
            IsModal = false;
        }

        public void ListenDecorate(UnityAction listener)
        {
            _decorate.onClick.AddListener(listener);
        }

        public void ListenBuild(UnityAction listener)
        {
            _build.onClick.AddListener(listener);
        }

        public void ListenDig(UnityAction listener)
        {
            _dig.onClick.AddListener(listener);
        }

        public void ListenGather(UnityAction listener)
        {
            _gather.onClick.AddListener(listener);
        }

        public void ListenCancel(UnityAction listener)
        {
            _cancel.onClick.AddListener(listener);
        }

        public void AddCharacter(Character character)
        {
            if (character == null)
            {
                return;
            }

            var characterUi = Instantiate(_characterUiPrefab, _characterGroup.transform);
            character.SetUi(characterUi);
        }

        private void Awake()
        {
            Instance = this;
            HideSelectionActions();
        }
    }
}