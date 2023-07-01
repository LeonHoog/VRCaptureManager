﻿namespace vrcapturemanager;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            window.Title = "VR Capture Manager";
        }
        window.MinimumWidth = 500;
        window.MinimumHeight = 500;

        return window;
    }
}
