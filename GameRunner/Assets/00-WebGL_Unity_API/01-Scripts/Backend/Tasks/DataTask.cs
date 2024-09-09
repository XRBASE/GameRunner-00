using UnityEngine.Events;

/// <summary>
/// Singular task for (backend) data retrieval.
/// </summary>
/// <typeparam name="T">return type of task and onComplete parameter</typeparam>
public class DataTask<T>
{
    /// <summary>
    /// Has this task been completed
    /// </summary>
    public bool Completed {
        get { return completed; }
    }
    
    public bool Failed {
        get { return failed; }
    }

    public string Error {
        get { return error; }
    }
    
    private bool completed = false;
    private bool failed = false;
    private string error = "";

    /// <summary>
    /// Callback for when the task completes
    /// </summary>
    public UnityAction<T> onComplete;
    public UnityAction<string> onFailed;
    
    /// <summary>
    /// List task to perform, before complete or failed can be called
    /// </summary>
    public UnityAction<UnityAction<T>, UnityAction<string>> action;
    /// <summary>
    /// Result of the task.
    /// </summary>
    public T result;
    
    public DataTask(UnityAction<UnityAction<T>, UnityAction<string>> action, UnityAction<T> onComplete = null, UnityAction<string> onFailed = null)
    {
        this.action = action;
        this.onComplete = onComplete;
        this.onFailed = onFailed;
    }

    /// <summary>
    /// Start task.
    /// </summary>
    public void Execute()
    {
        this.action.Invoke(OnComplete, OnFailed);
    }

    /// <summary>
    /// Complete task
    /// </summary>
    /// <param name="result">result of task./param>
    public void OnComplete(T result)
    {
        this.result = result;
        completed = true;
        onComplete?.Invoke(result);
    }
    
    public void OnFailed(string error)
    {
        failed = true;
        completed = true;
        this.error = error;
        onFailed?.Invoke(error);
    }
}