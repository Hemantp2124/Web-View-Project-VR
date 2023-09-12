using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuplex.WebView;

public class ButtonScript : MonoBehaviour
{
    CanvasWebViewPrefab mainWebViewPrefab;
    CanvasWebViewPrefab controlsWebViewPrefab;
    // Start is called before the first frame update
    void Start()
    {
        mainWebViewPrefab = GameObject.Find("CanvasWebViewPrefab").GetComponent<CanvasWebViewPrefab>();
        controlsWebViewPrefab = GetComponent<CanvasWebViewPrefab>();

        controlsWebViewPrefab.WebView.MessageEmitted += Controls_MessageEmitted;
        controlsWebViewPrefab.WebView.LoadHtml(CONTROLS_HTML);

    }

    async void _refreshBackForwardState()
    {

        // Get the main webview's back / forward state and then post a message
        // to the controls UI to update its buttons' state.
        var canGoBack = await mainWebViewPrefab.WebView.CanGoBack();
        var canGoForward = await mainWebViewPrefab.WebView.CanGoForward();
        var serializedMessage = $"{{ \"type\": \"SET_BUTTONS\", \"canGoBack\": {canGoBack.ToString().ToLowerInvariant()}, \"canGoForward\": {canGoForward.ToString().ToLowerInvariant()} }}";
        controlsWebViewPrefab.WebView.PostMessage(serializedMessage);
    }

    void Controls_MessageEmitted(object sender, EventArgs<string> eventArgs)
    {

        if (eventArgs.Value == "CONTROLS_INITIALIZED")
        {
            // The controls UI won't be initialized in time to receive the first UrlChanged event,
            // so explicitly set the initial URL after the controls UI indicates it's ready.
            _setDisplayedUrl(mainWebViewPrefab.WebView.Url);
            return;
        }
        var message = eventArgs.Value;
        if (message == "GO_BACK")
        {
            mainWebViewPrefab.WebView.GoBack();
        }
        else if (message == "GO_FORWARD")
        {
            mainWebViewPrefab.WebView.GoForward();
        }
    }

    void _setDisplayedUrl(string url)
    {

        if (controlsWebViewPrefab.WebView != null)
        {
            var serializedMessage = $"{{ \"type\": \"SET_URL\", \"url\": \"{url}\" }}";
            controlsWebViewPrefab.WebView.PostMessage(serializedMessage);
        }
    }

    const string CONTROLS_HTML = @"
            <!DOCTYPE html>
            <html>
                <head>
                    <!-- This transparent meta tag instructs 3D WebView to allow the page to be transparent. -->
                    <meta name='transparent' content='true'>
                    <meta charset='UTF-8'>
                    <style>
                        body {
                            font-family: Helvetica, Arial, Sans-Serif;
                            margin: 0;
                            height: 100vh;
                            color: white;
                        }
                        .controls {
                            display: flex;
                            justify-content: space-between;
                            align-items: center;
                            height: 100%;
                        }
                        .controls > div {
                            background-color: #283237;
                            border-radius: 8px;
                            height: 100%;
                        }
                        .url-display {
                            flex: 0 0 75%;
                            width: 75%;
                            display: flex;
                            align-items: center;
                            overflow: hidden;
                            cursor: default;
                        }
                        #url {
                            width: 100%;
                            white-space: nowrap;
                            overflow: hidden;
                            text-overflow: ellipsis;
                            padding: 0 15px;
                            font-size: 18px;
                        }
                        .buttons {
                            flex: 0 0 20%;
                            width: 20%;
                            display: flex;
                            justify-content: space-around;
                            align-items: center;
                        }
                        .buttons > button {
                            font-size: 40px;
                            background: none;
                            border: none;
                            outline: none;
                            color: white;
                            margin: 0;
                            padding: 0;
                        }
                        .buttons > button:disabled {
                            color: rgba(255, 255, 255, 0.3);
                        }
                        .buttons > button:last-child {
                            transform: scaleX(-1);
                        }
                        /* For Gecko only, set the background color
                        to black so that the shader's cutout rect
                        can translate the black pixels to transparent.*/
                        @supports (-moz-appearance:none) {
                            body {
                                background-color: black;
                            }
                        }
                    </style>
                </head>
                <body>
                    <div class='controls'>
                        <div class='url-display'>
                            <div id='url'></div>
                        </div>
                        <div class='buttons'>
                            <button id='back-button' disabled='true' onclick='vuplex.postMessage(""GO_BACK"")'>?</button>
                            <button id='forward-button' disabled='true' onclick='vuplex.postMessage(""GO_FORWARD"")'>?</button>
                        </div>
                    </div>
                    <script>
                        // Handle messages sent from C#
                        function handleMessage(message) {
                            var data = JSON.parse(message.data);
                            if (data.type === 'SET_URL') {
                                document.getElementById('url').innerText = data.url;
                            } else if (data.type === 'SET_BUTTONS') {
                                document.getElementById('back-button').disabled = !data.canGoBack;
                                document.getElementById('forward-button').disabled = !data.canGoForward;
                            }
                        }

                        function attachMessageListener() {
                            window.vuplex.addEventListener('message', handleMessage);
                            window.vuplex.postMessage('CONTROLS_INITIALIZED');
                        }

                        if (window.vuplex) {
                            attachMessageListener();
                        } else {
                            window.addEventListener('vuplexready', attachMessageListener);
                        }
                    </script>
                </body>
            </html>
        ";
}
