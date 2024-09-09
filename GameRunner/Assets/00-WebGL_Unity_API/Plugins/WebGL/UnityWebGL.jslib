mergeInto(LibraryManager.library, {
  UnityPong: function () {
	try {
      window.dispatchUnityEvent("UnityPong");
    } catch (e) {
      console.warn("Failed to dispatch UnityPong");
    }
  },
  JumpSpace: function(id) {
	try {
      window.dispatchUnityEvent("UnityJump", UTF8ToString(id));
    } catch (e) {
      console.warn("Failed to dispatch UnityJump");
      console.warn(e);
    }
  },
  UnityProgress: function (progression) {
	try {
      window.dispatchUnityEvent("UnityProgress", progression);
    } catch (e) {
      console.warn("Failed to dispatch UnityProgress");
    }
  },
  UnityLoaded: function (complete) {
	try {
      window.dispatchUnityEvent("UnityLoaded", complete);
    } catch (e) {
      console.warn("Failed to dispatch UnityLoaded");
    }
  },
  OnAvatarChanged: function (uuid, url) {
	try {
      window.dispatchUnityEvent("OnAvatarChanged", UTF8ToString(uuid), UTF8ToString(url));
    } catch (e) {
      console.warn("Failed to dispatch OnAvatarChanged");
    }
  }
});
