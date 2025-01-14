using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cohort.Prompts
{
    public class PromptVisual : MonoBehaviour
    {
        /// <summary>
        /// Message to show to the user, sets and gets TMP value.
        /// </summary>
        public string Title {
            get { return _titleField.text; }
            set {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _titleField.text = "";
                    _titleObject.gameObject.SetActive(false);
                }
                else {
                    _titleField.text = value;
                    _titleObject.gameObject.SetActive(true);
                }
            }
        }
        
        /// <summary>
        /// Message to show to the user, sets and gets TMP value.
        /// </summary>
        public string Message {
            get { return _messageField.text; }
            set {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _messageField.text = "";
                    _messageField.gameObject.SetActive(false);
                }
                else {
                    _messageField.text = value;
                    _messageField.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Currently selected option.
        /// </summary>
        public int SelectedContent { get; private set; }
        public string InputText {
            get { return _inputField.text; }
            set { _inputField.text = value; }
        }

        public Action onAccept;
        public Action onDecline;

        public bool hasContent;
        public bool hasInput;
        public Answers answers;

        [SerializeField] private TMP_Text _titleField;
        [SerializeField] private GameObject _titleObject;
        [SerializeField] private TMP_Text _messageField;
        
        [SerializeField] private Button _acceptBtn;
        [SerializeField] private Button _declineBtn;

        [SerializeField] private Transform _scrollView;
        [SerializeField] private Transform _contentParent;
        
        [SerializeField] private TMP_InputField _inputField;

        private List<ContentEntry> _entries;

        public void Start()
        {
            //disable content parent if there is no content
            _scrollView.gameObject.SetActive(hasContent);
            _inputField.gameObject.SetActive(hasInput);
            
            if (answers.HasFlag(Answers.Ok)) {
                if (answers.HasFlag(Answers.Yes)) {
                    Decline();
                    throw new Exception("Ok and yes cannot be used together, please pick one of the two");
                }
                
                _acceptBtn.GetComponentInChildren<TMP_Text>().text = "Ok";
            }
            _acceptBtn.onClick.AddListener(Accept);
            _declineBtn.gameObject.SetActive((answers & (Answers.Yes | Answers.Ok)) > 0);
            //no decline action means no decline button and a close button (that does not trigger decline, but does close the prompt)
            _declineBtn.onClick.AddListener(Decline);
            _declineBtn.gameObject.SetActive(answers.HasFlag(Answers.No));
        }

        /// <summary>
        /// Called by active content entry to set selected ID.
        /// </summary>
        /// <param name="entryId"></param>
        private void OnEntrySelected(int entryId, bool selected)
        {
            if (selected) {
                _acceptBtn.interactable = true;
                SelectedContent = entryId;
            }
            else {
                for (int i = 0; i < _entries.Count; i++) {
                    //if any value is on, do nothing
                    if (_entries[i].Selected) {
                        return;
                    }
                }

                //if no value selected, disable accept button.
                _acceptBtn.interactable = false;
            }
        }

        public void Accept()
        {
            onAccept?.Invoke();
            Close();
        }

        public void Decline()
        {
            onDecline?.Invoke();
            Close();
        }

        /// <summary>
        /// Destroy gameobject and clear callbacks.
        /// </summary>
        private void Close()
        {
            onAccept = null;
            onDecline = null;

            Destroy(gameObject);
        }

        /// <summary>
        /// Set content for generic choice prompts. Strings are shown after toggle for the user to select.
        /// </summary>
        /// <param name="contents">list of names for the user to pick from.</param>
        /// <exception cref="MissingReferenceException">Exception thrown when this prefab misses the template entry.</exception>
        public void SetContent(string[] contents)
        {
            //if generic type prompt, this button is not interactable till the first value has been set
            _acceptBtn.interactable = false;

            //this retrieves the initial or "template" item to use as prefab for all the other items.
            ContentEntry template = _contentParent.GetChild(0).GetComponent<ContentEntry>();

            if (!hasContent || template == null) {
                //template for content entry should be included in the prefab if the prompt is of a generic type (i.e. has options apart from yes/no)
                throw new MissingReferenceException($"Missing template for content entries in prompt {Message}!");
            }

            _entries = new List<ContentEntry>();
            _entries.Add(template);

            if (contents.Length == 0) {
                template.gameObject.SetActive(false);
                return;
            }
            else {
                template.gameObject.SetActive(true);
            }

            //template is used as initial value
            _entries[0].SetData(0, contents[0]);
            _entries[0].onSelect = OnEntrySelected;

            for (int i = 1; i < contents.Length; i++) {
                //copy template for every content and set id and value
                _entries.Add(_entries[0].Copy(i, contents[i]));
                _entries[i].onSelect = OnEntrySelected;
            }
        }
    }
}