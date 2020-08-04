# FilterPlugin
XRM Plugin to allow for formatting name of filter lookup result.  

# How To Use It:

# Example Config:
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
