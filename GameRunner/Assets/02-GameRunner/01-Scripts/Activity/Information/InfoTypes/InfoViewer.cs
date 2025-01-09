using System;
using UnityEngine;

namespace Cohort.GameRunner.InformationPoints {
	/// <summary>
	/// Shows information required for solving the minigames.
	/// </summary>
	public abstract class InfoViewer : MonoBehaviour {
		/// <summary>
		/// Initializes the information viewer with given json data.
		/// </summary>
		/// <param name="infoData">Json information data.</param>
		/// <param name="onInfoClosed">close action that enables the game input again.</param>
		public abstract void Initialize(string infoData, Action onInfoClosed);
		
		public abstract void CloseInfoViewer();
		
		protected virtual void Awake() {
			InfoManager.Instance.InitializeInfo(this);
		}
	}
}