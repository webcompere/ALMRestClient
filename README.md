# ALMRestClient
A .NET implementation of a basic client that can work with HP ALM SAAS's REST interface. In C# with .NET 4.5

Please read https://codingcraftsman.wordpress.com/2014/04/11/hp-alm-give-it-a-rest/ for more info

Depends on restsharp via NuGet.

Please note, this comes without any warranty or support. It has worked for me in the past with HP ALM. If you wish to raise issues within this repo, I'll try to answer them on a best efforts basis. If you wish to collaborate on the code, then please clone and offer a pull request.

Basic usage:

    ALMClient client = new ALMClient(almUrl, almUser, almPassword, domain, project);
    client.Login();
    
Once logged in you can use:

    client.GetDefects()
    
This will provide a list of defects as ALMItem objects

You can change an item using

    client.UpdateDefect(defectId, itemWithChangesIn);
    
The ALMItem for itemWithChangesIn has a dictionary in it, into which you can make your changes. You can either modify the one you got from GetDefects, or create a new one and just set the changes in it.

ALMItem has a few getters/setters for common fields. There are other fields.

To setup the test project, you will need to set a breakpoint within the test and run the following within the immediate window to set the user settings:

    new ALMClientTests.ALMClientTestConfigure().SetUpSettings("<username>","<password>","<https://HPALM Address:Port/>")
    
