using Cohort.GameRunner.LocoMovement;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[DefaultExecutionOrder(-1)] //before anything that might need a spawnpoint reference (e.g. Locomotion).
public class SpawnPoint : MonoBehaviour
{
    public const int DEFAULT = 0;

    private static Dictionary<int, SpawnPoint> _spawnPoints = new Dictionary<int, SpawnPoint>();
    
    public int Id
    {
        get
        {
            return _id;
        }
    }
    [SerializeField] private int _id;
    
    private void Awake()
    {
        if (_spawnPoints is null)
            _spawnPoints = new Dictionary<int, SpawnPoint>();
        if (_spawnPoints.ContainsKey(_id)) {
            Debug.LogError($"Double id exception, removing spawnpoint {_id}");
            Destroy(this);
            return;
        }
        
        _spawnPoints.Add(_id, this);
    }

    private void OnDestroy()
    {
        _spawnPoints.Remove(_id);
    }

    public static bool TryGetById(int id, out SpawnPoint spawn) {
        if (_spawnPoints.ContainsKey(id)) {
            spawn = _spawnPoints[id];
            return true;
        }

        spawn = null;
        return false;
    }
    
    public static SpawnPoint GetById(int id)
    {
        if (_spawnPoints.ContainsKey(id)) {
            return _spawnPoints[id];
        }
        return null;
    }

    public static Vector3 DefaultSpawnPosition()
    {
        SpawnPoint spawn = GetById(DEFAULT);
        if( spawn != null)
        {
            return spawn.transform.position;
        }
        return Vector3.zero;
    }
    
    public void MoveToSpawnPoint()
    {
        Locomotion.Local.MoveTo(transform.position);
    }
    
    /// <summary>
    /// Teleports the localplayer to the spawnpoint and orients the avatar towards the z+ axis of this transform
    /// </summary>
    public void TeleportToSpawnPoint()
    {
        Locomotion.Local.TeleportTo(transform.position, transform.rotation);
        //TODO_COHORT: camera
        //CameraState.Instance.RotateCamera(transform.rotation);
    }
    
#if UNITY_EDITOR
[CustomEditor(typeof(SpawnPoint))]
    private class SpawnPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var node = (SpawnPoint) target;
            if(node._id== DEFAULT)
                EditorGUILayout.HelpBox("This is the default spawnpoint", MessageType.Info);
            else
                EditorGUILayout.HelpBox($"Make sure you have 1 spawnpoint set as the default with id = {DEFAULT}. This will be the main spawnpoint for the players. Also make sure that each spawnpoint has a unique ID", MessageType.Info);
            if (GUILayout.Button("Snap nearest collider below"))
            {
                if (Physics.Raycast(node.transform.position, Vector3.down, out var hit))
                {
                    node.transform.position = hit.point;
                }
            }
        }
    }
    #endif
}
