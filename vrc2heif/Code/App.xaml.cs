﻿using Newtonsoft.Json;

namespace vrc2heif;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            window.Title = "VRC2HEIF";
        }

        return window;
    }
}
