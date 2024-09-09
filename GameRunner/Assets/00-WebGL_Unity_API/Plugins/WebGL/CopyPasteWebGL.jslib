var CopyPastePlugin =
{
  ReadFromClipBoard: function(gObj, vName)
  {
      var gameObjectName = UTF8ToString(gObj);
      var voidName = UTF8ToString(vName);
      function getCpText( e )
      {
          e.preventDefault();
          var pastedText = undefined;
          if( window.clipboardData && window.clipboardData.getData )
          {
              pastedText = window.clipboardData.getData('Text');
          }
          else if( e.clipboardData && e.clipboardData.getData )
          {
              pastedText = e.clipboardData.getData('text/plain');
          }
          unityInstance.SendMessage(gameObjectName, voidName, pastedText);
      }
      if( navigator.userAgent.indexOf("Firefox") != -1 )
      {
          document.onpaste = getCpText;
      }
      else if( navigator.userAgent.indexOf("Edge") != -1 )
      {
          unityInstance.SendMessage(gameObjectName, OnError, "Paste: this browser is not supported.");
      }
      else if( navigator.userAgent.indexOf("Chrome") != -1 || navigator.userAgent.indexOf("Opera") != -1 )
      {
          navigator.clipboard.readText().then(function(data) {
              unityInstance.SendMessage(gameObjectName, voidName, data);
          }, function() {
              unityInstance.SendMessage(gameObjectName, OnError, "Paste: no text aviable in clipboard");
          })
      }
      else if( navigator.userAgent.indexOf("Safari") != -1 )
      {
          unityInstance.SendMessage(gameObjectName, OnError, "Paste: this browser is not supported.");
      }
      else
      {
          unityInstance.SendMessage(gameObjectName, OnError, "Paste: this browser is not supported.");
      }
  },
  SetToClipboard: function(value)
  {
      if(navigator.userAgent.indexOf("Firefox") != -1 )
      {
          navigator.clipboard.writeText(UTF8ToString(value));
      }
      else if( navigator.userAgent.indexOf("Edge") != -1 )
      {
          unityInstance.SendMessage(gameObjectName, OnError, "Copy: this browser is not supported.");
      }
      else if( navigator.userAgent.indexOf("Chrome") != -1 || navigator.userAgent.indexOf("Opera") != -1 )
      {
          navigator.clipboard.writeText(UTF8ToString(value));
      }
      else if( navigator.userAgent.indexOf("Safari") != -1 )
      {
          unityInstance.SendMessage(gameObjectName, OnError, "Copy: this browser is not supported.");
      }
      else
      {
          unityInstance.SendMessage(gameObjectName, OnError, "Copy: this browser is not supported.");
      }
  }
};
mergeInto(LibraryManager.library, CopyPastePlugin);