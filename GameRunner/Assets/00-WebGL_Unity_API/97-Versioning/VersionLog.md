# v0.2.0
added information points, simple video player, wordle and quiz minigame,
networking now contains it's own networking loop. minigames can now be exited through button.

## Summary
Basic game functionality has been implemented into a first design. Information has been added,
so data can be given, which can be tested through minigames.
 - Video support added, so KPN videos can be shown.

Networking has been connected to it's own service loop, so the usage of fixed update can be separated
and used for physics updates.

# v0.1.5
Added loading management for controling server UI. Adds updated token flow.

## Summary
Loading manager that is handled to update and disable the server's loading screen.
Adds a new way of using the token, to match the new workflow that is applied to
the webserver.

# v0.1.4
Time scaled lerp

## Summary
Added a time scaled lerp/ framerate independet lerp to the mathbuddy collection.

# v0.1.3
namespace renames

## Summary
Found old namespaces still called ravel, which have been renamed to cohort. Also adds an Initialized boolean in the player instances
that indicates whether the photon data has been set yet.

# v0.1.3
Avatars and animations

## Summary
Avatar importer using RPM for now. Supports functionality for prefab and GLB importing. Animation system, not hooked up yet, for 
managing avatar animations.

# v0.1.2
Player Management

## Summary
Adds generic player management that handles network subscriptions and interface classes that can be used for initial
player management.

# v0.1.1
patch space removal

## Summary
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