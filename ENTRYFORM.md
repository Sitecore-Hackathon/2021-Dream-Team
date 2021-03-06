# Hackathon Submission Entry form

## Team name
DreamTeam

## Category
The best enhancement to SXA

## Description
This module will help to make more flexible and allow to be more configurable of `SxA Themes`. What does it mean: SxA has powerful module called `Theme` where each `Site`  can have own set of styles, scripts, fonts, images etc. As well, this module includes some `Base Theme` as a common set of front-end stuff across all `Sites`.
The most paintful problem which every business try to resolve is performance optimization. Using LighHouse as part of Google Page Speed Ingight business faced with some recommendations from this tool, and one of recommendations, is to reduce amount of unused .js and .css files, make them smaller, keep only styles and js code related to the components on your page. It's really had to do with current `Theme` stuff in `SxA`. 
Specifically for this reason we have 3 enhancment in `SxA Theme` module which will mitigate a problems described above.
As we are know, if not then we can type in Google something like this `critical css sxa sitecore` and get a lot of recoomendations to improve this situation. Here the options which we can enhance with `SxA Theme`:
1. __Remove unused css and js code from CD env.__
⋅⋅*Typically when we rendering a page on CD env. all js and css from `Base Themes` as well loading on a page. With our module will be possible to specify what should be included from `Base Theme` into a live page witnin `AssetLinksGenerator` output.
2. __Use critical css for the page__
⋅⋅*Ofcourse, critical css should improve page performance and should be loaded above the page as fast as possible. One problem for the developer to identify css which are critical (it depends from first page elements and require research those critical components over pages), the second one, how to distingues those css entry, since, it is part of `Site Theme` and, of course, this should be separate out `optimized-min` file.
3. __Use css and js which are required by components on a page__
⋅⋅*This is the most hardest part, since, require specific service which will collect base on list of `Renderings` on a page scope of js and css files which should be optimized on fly and embeeded with `AssetLinksGenerator` output.
4. And the last one: of course, __use js, css deffer approach, direct font loading, use cache, optimize media, use CDN etc.__

## Video link
⟹ Provide a video highlighing your Hackathon module submission and provide a link to the video. You can use any video hosting, file share or even upload the video to this repository. _Just remember to update the link below_

⟹ [Replace this Video link](#video-link)



## Pre-requisites and Dependencies

⟹ Does your module rely on other Sitecore modules or frameworks?

- List any dependencies
- Or other modules that must be installed
- Or services that must be enabled/configured

_Remove this subsection if your entry does not have any prerequisites other than Sitecore_

## Installation instructions
⟹ Write a short clear step-wise instruction on how to install your module.  

> _A simple well-described installation process is required to win the Hackathon._  
> Feel free to use any of the following tools/formats as part of the installation:
> - Sitecore Package files
> - Docker image builds
> - Sitecore CLI
> - msbuild
> - npm / yarn
> 
> _Do not use_
> - TDS
> - Unicorn
 
f. ex. 

1. Start docker environment using `.\Start-Hackathon.ps1`
2. Open solution in Visual Studio and run build
3. Use the Sitecore Installation wizard to install the [package](#link-to-package)
4. ...
5. profit

### Configuration
⟹ If there are any custom configuration that has to be set manually then remember to add all details here.

_Remove this subsection if your entry does not require any configuration that is not fully covered in the installation instructions already_

## Usage instructions
⟹ Provide documentation about your module, how do the users use your module, where are things located, what do the icons mean, are there any secret shortcuts etc.

Include screenshots where necessary. You can add images to the `./images` folder and then link to them from your documentation:

![Hackathon Logo](docs/images/hackathon.png?raw=true "Hackathon Logo")

You can embed images of different formats too:

![Deal With It](docs/images/deal-with-it.gif?raw=true "Deal With It")

And you can embed external images too:

![Random](https://thiscatdoesnotexist.com/)

## Comments
If you'd like to make additional comments that is important for your module entry.
