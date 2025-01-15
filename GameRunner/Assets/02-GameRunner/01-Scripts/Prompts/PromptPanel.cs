using System;
using System.Collections.Generic;
using Cohort.UI.Generic;
using UnityEngine;

namespace Cohort.Prompts
{
    /// <summary>
    /// Over all panel in which prompts are shown.
    /// </summary>
    public class PromptPanel : UIPanel //this is panel not content, because the panel is not in the container, but center screen.
    {
        
        // Prefab to use when no prefab is given along with the request to prompt the user.
        [SerializeField] private PromptVisual _basePrefab;
        //list of all (active) prompts (should usually be one).
        private List<Prompt> _prompts;

        protected void Awake() {
            UILocator.Register(this);
            
            _prompts = new List<Prompt>();
        }

        /// <summary>
        /// Close and dispose all prompts (does not invoke any callback).
        /// </summary>
        private void CloseAll()
        {
            for (int i = 0; i < _prompts.Count; i++) {
                _prompts[i].Dispose();
            }
            _prompts.Clear();
            Deactivate();
        }

        public void RemovePrompt(Prompt toRemove)
        {
            _prompts.Remove(toRemove);
            if (_prompts.Count <= 0) {
                Deactivate();
            }
        }

        /// <summary>
        /// Create prompt with message and show to user.
        /// </summary>
        /// <param name="title">Title to show to the user.</param>
        /// <param name="message">Message to show to the user.</param>
        /// <param name="onAccept">Callback when user clicks accept or yes.</param>
        /// <param name="onDecline">Callback when user clicks decline or no.</param>
        /// <returns>Prompt for later subscription or management purposes.</returns>
        public Prompt ThrowPrompt(string title, string message, Action onAccept = null, Action onDecline = null, Answers answers = Answers.Yes | Answers.No)
        {
            Activate();
            Prompt p = new Prompt(GetVisual(), title, message, onAccept, onDecline, answers);
            _prompts.Add(p);
            return p;
        }

        /// <summary>
        /// Create prompt with message and show to user.
        /// </summary>
        /// <param name="visualPrefab">Prefab to use for the prompt visuals instead of default.</param>
        /// <param name="message">Message to show to the user.</param>
        /// <param name="onAccept">Callback when user clicks accept or yes.</param>
        /// <param name="onDecline">Callback when user clicks decline or no.</param>
        /// <returns>Prompt for later subscription or management purposes.</returns>
        public Prompt ThrowPrompt(PromptVisual visualPrefab, string title, string message, Action onAccept = null, Action onDecline = null, Answers answers = Answers.Yes | Answers.No)
        {
            Activate();
            Prompt p = new Prompt(GetVisual(visualPrefab), title, message, onAccept, onDecline, answers);
            _prompts.Add(p);
            return p;
        }

        /// <summary>
        /// Create typed(generic) prompt with message and show to user.
        /// </summary>
        /// <param name="title">Title to show to the user.</param>
        /// <param name="message">Message to show to the user.</param>
        /// <param name="content">Content list for user to choose from.</param>
        /// <param name="onAccept">Callback when user clicks accept or yes.</param>
        /// <param name="onDecline">Callback when user clicks decline or no.</param>
        /// <returns>Prompt for later subscription or management purposes.</returns>
        public ListPrompt<T> ThrowListPrompt<T>(string title, string message, T[] content, Action<T> onAccept = null, Action onDecline = null, Answers answers = Answers.Yes | Answers.No)
        {
            Activate();
            ListPrompt<T> p = new ListPrompt<T>(GetVisual(), title, message, content, onAccept, onDecline, answers);
            _prompts.Add(p);
            return p;
        }
        
        /// <summary>
        /// Create typed(generic) prompt with message and show to user.
        /// </summary>
        /// <param name="title">Title to show to the user.</param>
        /// <param name="message">Message to show to the user.</param>
        /// <param name="visualPrefab">Prefab to use for the prompt visuals instead of default.</param>
        /// <param name="content">Content list for user to choose from.</param>
        /// <param name="onAccept">Callback when user clicks accept or yes.</param>
        /// <param name="onDecline">Callback when user clicks decline or no.</param>
        /// <returns>Prompt for later subscription or management purposes.</returns>
        public ListPrompt<T> ThrowListPrompt<T>(PromptVisual visualPrefab, string title, string message, T[] content, Action<T> onAccept = null, Action onDecline = null, Answers answers = Answers.Yes | Answers.No)
        {
            Activate();
            ListPrompt<T> p = new ListPrompt<T>(GetVisual(visualPrefab), title, message, content, onAccept, onDecline, answers);
            _prompts.Add(p);
            return p;
        }
        
        /// <summary>
        /// Create prompt with user input and show to user.
        /// </summary>
        /// <param name="title">Title to show to the user.</param>
        /// <param name="message">Message to show to the user.</param>
        /// <param name="inputData">Data to prefill in the inputfield.</param>
        /// <param name="onAccept">Callback when user clicks accept or yes.</param>
        /// <param name="onDecline">Callback when user clicks decline or no.</param>
        /// <returns>Prompt for later subscription or management purposes.</returns>
        public InputPrompt ThrowInputPrompt(string title, string message, string inputData = "", Action<string> onAccept = null, Action onDecline = null, Answers answers = Answers.Yes | Answers.No)
        {
            Activate();
            InputPrompt p = new InputPrompt(GetVisual(), title, message, inputData, onAccept, onDecline, answers);
            _prompts.Add(p);
            return p;
        }
        
        /// <summary>
        /// Create prompt with user input and show to user.
        /// </summary>
        /// <param name="title">Title to show to the user.</param>
        /// <param name="message">Message to show to the user.</param>
        /// /// <param name="inputData">Data to prefill in the inputfield.</param>
        /// <param name="visualPrefab">Prefab to use for the prompt visuals instead of default.</param>
        /// <param name="onAccept">Callback when user clicks accept or yes.</param>
        /// <param name="onDecline">Callback when user clicks decline or no.</param>
        /// <returns>Prompt for later subscription or management purposes.</returns>
        public InputPrompt ThrowInputPrompt(PromptVisual visualPrefab, string title, string message, string inputData = "", Action<string> onAccept = null, Action onDecline = null, Answers answers = Answers.Yes | Answers.No)
        {
            Activate();
            InputPrompt p = new InputPrompt(GetVisual(visualPrefab), title, message, inputData, onAccept, onDecline, answers);
            _prompts.Add(p);
            return p;
        }

        /// <summary>
        /// Get instance of base visual for prompt.
        /// </summary>
        private PromptVisual GetVisual()
        {
            return GetVisual(_basePrefab);
        }

        /// <summary>
        /// Get instance of visual for prompt.
        /// </summary>
        private PromptVisual GetVisual(PromptVisual prefab)
        {
            return Instantiate(prefab, transform);
        }

        public override void Activate() {
            base.Activate();
                
            transform.SetAsLastSibling();
        }
    }
}