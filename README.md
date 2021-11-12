# FindMySteamDLC

## A .NET WPF application designed to fetch the Steam games on your computer and to show you all the DLCs available for this game.

![capture](./img/capture.png)

### My main goal working on this app is to train myself making some good looking UI and to be honest the app itself is not really useful. Nonetheless there is some interesting things happening in the code.

<br/>

### **• Fetching games informations from the computer**

This wasn't really hard to do as there are .acf files for each games installed (*Steam\steamapps*) which contains a lot of informations about them. I made a [little method](https://github.com/DrKrusto/FindMySteamDLC/blob/7e1106069c23767501cc00bcd203fcf203bbec69/FindMySteamDLC/src/Handlers/SteamInfo.cs#L67) which parse those informations from this file. The structure looks like JSON but doesn't have the ':' between the value and the key so I had to make my own parser.

<br/>

### **• Fetching DLCs informations**

We can find all of the DLCs's AppIDs installed on the computer in the .acf files, but we can't have the precise name of the DLC nor any infos of non installed DLCs. There is three possibilities to get these informations:
- Steam libraries: Maybe the most efficient way to fetch these informations. The thing is that I neither could't understand how it worked nor it couldn't do why I wanted.
- Web APIs: There is multiple APIs which fetch the informations we need like [SteamSPY](https://steamspy.com/) or the [storefront API of Steam](https://wiki.teamfortress.com/wiki/User:RJackson/StorefrontAPI) and returns them in a JSON file that can be easily used after. I used both previously and sometime I would have a major issue: the database is not updated/doesn't have all the informations of Steam.
- Web scraping: Not the most efficient way to retrieve the informations but the one I'm using right now. Go to the DLC page of a game (ex: [Fallout: New Vegas](https://store.steampowered.com/dlc/22380/Fallout_New_Vegas/)) and add 'ajaxgetfilteredrecommendations' to the URL. It gives a JSON file with the HTML result of the page (using the page URL directly without the ajax parameter may give you false results as there is multiple categories for the DLCs). Then use a HTML parser to get the information you want and voilà, you have the exact DLC information you wanted.

<br/>

### **• Fetching images**

In the Steam directory (*Steam\appcache\librarycache*) you can find every image that Steam is using in the application. For the DLCs found on internet [you can download the image directly from Steam](https://github.com/DrKrusto/FindMySteamDLC/blob/7e1106069c23767501cc00bcd203fcf203bbec69/FindMySteamDLC/src/Handlers/SteamInfo.cs#L191).

<br/>

### **• ADO.NET/EntityFramework**

The thing is with downloading, scraping and parsing HTML, JSON and ACF files is that it can get really slow during the loading of the application. I decided to use a database to avoid these slow operations on each use of the app. The app is using ADO.NET and SQLite for the database and the file is steaminfo.sqlite. I tried in another branch to change ADO.NET to EntityFrameworkCore but I didn't like the performance I got from the change so for now I'm staying with ADO.NET.