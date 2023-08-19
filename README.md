Dynamic Mission Generator service for _Keep Talking and Nobody Explodes_.

This service relies on [Mod Selector](https://steamcommunity.com/sharedfiles/filedetails/?id=801400247) being installed, as all interactions are done through that.

# Usage

To start a mission, enter a mission string into the text box, then press Enter or click the Run button. A mission string consists of a combination of the following tokens, separated by spaces.

* `[number]*[module ID]` – adds the specified number of the specified type of module. For compatibility, `;` may be used in place of `*`. The number and `*` may be omitted to add a single module. If the module ID contains spaces, enclose it in quotation marks. If you specify multiple module IDs separated with `,` or `+`, modules will be selected at random from those types. Whitespace may be present around the `*` or after a `,`. Prefix this token with an `!` to ensure that there are the fewest number of duplicate modules. The following special tokens may be used in place of a module ID list:
  * `ALL_SOLVABLE` – any regular module
  * `ALL_NEEDY` – any needy module
  * `ALL_VANILLA` – any vanilla regular module
  * `ALL_MODS` – any mod regular module
  * `ALL_VANILLA_NEEDY` – any vanilla needy module
  * `ALL_MODS_NEEDY` – any mod needy module
  * `profile:[name]` – any regular module enabled by the specified profile (and not disabled by other profiles)
  * `needyprofile:[name]` – any needy module enabled by the specified profile (and not disabled by other profiles)
* `[h]:[m]:[s]` – sets the starting bomb time. The hours may be omitted. The default is 2 minutes per regular module.
* `[number]X` – sets the strike limit. The default is 1 per 12 regular modules with a minimum of 3.
* `needyactivationtime:[seconds]` – sets the time before all needy modules activate. The default is 90 seconds.
* `widgets:[number]` – sets the number of random widgets. The default is 5.
* `frontonly` – forces all modules to be on the face with the timer where possible.
* `nopacing` – disables pacing events for the mission.
* `room:[name]` - Selects a particular defusal room. Multiple rooms may be separated with commas. If multiple rooms are listed, then a random room will be selected from that list.
* `factory:[mode]` – sets the [Factory](https://steamcommunity.com/sharedfiles/filedetails/?id=1307301431) mode for the mission. Valid modes are `static`, `finite`, `finitegtime`, `finitegstrikes`, `finitegtimestrikes`, `infinite`, `infinitegtime`, `infinitegstrikes`, `infinitegtimestrikes`.
* `mode:[mode]` - Sets the mode for the mission. Valid modes are `normal`, `time`, `zen`, and `steady`.
* You can modify mode settings for Time, Zen, and Steady modes. Keywords for the settings are preceeded by `tm_`, `zm_`, or `sm_`.



To specify [multiple bombs](https://steamcommunity.com/sharedfiles/filedetails/?id=806104225), enclose each bomb description in parentheses. You can repeat a bomb configuration by providing `[number]*` before the `(`. Prefix this token with an `!` to ensure that there are the fewest number of duplicate modules on the bomb. You can also specify bomb properties (time, strikes, needy activation time, widget count, front only) outside parentheses to apply them to all following bombs. 
Example: `5:00 2*(3*ALL_SOLVABLE) 3:00 (1X 3*ALL_SOLVABLE)`

Anything following `//` is considered a comment and ignored. Anything between `/*` and `*/` is also comment but can span multiple lines.

Starting to type certain tokens will cause a list of potential completions to appear. You can quickly insert one of these options by clicking it or pressing Tab.

To get around spam filters on places like Twitch, ! can be used instead of : for all tokens apart from bomb time.

# Modkit support for DMG

The [community fork of the KTANE modkit](https://github.com/Qkrisi/ktanemodkit) supports loading and editing missions using DMG syntax. If you don't know how to do use the modkit, refer to [the official modkit wiki](https://github.com/keeptalkinggame/ktanemodkit/wiki), specifically [1. Getting Started](https://github.com/keeptalkinggame/ktanemodkit/wiki/1.-Getting-Started) and [4. Custom Missions](https://github.com/keeptalkinggame/ktanemodkit/wiki/4.-Custom-Missions)

All features above are supported, with the exception of `profile:[profile]`, `room:[name]`, `mode:[mode]`, `tm/zm/sm` settings, and `!` (duplication reduction).

In addition, documentation comments ("doc comments") are also supported to specify mission names and descriptions within the DMG string. There are variants of doc comments using either inline or multiline comments:

    //// Mission name (start line with four '/'s)
    /// Description line 1 (start each description line with three '/'s)
    /// Description line 2 
    
    /** (start comment with two '*'s)
      Mission name
      Description line 1
      Description line 2
    */
    
In both cases, whitespace is ignored.

## Creating a new mission using a DMG mission file (such as one saved using DMG in-game)

Under the `Keep Talking ModKit` menu, click `Missions/Load DMG Misison File`. This will create a mission in the `Assets/Missions` directory with a name matching the mission name, if specified in the doc comments.

![Click Keep Talking ModKit/Missions/Load DMG Mission File](https://cdn.discordapp.com/attachments/694963465005170690/845519953922621460/unknown.png)

## Editing mission using DMG syntax / getting DMG version of a mission

As seen in the screenshot below, there is now a DMG mission string box in the mission editor panel. The text area will automatically update with the current version of the DMG mission. If it does not update, use the `Force Refresh DMG Mission String` button to forcefully refresh it.

You can edit the DMG mission string in this text area. Clicking `Load from DMG Mission String` will update the mission asset to match the DMG specficiation.

![Inspector panel showing DMG Mission String editor with Refresh and Load buttons](https://media.discordapp.net/attachments/286995174607814656/845897452577226782/unknown.png?width=354&height=559)

## Loading a DMG mission pack 

The modkit supports loading an entire pack of missions specified in DMG syntax into mission and table of contents assets. To do so, begin by verifing that your "pack" directory looks like the following:

    /pack_name/section_1_name/mission1.txt
    /pack_name/section_1_name/mission2.txt
    /pack_name/section_2_name/mission3.txt
    /pack_name/section_3_name/mission4.txt
    /pack_name/section_3_name/mission5.txt
    /pack_name/section_3_name/mission6.txt
    
In other words, the "pack" directory should contain subfolders, each containing a series of mission files. 

Section names are determined from the name of the section folders. Mission names are determined by the mission names in the mission doc strings. By default, sections and missions are ordered alphabetically. To override this behaivour, you can optionally specify numbers in the file names to order sections and missions:

    /pack_name/2. section_1_name/1. mission1.txt
    /pack_name/2. section_1_name/2. mission2.txt
    /pack_name/3. section_2_name/mission3.txt
    /pack_name/1. section_3_name/5. mission4.txt
    /pack_name/1. section_3_name/20. mission5.txt
    /pack_name/1. section_3_name/10. mission6.txt
    
This will result in a table of contents like this:

    pack_name
      1. section_3_name
         1.1 mission 4 name
         1.2 mission 6 name
         1.3 mission 5 name
      2. section_1_name
         2.1 mission 1 name
         2.2 mission 2 name
      3. section_2_name
         3.1 mission 3 name
         
Under the `Keep Talking ModKit` menu, click `Missions/Load DMG Mission Folder`. This will create a mission pack in the `Assets/Missions` directory matching the above.

![Click Keep Talking ModKit/Missions/Load DMG Mission Folder](https://cdn.discordapp.com/attachments/694963465005170690/845519953922621460/unknown.png)
