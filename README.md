Dynamic Mission Generator service for _Keep Talking and Nobody Explodes_.

This service relies on [Mod Selector](https://steamcommunity.com/sharedfiles/filedetails/?id=801400247) being installed, as all interactions are done through that.

# Usage

To start a mission, enter a mission string into the text box, then press Enter or click the Run button. A mission string consists of a combination of the following tokens, separated by spaces.

* `[number]*[module ID]` – adds the specified number of the specified type of module. For compatibility, `;` may be used in place of `*`. The number and `*` may be omitted to add a single module. If the module ID contains spaces, enclose it in quotation marks. If you specify multiple module IDs separated with `,` or `+`, modules will be selected at random from those types. Whitespace may be present around the `*` or after a `,`. The following special tokens may be used in place of a module ID list:
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
* `factory:[mode]` – sets the [Factory](https://steamcommunity.com/sharedfiles/filedetails/?id=1307301431) mode for the mission. Valid modes are `static`, `finite`, `finitegtime`, `finitegstrikes`, `finitegtimestrikes`, `infinite`, `infinitegtime`, `infinitegstrikes`, `infinitegtimestrikes`

To specify [multiple bombs](https://steamcommunity.com/sharedfiles/filedetails/?id=806104225), enclose each bomb description in parentheses. You can repeat a bomb configuration by providing `[number]*` before the `(`, and you can also specify bomb properties (time, strikes, needy activation time, widget count, front only) outside parentheses to apply them to all following bombs. Example: `5:00 2*(3*ALL_SOLVABLE) 3:00 (1X 3*ALL_SOLVABLE)`

Starting to type certain tokens will cause a list of potential completions to appear. You can quickly insert one of these options by clicking it or pressing Tab.
