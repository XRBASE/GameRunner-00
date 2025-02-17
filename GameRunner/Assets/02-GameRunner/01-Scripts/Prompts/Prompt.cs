using System;
using Cohort.UI.Generic;
using Object = UnityEngine.Object;

namespace Cohort.Prompts
{
    /// <summary>
    /// non generic verions with just yes/no button.
    /// </summary>
    public class Prompt
    {
        public Action onAccept;
        public Action onDecline;

        protected PromptVisual _visual;
        protected bool _disposed = false;

        /// <summary>
        /// Create new non-generic version of prompt.
        /// If onDecline is null, the prompt will create a single answer version that includes a close button.
        /// </summary>
        /// <param name="visual">Visual representation of the prompt</param>
        /// <param name="title">Title to display to user.</param>
        /// <param name="message">Message to display to user.</param>
        /// <param name="onAccept">Callback for when user clicks accept.</param>
        /// <param name="onDecline">Callback for when user clicks decline.</param>
        /// <param name="answers">Which answer options should be shown in the prompt.</param>
        public Prompt(PromptVisual visual, string title, string message, Action onAccept = null, Action onDecline = null, Answers answers = (Answers.Yes | Answers.No))
        {
            _visual = visual;
            _visual.Title = title;
            _visual.Message = message;
            _visual.hasContent = false;

            this.onAccept = onAccept;
            _visual.onAccept += OnAccept;
            
            this.onDecline = onDecline;
            _visual.onDecline += OnDecline;

            _visual.answers = answers;
        }

        ~Prompt()
        {
            //dispose if not disposed already.
            Dispose();
        }

        /// <summary>
        /// Clears callbacks and any remaining data. Also deletes the matching visual and its object.
        /// </summary>
        public virtual void Dispose()
        {
            //skip if already disposed
            if (_disposed)
                return;

            onAccept = null;
            onDecline = null;

            if (_visual) {
                Object.Destroy(_visual.gameObject);
                _visual = null;
            }

            _disposed = true;
        }

        public void Accept()
        {
            OnAccept();
            _visual.Accept();
        }

        public void Decline()
        {
            OnDecline();
            _visual.Decline();
        }

        public virtual void OnAccept()
        {
            UILocator.Get<PromptPanel>().RemovePrompt(this);
            onAccept?.Invoke();
        }

        public virtual void OnDecline()
        {
            UILocator.Get<PromptPanel>().RemovePrompt(this);
            onDecline?.Invoke();
        }
    }

    public class Prompt<T> : Prompt
    {
        public T Value { get; set; }
        public new Action<T> onAccept;
        
        public Prompt(PromptVisual visual, string title, string message, Action<T> onAccept = null,
                      Action onDecline = null, Answers answers = (Answers.Yes | Answers.No)) : base(
            visual, title, message, null, onDecline, answers)
        {
            //this version has it's own accept with a type, but otherwise uses the non-generic methods constructor for subscription 
            this.onAccept = onAccept;
        }
        
        /// <summary>
        /// Base prompt accept invokes the generic one, which picks selected out of content list as parameter for the choice.
        /// </summary>
        public override void OnAccept()
        {
            UILocator.Get<PromptPanel>().RemovePrompt(this);
            
            onAccept?.Invoke(Value);
        }
    }

    /// <summary>
    /// Generic version that lets user pick someting of T and click yes/no. Yes returns the picked item.
    /// </summary>
    public class ListPrompt<T> : Prompt<T>
    {
        private T[] _content;

        /// <summary>
        /// Create new generic version of prompt.
        /// If onDecline is null, the prompt will create a single answer version that includes a close button.
        /// </summary>
        /// <param name="visual">Visual representation of the prompt</param>
        /// <param name="title">Title to display to user.</param>
        /// <param name="message">Message to display to user.</param>
        /// <param name="content">List of content for the user to choose from.</param>
        /// <param name="onAccept">Callback for when user clicks accept.</param>
        /// <param name="onDecline">Callback for when user clicks decline.</param>
        public ListPrompt(PromptVisual visual, string title, string message, T[] content, Action<T> onAccept, Action onDecline, Answers answers = (Answers.Yes | Answers.No))
            : base(visual, title, message, onAccept, onDecline, answers)
        {
            _visual.hasContent = true;
            _content = content;
            SetContent();
        }

        /// <summary>
        /// Clears callbacks and any remaining data. Also deletes the matching visual and its object.
        /// </summary>
        public override void Dispose()
        {
            if (_disposed)
                return;

            onAccept = null;

            base.Dispose();
        }

        /// <summary>
        /// Create string array from internal content array and pass that into the visual class to initialize options.
        /// </summary>
        private void SetContent()
        {
            string[] data = new string[_content.Length];
            for (int i = 0; i < _content.Length; i++) {
                data[i] = _content[i].ToString();
            }

            _visual.SetContent(data);
        }

        /// <summary>
        /// Base prompt accept invokes the generic one, which picks selected out of content list as parameter for the choice.
        /// </summary>
        public override void OnAccept()
        {
            Value = _content[_visual.SelectedContent];
            base.OnAccept();
        }
    }

    public class InputPrompt : Prompt<string>
    {
        public InputPrompt(PromptVisual visual, string title, string message, string inputData,
                               Action<string> onAccept, Action onDecline,
                               Answers answers = Answers.Ok | Answers.No) : base(
            visual, title, message, onAccept, onDecline, answers)
        {
            _visual.hasInput = true;
            _visual.InputText = inputData;
        }

        public override void OnAccept()
        {
            Value = _visual.InputText;
            base.OnAccept();
        }
    }

    [Flags]
    public enum Answers
    {
        None = 0,
        Yes = 1<<0,
        No = 1<<1,
        //Ok replaces the yes text with Ok. Yes and Ok together are not supported for this reason.
        Ok = 1<<2, 
        
    }
}