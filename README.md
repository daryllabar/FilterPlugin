
# FilterPlugin
Plugin to allow for formatting name of filter lookup result.  
So rather than this where the names are exact and you can't differentiate:
![Unformatted](https://raw.githubusercontent.com/daryllabar/FilterPlugin/master/.github/images/UnformattedLookup.png)

you can add additional columns to be able to distinguish the difference between records:
![Formatted](https://raw.githubusercontent.com/daryllabar/FilterPlugin/master/.github/images/FormattedLookup.png)

For a full walk through video of what it is, and how to install it, see the [video](https://youtu.be/BkOLeoMkW_8)!

# Install
  - Download the plugin assembly (DLaB.Xrm.Filter.Plugins.dll) from the latest release on the [releases](/releases) page.
  - Register the plugin to your org using the plugin registration tool
  - Register two "RetrieveMultiple" plugin steps, Pre-Operation and Post-Operation for the entity(-ies) that you want to update the name format of.
    - Paste JSON config in the Unsecure Configuration for both steps (or alternatively, leave it blank which will fall back to using the Lookup View for the entity).
  - That's it.  Go and test the formatting!

# Plugin Config
## Lookup View Columns
\* Note - The recommended approach is to define JSON that is specificed in the pugin step registrations \* If no values are entered for both the Secure and Unsecure configuration of the plugin registration, the plugin will default to using the first 3 columns defined in the LookupView for the entity.

## Config Options

|Name|Type|
|--|--|
|attributes| Array of attribute.|
|attributes[].attribute| (Required) The name of attribute to be used in the format string for the display name. |
|attributes[].maxLength| (Optional) The maximum length of the string.  If the attribute is longer than the max length, it will be truncated.  If no maxLength is provided, the string will not be truncated. |
|attributes[].prefix| (Optional) A prefix to include if the attribute has a value (i.e. contains at least one character that is not white space)|
|format| The format of the display name.  This format uses [composite based formatting options allowed by the .net string.Format method](https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting). |
|nameAttribute|The name of the display name of the entity|

\* Please note: All names must be the logical (lower case) name of the attributes

## Example Config
The following example config 
``` JSON
{
    "attributes":[
        {
            "attribute":"fullname",
            "maxLength":40
        }, {
            "attribute":"parentcustomerid",
            "maxLength":30,
            "prefix":" - "
        }, {
            "attribute":"address1_stateorprovince",
            "prefix":", "
        }],
    "format":"{0}{1}{2}",
    "nameAttribute":"fullname"
}
```
Given an entity with the name, account, and state of "Bob Smith", "Acme", "Alaska" respectively, the display name in the lookup would be "Bob Smith - Acme, Alaska"
