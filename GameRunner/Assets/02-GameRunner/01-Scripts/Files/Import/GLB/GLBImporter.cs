using System;
using Cohort.Patterns;
using GLTFast;
using ReadyPlayerMe.Core;
using UnityEngine;

namespace Cohort.Import.GLB {
	public class GLBImporter : Singleton<GLBImporter> {
		public GLTFDeferAgent gltfDeferAgent;

		private void Start() {
			AvatarLoaderSettings loaderSettings = AvatarLoaderSettings.LoadSettings();
			gltfDeferAgent = Instantiate(loaderSettings.GLTFDeferAgent).GetComponent<GLTFDeferAgent>();
		}

		public void ImportGLBFile(byte[] data, Transform parent, Action<GameObject> onLoadingFinished) {
			LoadGltfBinary(data, parent, onLoadingFinished);
		}

		private async void LoadGltfBinary(byte[] data, Transform parent, Action<GameObject> onLoadingFinished) {
			if (parent == null) {
				parent = new GameObject("Model").transform;
			}

			parent.gameObject.SetActive(false);

			var matGen = new GlbMaterialGenerator();
			var gltf = new GltfImport(null, gltfDeferAgent.GetGLTFastDeferAgent(), matGen);

			bool success = await gltf.LoadGltfBinary(
				data,
				// The URI of the original data is important for resolving relative URIs within the glTF
				null
			);
			if (success) {
				if (await gltf.InstantiateMainSceneAsync(parent)) {
					parent.gameObject.SetActive(true);
					onLoadingFinished?.Invoke(parent.gameObject);
				}
				else {
					Debug.LogError("instantiation error");
				}
			}
		}
	}
}