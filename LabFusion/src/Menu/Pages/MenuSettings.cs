﻿using Il2CppSLZ.Marrow.SceneStreaming;

using LabFusion.Data;
using LabFusion.Downloading.ModIO;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Preferences.Client;
using LabFusion.Representation;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using LabFusion.Voice;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuSettings
{
    public static void PopulateSettings(GameObject settingsPage)
    {
        var rootPage = settingsPage.transform.Find("scrollRect_Options/Viewport/Content").GetComponent<PageElement>();

        // Base Fusion Settings
        var clientPage = rootPage.AddPage();
        PopulateClientSettings(clientPage);

        var downloadingPage = rootPage.AddPage();
        PopulateDownloadingSettings(downloadingPage);

#if DEBUG
        var debugPage = rootPage.AddPage();

        PopulateDebugSettings(debugPage);
#endif

        // Network Layer Settings
        var networkLayerPage = rootPage.AddPage();
        foreach (var networkLayer in NetworkLayer.Layers)
        {
            FusionLogger.Log($"Created settings for {networkLayer.Title}");
            var networkLayerGroup = networkLayerPage.AddElement<GroupElement>(networkLayer.Title);
            // If exception is thrown, then settings arent implemented, and a category should not be added
            try
            {
                networkLayer.OnCreateSettings(networkLayerGroup);
            }
            catch (Exception ex)
            {
                networkLayerPage.RemoveElement(networkLayerGroup);

                if (ex is NotImplementedException)
                {
#if DEBUG
                    FusionLogger.Log($"Layer {networkLayer.Title} has no settings to implement.");
#endif
                }
                else
                {
                    FusionLogger.LogException($"creating {networkLayer.Title} settings", ex);
                }
            }
        }

        // Module Settings
        var modulePage = rootPage.AddPage();
        foreach (var module in ModuleManager.Modules)
        {
            var moduleGroup = modulePage.AddElement<GroupElement>(module.Name);
            // If exception is thrown, then settings arent implemented, and a category should not be added
            try
            {
                module.OnCreateSettings(moduleGroup);
            }
            catch (Exception ex)
            {
                modulePage.RemoveElement(moduleGroup);

                if (ex is NotImplementedException)
                {
#if DEBUG
                    FusionLogger.Log($"Layer {module.Name} has no settings to implement.");
#endif
                }
                else
                {
                    FusionLogger.LogException($"creating {module.Name} settings", ex);
                }
            }
        }

        // Categories
        var categoriesRoot = settingsPage.transform.Find("scrollRect_Categories/Viewport/Content").GetComponent<PageElement>();
        var categoriesPage = categoriesRoot.AddPage("Default");

        categoriesPage.AddElement<FunctionElement>("Client").Link(clientPage).WithColor(Color.white);
        categoriesPage.AddElement<FunctionElement>("Downloading").Link(downloadingPage).WithColor(Color.cyan);
        categoriesPage.AddElement<FunctionElement>("Network Layers").Link(networkLayerPage).WithColor(Color.white);
        categoriesPage.AddElement<FunctionElement>("Modules").Link(modulePage).WithColor(Color.white);

#if DEBUG
        categoriesPage.AddElement<FunctionElement>("Debug").Link(debugPage).WithColor(Color.red);
#endif
    }

    private static void PopulateClientSettings(PageElement page)
    {
        // Visual
        var visualGroup = page.AddElement<GroupElement>("Visuals");

        visualGroup.AddElement<FloatElement>("Menu Size")
            .AsPref(ClientSettings.MenuSize)
            .WithIncrement(0.1f)
            .WithLimits(1f, 2f);

        visualGroup.AddElement<BoolElement>("NameTags")
            .AsPref(ClientSettings.NameTags);

        visualGroup.AddElement<EnumElement>("Nickname Visibility")
            .AsPref(ClientSettings.NicknameVisibility);

        visualGroup.AddElement<BoolElement>("Mute Icon")
            .AsPref(ClientSettings.VoiceChat.MutedIndicator);

        // NameTag color
        var nameTagColorPref = ClientSettings.NameTagColor;

        var nameTagColorGroup = page.AddElement<GroupElement>("NameTag Color")
            .WithColor(nameTagColorPref);

        var hueElement = nameTagColorGroup.AddElement<FloatElement>("Hue")
            .WithIncrement(0.05f)
            .WithLimits(0f, 1f)
            .AsPref(ClientSettings.NameTagHue, OnColorElementChanged);

        var saturationElement = nameTagColorGroup.AddElement<FloatElement>("Saturation")
            .WithIncrement(0.05f)
            .WithLimits(0f, 1f)
            .AsPref(ClientSettings.NameTagSaturation, OnColorElementChanged); ;

        var valueElement = nameTagColorGroup.AddElement<FloatElement>("Value")
            .WithIncrement(0.05f)
            .WithLimits(0f, 1f)
            .AsPref(ClientSettings.NameTagValue, OnColorElementChanged); ;

        void OnColorElementChanged(float value)
        {
            nameTagColorGroup.Color = ClientSettings.NameTagColor;
        }

        // Voice Chat
        var voiceChatGroup = page.AddElement<GroupElement>("Voice Chat");

        voiceChatGroup.AddElement<FloatElement>("Global Volume")
            .AsPref(ClientSettings.VoiceChat.GlobalVolume)
            .WithLimits(0f, 3f)
            .WithIncrement(0.1f);

        var inputDeviceGroup = page.AddElement<GroupElement>("Input Device");

        PopulateInputDeviceGroup(inputDeviceGroup);
    }

    private static void PopulateInputDeviceGroup(GroupElement element)
    {
        var devices = VoiceInfo.InputDevices;

        var inputPreference = ClientSettings.VoiceChat.InputDevice;

        Dictionary<string, FunctionElement> deviceElements = new();

        var defaultButton = element.AddElement<FunctionElement>("Default")
            .WithColor(string.IsNullOrEmpty(inputPreference.Value) ? Color.green : Color.gray)
            .Do(() =>
            {
                inputPreference.Value = string.Empty;
            });

        deviceElements.Add(string.Empty, defaultButton);

        foreach (var device in devices)
        {
            var color = inputPreference.Value == device ? Color.green : Color.gray;

            var deviceButton = element.AddElement<FunctionElement>(device)
                .WithColor(color)
                .Do(() =>
                {
                    inputPreference.Value = device;
                });

            deviceElements.Add(device, deviceButton);
        }

        inputPreference.OnValueChanged += OnPrefChanged;

        element.OnCleared += () =>
        {
            inputPreference.OnValueChanged -= OnPrefChanged;
        };

        void OnPrefChanged(string value)
        {
            foreach (var element in deviceElements)
            {
                if (string.IsNullOrWhiteSpace(value) && string.IsNullOrEmpty(element.Key))
                {
                    element.Value.Color = Color.green;
                    continue;
                }

                if (value == element.Key)
                {
                    element.Value.Color = Color.green;
                    continue;
                }

                element.Value.Color = Color.gray;
            }
        }
    }

    private static void PopulateDownloadingSettings(PageElement page)
    {
        var generalGroup = page.AddElement<GroupElement>("General");

        generalGroup.AddElement<BoolElement>("Download Spawnables")
            .AsPref(ClientSettings.Downloading.DownloadSpawnables);

        generalGroup.AddElement<BoolElement>("Download Avatars")
            .AsPref(ClientSettings.Downloading.DownloadAvatars);

        generalGroup.AddElement<BoolElement>("Download Levels")
            .AsPref(ClientSettings.Downloading.DownloadLevels);

        generalGroup.AddElement<BoolElement>("Keep Downloaded Mods")
            .AsPref(ClientSettings.Downloading.KeepDownloadedMods);

        generalGroup.AddElement<IntElement>("Max File Size (MB)")
            .AsPref(ClientSettings.Downloading.MaxFileSize)
            .WithIncrement(10)
            .WithLimits(0, 10000);

        generalGroup.AddElement<IntElement>("Max Level Size (MB)")
            .AsPref(ClientSettings.Downloading.MaxLevelSize)
            .WithIncrement(100)
            .WithLimits(0, 10000);

        generalGroup.AddElement<BoolElement>("Download Mature Content")
            .AsPref(ClientSettings.Downloading.DownloadMatureContent)
            .WithColor(Color.red);

        var quickDownloads = page.AddElement<GroupElement>("Quick Downloads");

        quickDownloads.AddElement<FunctionElement>("Download Cosmetics")
            .Do(() =>
            {
                ModIODownloader.EnqueueDownload(new ModTransaction()
                {
                    ModFile = new ModIOFile(ModReferences.FusionCosmeticsId),
                    Callback = (info) =>
                    {
                        if (info.result != Downloading.ModResult.SUCCEEDED)
                        {
                            FusionNotifier.Send(new FusionNotification()
                            {
                                Title = "Download Failed",
                                Message = "Failed to download Fusion Cosmetics!",
                                SaveToMenu = false,
                                ShowPopup = true,
                                Type = NotificationType.ERROR,
                            });
                        }
                    },
                });
            });
    }

#if DEBUG
    private static void PopulateDebugSettings(PageElement page)
    {
        var generalGroup = page.AddElement<GroupElement>("General");

        generalGroup.AddElement<FunctionElement>("Load Testing Level")
            .Do(() =>
            {
                SceneStreamer.Load(FusionLevelReferences.FusionTestingReference.Barcode, null);
            });

        generalGroup.AddElement<FunctionElement>("Spawn Player Rep")
            .Do(() =>
            {
                PlayerRepUtilities.CreateNewRig((rig) =>
                {
                    rig.transform.position = RigData.Refs.RigManager.physicsRig.feet.transform.position;
                });
            });

        generalGroup.AddElement<FunctionElement>("Send To Floating Point")
            .Do(() =>
            {
                var physRig = RigData.Refs.RigManager.physicsRig;

                float force = 100000000000000f;

                for (var i = 0; i < 10; i++)
                {
                    physRig.rbFeet.AddForce(Vector3Extensions.left * force, ForceMode.VelocityChange);
                    physRig.rbKnee.AddForce(Vector3Extensions.right * force, ForceMode.VelocityChange);
                    physRig.rightHand.rb.AddForce(Vector3Extensions.up * force, ForceMode.VelocityChange);
                    physRig.leftHand.rb.AddForce(Vector3Extensions.down * force, ForceMode.VelocityChange);
                }
            });
    }
#endif
}