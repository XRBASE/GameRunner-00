# v0.3.1
- minor patches in spawn point, so that it does not teleport the player when starting a minigame
- Information points now integrated into the minigame system, so that text and video can also be shown as a minigame.

# v0.3.0
Networked minigame state and player progression. Networked minigames now activate for all players, but the minigame itself is still single player.
Minigames now all have their own minimum and maximum score. Score is tracked as integer now and not as float anymore.

# v0.2.2
info point bugfix (exit button added). Stop call of video player now causes a black screen as to provide more feedback.
mute button of video player now also sets the volume slider.

# v0.2.1
Added minigame timer that finished the minigame if a timelimit is reached. Adds different texts to
the points UI that are displayed based on what caused the minigame to finish. Removed superclass MiniGame,
as name was too similar to the Minigame class. Functions of removed class have been moved up into Minigame class.

# v0.2.0
added information points, simple video player, wordle and quiz minigame,
networking now contains it's own networking loop. minigames can now be exited through button.

Basic game functionality has been implemented into a first design. Information has been added,
so data can be given, which can be tested through minigames.
 - Video support added, so KPN videos can be shown.

Networking has been connected to it's own service loop, so the usage of fixed update can be separated
and used for physics updates.

# v0.1.5
Loading manager that is handled to update and disable the server's loading screen.
Adds a new way of using the token, to match the new workflow that is applied to
the webserver.

# v0.1.4
Added a time scaled lerp/ framerate independet lerp to the mathbuddy collection.

# v0.1.3
Found old namespaces still called ravel, which have been renamed to cohort. Also adds an Initialized boolean in the player instances
that indicates whether the photon data has been set yet.

# v0.1.3
Avatar importer using RPM for now. Supports functionality for prefab and GLB importing. Animation system, not hooked up yet, for 
managing avatar animations.

# v0.1.2
Adds generic player management that handles network subscriptions and interface classes that can be used for initial
player management.

# v0.1.1
removed spaces and connected service and repo classes. Adds Photon service and repo class to manage connection behaviour.
adds string id for channels which will in the future be used instead of space id's to retrieve asset data from.

# v0.1.0 Linnorm

## Summary
First POC, of the server API framework.
Logs in user and connects to photon based on ServerHandle data. 
Single scene workflow, that contains right webGL build settings and nothing apart from Server and photon code (and related dependencies).

# New version todo:
- Create new version number based on changes:
	- https://semver.org/
- Create codeword (Generate random DnD monster name).
	- https://perchance.org/m9vg0idjpv
- Write summary based on what changed this version.
- if required, write bugfixes list.