Publish Extensions for Visual Studio 2013/2015
========================

This extension allows to publish selected files from the solution explorer or to publish the active file in the editor. It comes in handy when working on static files like CSS or JavaScript files, without the need to re-publish the whole project to see the changes.


Features
---
- Auto-publish: files and folders are published automatically on save/add/rename/move/build
- Publish selected files or folders to local or network folders
- Exclude single files or folders in the solution explorer
- Exclude files or folders by a project wide filter (i.e. '*.cs')
- Specify mappings that files or folders are renamed during publishing
- Quickly publish the active document in the editor by using the context menu or pressing Ctrl-Shift-P
- Separate configuration for each project
- Configure a different publish location for each developer of the project
- Configure several publish locations (comma-separated list)


Notes
---

**Auto-publish on remove is partially supported**  
Auto-publish of removed files and empty folders is supported. Auto-publish of removed folders with files is not supported.

**Publishing of assemblies**  
In order to publish assemblies include the bin folder into a project.