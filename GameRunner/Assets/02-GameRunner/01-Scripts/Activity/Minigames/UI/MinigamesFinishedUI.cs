using Cohort.GameRunner.Minigames;
using Cohort.UI.Generic;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(103)] //After MinigameManager
public class MinigamesFinishedUI : UIPanel {
    public const string SCORE_KEYWORD = "[POINTS]";

    [SerializeField] private TMP_Text _body;

    private string _templateText;
    
    private void Awake() {
        UILocator.Register(this);

        _templateText = _body.text;
        
        MinigameManager.Instance.onAllMinigamesFinished += OnAllMinigamesFinished;
        Deactivate();
    }

    private void OnDestroy() {
        MinigameManager.Instance.onAllMinigamesFinished -= OnAllMinigamesFinished;
    }

    private void OnAllMinigamesFinished(int score) {
        string bodyTx = _templateText;
        if (bodyTx.Contains(SCORE_KEYWORD)) {
            bodyTx = bodyTx.Replace(SCORE_KEYWORD, score.ToString());
        }

        _body.text = bodyTx;
        
        Activate();
    }
}
