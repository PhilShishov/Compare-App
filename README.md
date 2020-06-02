# CompareApp
C#, XAML, WPF

Main function: Compare XML & XSD files -> Search for differences -> Add them to missing -> Save

## Build status

| Compare App |
|:--------------|
[![Build status](https://ci.appveyor.com/api/projects/status/bep7tqi7o78ahrba?svg=true)](https://ci.appveyor.com/project/PhilShishov/compare-app)

 <img src="https://image.ibb.co/gLds89/compareapp.png" width="900" height="450">

## Using 

.Net Framework 4.5.2
1. Load one XSD and one XML file. Click on Compare to search for the differences
2. Open a random red marked dropdown
3. Right-click on a node and choose : 
- Add all missing nodes to XML
- Add a chosen node to XML
- Add a chosen node and its children to XML 
4. Save XML to a new or already existing XML

## MVVM pattern and Prism framework
- Dependency Injection
- DelegateCommands
- SuppressUnmanagedCodeSecurity - control unmanaged code
