using System;

public class InteractiveVideo : VideoViewer
{
    public InteractiveButton _nteractiveButton;
    
    public override void Initialize(string gameData, float timeLimit, int minScore, int maxScore,
        Action<FinishCause, int> onFinished, Action onExit) {
        base.Initialize(gameData, timeLimit, minScore, maxScore, onFinished, onExit);

        
    }
}
