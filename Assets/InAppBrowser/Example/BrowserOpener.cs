﻿using UnityEngine;
using System.Collections;

public class BrowserOpener : MonoBehaviour {

	public static BrowserOpener instance;
	public string pageToOpen = "https://www.google.com";

	// check readme file to find out how to change title, colors etc.
	public void OnButtonClicked() {
		InAppBrowser.DisplayOptions options = new InAppBrowser.DisplayOptions();
		options.displayURLAsPageTitle = false;
		options.pageTitle = "InAppBrowser example";
        options.hidesTopBar = true;

        InAppBrowser.OpenURL(pageToOpen, options);
	}

	public void OnClearCacheClicked() {
		InAppBrowser.ClearCache();
	}

    private void Start()
    {
		instance = this;
    }
}
