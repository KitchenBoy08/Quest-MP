﻿using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;

using MelonLoader;

namespace LabFusion.Preferences.Server;

public class ServerSettings
{
    // General settings
    public IFusionPref<bool> NametagsEnabled;
    public IFusionPref<bool> VoiceChatEnabled;
    public IFusionPref<bool> PlayerConstraintsEnabled;
    public IFusionPref<ServerPrivacy> Privacy;
    public IFusionPref<TimeScaleMode> TimeScaleMode;
    public IFusionPref<int> MaxPlayers;

    // Visual
    public IFusionPref<string> ServerName;
    public IFusionPref<string> ServerDescription;
    public IFusionPref<List<string>> ServerTags;

    // Mortality
    public IFusionPref<bool> ServerMortality;

    // Permissions
    public IFusionPref<PermissionLevel> DevToolsAllowed;
    public IFusionPref<PermissionLevel> ConstrainerAllowed;
    public IFusionPref<PermissionLevel> CustomAvatarsAllowed;
    public IFusionPref<PermissionLevel> KickingAllowed;
    public IFusionPref<PermissionLevel> BanningAllowed;

    public IFusionPref<PermissionLevel> Teleportation;

    public static ServerSettings CreateMelonPrefs(MelonPreferences_Category category)
    {
        // Server settings
        var settings = new ServerSettings
        {
            // General settings
            NametagsEnabled = new FusionPref<bool>(category, "Server Nametags Enabled", true, PrefUpdateMode.SERVER_UPDATE),
            VoiceChatEnabled = new FusionPref<bool>(category, "Server Voicechat Enabled", true, PrefUpdateMode.SERVER_UPDATE),
            PlayerConstraintsEnabled = new FusionPref<bool>(category, "Server Player Constraints Enabled", false, PrefUpdateMode.SERVER_UPDATE),
            Privacy = new FusionPref<ServerPrivacy>(category, "Server Privacy", ServerPrivacy.PUBLIC, PrefUpdateMode.LOCAL_UPDATE),
            TimeScaleMode = new FusionPref<TimeScaleMode>(category, "Time Scale Mode", Senders.TimeScaleMode.LOW_GRAVITY, PrefUpdateMode.SERVER_UPDATE),
            MaxPlayers = new FusionPref<int>(category, "Max Players", 10, PrefUpdateMode.SERVER_UPDATE),

            // Visual
            ServerName = new FusionPref<string>(category, "Server Name", string.Empty, PrefUpdateMode.SERVER_UPDATE),
            ServerDescription = new FusionPref<string>(category, "Server Description", string.Empty, PrefUpdateMode.SERVER_UPDATE),
            ServerTags = new FusionPref<List<string>>(category, "Server Tags", new List<string>(), PrefUpdateMode.LOCAL_UPDATE),

            // Mortality
            ServerMortality = new FusionPref<bool>(category, "Server Mortality", true, PrefUpdateMode.SERVER_UPDATE),

            // Server permissions
            DevToolsAllowed = new FusionPref<PermissionLevel>(category, "Dev Tools Allowed", PermissionLevel.DEFAULT, PrefUpdateMode.SERVER_UPDATE),
            ConstrainerAllowed = new FusionPref<PermissionLevel>(category, "Constrainer Allowed", PermissionLevel.DEFAULT, PrefUpdateMode.SERVER_UPDATE),
            CustomAvatarsAllowed = new FusionPref<PermissionLevel>(category, "Custom Avatars Allowed", PermissionLevel.DEFAULT, PrefUpdateMode.SERVER_UPDATE),
            KickingAllowed = new FusionPref<PermissionLevel>(category, "Kicking Allowed", PermissionLevel.OPERATOR, PrefUpdateMode.SERVER_UPDATE),
            BanningAllowed = new FusionPref<PermissionLevel>(category, "Banning Allowed", PermissionLevel.OPERATOR, PrefUpdateMode.SERVER_UPDATE),

            Teleportation = new FusionPref<PermissionLevel>(category, "Teleportation", PermissionLevel.OPERATOR, PrefUpdateMode.SERVER_UPDATE),
        };

        return settings;
    }
}