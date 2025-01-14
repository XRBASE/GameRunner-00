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
                                                     "Something whent wrong joining the photon room! Do you want to try to reconnect?",
                                                     Network.Local.RoomManager.OnReconnect);
        }
    }
}