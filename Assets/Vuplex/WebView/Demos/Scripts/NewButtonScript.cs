using System;
using UnityEngine;
using Vuplex.WebView;
using UnityEngine.UI;

public class NewButtonScript : MonoBehaviour
{
    public CanvasWebViewPrefab mainWebViewPrefab;
    public InputField url;
    public void WebGoBack()
    {
        mainWebViewPrefab.WebView.GoBack();
    }
    public void WebGoForword()
    {
        mainWebViewPrefab.WebView.GoForward();
    }
    public void Search()
    {
        try
        {
            Debug.Log("input text:" + url.text);
            mainWebViewPrefab.WebView.LoadUrl(url.text);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            string searchQuery = "https://www.google.com/search?q=" + url.text;
            mainWebViewPrefab.WebView.LoadUrl(searchQuery);
        }
    }
}
