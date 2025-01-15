#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneContainer", menuName = "Cohort/CreateSceneContainer")]
public class SceneContainer : ScriptableObject
{
    public List<Object> Scenes;
}

#endif