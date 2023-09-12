using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuplex.WebView;

public class Educational : MonoBehaviour
{
    public CanvasWebViewPrefab browser;
    private string url;
    public InputField addressBar;

    // Start is called before the first frame update
    public void MathSimulation()
    {
       
        url = "https://excalidraw.com/#room=2a54305a5da7ed080b61,E8mgaCP3Hn8pMyZBLTd1zQ";
        addressBar.text = url;
        browser.WebView.LoadUrl(url);
    }

    public void PhysicsSimulation()
    {
        url = "https://cloud13.de/testwhiteboard/?whiteboardid=myNewWhiteboard";
        addressBar.text = url;
        browser.WebView.LoadUrl(url);
    }

    public void ChemistrySimulation()
    {
        url = "https://phet.colorado.edu/sims/html/build-a-nucleus/latest/build-a-nucleus_all.html";
        addressBar.text = url;
        browser.WebView.LoadUrl(url);
    }

    public void BiologySimulation()
    {
        url = "https://phet.colorado.edu/sims/html/natural-selection/latest/natural-selection_all.html";
        addressBar.text = url;
        browser.WebView.LoadUrl(url);
    }
}
