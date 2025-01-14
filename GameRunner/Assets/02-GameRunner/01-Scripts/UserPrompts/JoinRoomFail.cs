using Cohort.Prompts;
using Cohort.UI.Generic;
using UnityEngine;

namespace Cohort.UserPrompts {
    public class JoinRoomFail : MonoBehaviour {
        private void Awake() {
            Network.Local.Callbacks.onJoinRoomFailed += OnJoinRoomFailed;
        }

        private void OnDestroy() {
            Network.Local.Callbacks.onJoinRoomFailed -= OnJoinRoomFailed;
        }

        private void OnJoinRoomFailed() {
            UILocator.Get<PromptPanel>().ThrowPrompt("Failed joining room",
                                                     "You were not able to join the game, do you want to try again?",
                                                     Network.Local.RoomManager.OnReconnect);
        }
    }
}