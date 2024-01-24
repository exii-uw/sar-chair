using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class WebBrowsing : MonoBehaviour, IDemo
{
    private int prevScrollNum = 0;
    public int scrollNum = 0;
    public int zoomAmount = 150;
    private readonly int increment = 500;
    private SimpleWebBrowser.WebBrowser browser;
    DemoManager demoManager;
    private string url;

    // Start is called before the first frame update
    void Start()
    {
        browser = gameObject.transform.Find("InWorldBrowser").GetComponent<SimpleWebBrowser.WebBrowser>();
        demoManager = gameObject.transform.parent.GetComponent<DemoManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (prevScrollNum != scrollNum)
        {
            int multiplier = 1; // negative to scroll up
            if (scrollNum < prevScrollNum)
            {
                multiplier = -1;
            }

            prevScrollNum = scrollNum;
            int scrollAmount = (multiplier * increment);

            // scroll
            browser._mainEngine.SendExecuteJSEvent("window.scrollBy(0," + scrollAmount + ");");
        }

        // zoom
        browser._mainEngine.SendExecuteJSEvent("document.body.style.zoom=" + (zoomAmount / 100.0) + ";this.blur();");

        if (demoManager.currentDemo == DemoManager.Demo.Twenty)
            MoveContent();

    }

    public void ScrollDown()
    {
        scrollNum++;
    }

    public void ScrollUp()
    {
        scrollNum--;
    }

    public void MoveContent()
    {
        if (demoManager.currentDemo == DemoManager.Demo.Twenty)
        {
            string basePath = "C:/Users/nvjoshi/Downloads/";
            DirectoryInfo dirInfo = new DirectoryInfo(basePath);
            FileInfo[] textFiles = dirInfo.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();
            
            foreach(FileInfo file in textFiles)
            {
                if (file.Extension.Equals(".txt"))
                {
                    string text = File.ReadAllText(file.FullName);
                    if (text.Equals(url) || !file.Name.StartsWith("recent"))
                        return;
                    print("Navigating to " + text);
                    url = text;
                    browser._mainEngine.SendNavigateEvent(text, false, false);
                    return;
                }
            }

        }
    }

    void IDemo.TriggerPlay()
    {
        throw new System.NotImplementedException();
    }

    void IDemo.TriggerPause()
    {
        throw new System.NotImplementedException();
    }

    void IDemo.TriggerNext()
    {
        ScrollDown();
    }

    void IDemo.TriggerBack()
    {
        ScrollUp();
    }
}
