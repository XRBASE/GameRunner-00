using Cohort.BackendData;

using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// Queue of backend tasks, which all overwrite the previously gotten result.
/// </summary>
/// <typeparam name="T">type of datacontainer that is retrieved by each task (and overwrites previous results).</typeparam>
public class TaskQueue<T> where T : DataContainer
{
    //TODO: is task queue still needed?
    //I think better design might be not to queue calls, but make one call for that data, check with ruben
    
    /// <summary>
    /// List of tasks, to which to pass the previous result and from which to retrieve the new data.
    /// </summary>
    private Queue<UnityAction<T, UnityAction<T>, UnityAction<string>>> tasks;
    /// <summary>
    /// Callback after queue has been finished.
    /// </summary>
    public UnityAction<T> onComplete;
    public UnityAction<string> onFailed;
    /// <summary>
    /// Result of all tasks (new data is overwritten, old data is kept).
    /// </summary>
    public T result;

    public TaskQueue()
    {
        tasks = new Queue<UnityAction<T, UnityAction<T>, UnityAction<string>>>();
    }
    
    /// <summary>
    /// Constructor for taskqueue
    /// </summary>
    /// <param name="capacity">amount of tasks that will be added.</param>
    public TaskQueue(int capacity)
    {
        tasks = new Queue<UnityAction<T, UnityAction<T>, UnityAction<string>>>(capacity);
    }

    /// <summary>
    /// Add task to the queue.
    /// </summary>
    /// <param name="task">task to add.</param>
    public void Add(UnityAction<T, UnityAction<T>, UnityAction<string>> task)
    {
        tasks.Enqueue(task);
    }

    /// <summary>
    /// Start queue execution.
    /// </summary>
    /// <param name="input">initial input for first method.</param>
    public void Execute(T input)
    {
        //dequeue the next task in the list and add on finished to the onfinished call
        tasks.Dequeue().Invoke(input, OnTaskFinished, OnTaskFailed);
    }

    /// <summary>
    /// Called when one task has been completed.
    /// </summary>
    /// <param name="item">result of the previous task.</param>
    private void OnTaskFinished(T item)
    {
        //update item with new values from next task
        if (result == null) {
            result = item;
        }
        else {
            result.Overwrite(item);
        }
        
        //check if there's more tasks
        if (tasks.Count > 0) {
            Execute(result);
        }
        else {
            //if no more tasks, then execute onfinished
            onComplete?.Invoke(result);
        }
    }

    private void OnTaskFailed(string error)
    {
        Debug.LogError($"queue failure ({error})!");
        onFailed?.Invoke(error);
    }
}