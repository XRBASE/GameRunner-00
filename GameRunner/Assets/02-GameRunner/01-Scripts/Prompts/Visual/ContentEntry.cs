using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cohort.Prompts
{
    public class ContentEntry : MonoBehaviour
    {
        /// <summary>
        /// Name for this specific entry choice.
        /// </summary>
        private string Name {
            get { return nameField.text; }
            set {
                nameField.text = value;
                gameObject.name = value;
            }
        }

        public bool Selected {
            get { return _selector.isOn; }
        }

        public Action<int, bool> onSelect;

        [SerializeField] private TMP_Text nameField;
        [SerializeField] private Toggle _selector;

        private int _id;

        private void Awake()
        {
            //remove all previous listeners from selector if any remain from the copy.
            _selector.onValueChanged.RemoveAllListeners();
            _selector.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnDestroy()
        {
            _selector.onValueChanged.RemoveAllListeners();
            onSelect = null;
        }

        /// <summary>
        /// Called when selected or deselected.
        /// </summary>
        private void OnToggleChanged(bool selected)
        {
            onSelect?.Invoke(_id, selected);
        }

        /// <summary>
        /// Update data of this entry.
        /// </summary>
        /// <param name="id">id of the item in the original list, used for finding the matching generic type.</param>
        /// <param name="name">Name of this field/ option.</param>
        public void SetData(int id, string name)
        {
            _id = id;
            Name = name;
        }

        /// <summary>
        /// Copy this Content entry, used to spawn all choices using the template as sample.
        /// </summary>
        /// <param name="id">Id to assign to the copy.</param>
        /// <param name="name">Name to assign to the copy.</param>
        public ContentEntry Copy(int id, string name)
        {
            ContentEntry entry = Instantiate(this, transform.parent);
            entry.SetData(id, name);

            return entry;
        }
    }
}