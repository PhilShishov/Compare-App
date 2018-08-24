# CompareApp
C#, XAML, WPF

Main function: Compare XML & XSD files -> Search for differences -> Add them to missing -> Save

 <img src="https://image.ibb.co/gLds89/compareapp.png" width="900" height="450">

## MVVM pattern and Prism framework
- Dependency Injection
- DelegateCommands
- SuppressUnmanagedCodeSecurity - control unmanaged code

## Using 

.Net Framework 4.5.2
1. Load one XSD and one XML file. Click on Compare to search for the differences
2. Open a random red marked dropdown
3. Right-click on a node and choose : 
- Add all missing nodes to XML
- Add a chosen node to XML
- Add a chosen node and its children to XML 
4. Save XML file ot a new or already existing file
