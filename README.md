# Cleaning Robots

This is a Caves of Qud mod that adds a cleaner creature to the robots faction that spends time mopping up spills. For the full description, see the [Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=3077616001).

Cleaning Robots is licensed under the [GNU General Public License v3](http://www.gnu.org/licenses/agpl.html), which can be found in full in [LICENSE.md](LICENSE.md).

## Credits

This mod was almost entirely self-made, but **A Blessed Feline** from the Caves of Qud Discord server provided the sprite used for the cleaner. Thank you!

## Changelog

### 9 December, 2024
#### Version 1.1.1
* Retooled the way that cleaners determine who is allowed to configure or deactivate them. Cleaners that are doing their own thing (villagers, Mechanimists, etc.) cannot be configured, and neither can cleaners that are hostile to you or following another creature. Any others will accept being configured or deactivated.
* Deactivating a cleaner now causes everything in its inventory to drop onto the ground instead of being deleted outright. Can't say they weren't doing their job well, though.
* Dormant cleaners now have gray highlights, to visually differentiate them from active cleaners.

### 23 September, 2024
#### Version 1.1
* Updated to work with game version 2.0.207.101.
* Updated the author listing and mod ID to use my current preferred name.
* Added scrapped cleaners. 3 of them are guaranteed to appear in Golgotha, and they can be repaired into dormant cleaners. They're also guaranteed to be dropped on death by cleaners themselves.
* Added dormant cleaners. You can get them through tinkering (use Psychometry or find a data disk) or by repairing a scrapped cleaner. Dormant cleaners can be deployed to create a neutral cleaner anywhere you want! It won't have any special allegiance to you, though.
* Non-hostile cleaners can now be deactivated from their context menu to turn them into dormant cleaners. The reprogrammed cleaner in Grit Gate is an exception, for obvious reasons.
* Cleaners no longer spawn lacquered.
* Cleaners are now always neutral, regardless of your robot reputation. They'll still fight back if you attack them, or attack another robot near them.
* Polished the description for cleaners.
* (internal) Re-pathed all instances of `Ava` to `Ceres`.

### 22 June, 2024
#### Version 1.0.3
* Updated to work with game version 2.0.207.72, which featured breaking API changes.
* Fixed a bug where cleaners wouldn't mop up pure liquids, even if enabled to.

### 6 June, 2024
#### Version 1.0.2
* Updated to work with the Spring Molting patch (2.0.207.63).

### 14 November, 2023
#### Version 1.0.1
* The reprogrammed cleaner in Grit Gate will default to the "fearful" behavior during A Call to Arms.

### 9 November, 2023
* Initial release.
