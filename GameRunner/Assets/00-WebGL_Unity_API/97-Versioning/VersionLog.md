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

# v0.1.0
codeword: Linnorm,
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