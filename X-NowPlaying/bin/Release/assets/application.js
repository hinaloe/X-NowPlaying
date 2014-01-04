//
// Reference : http://denspe.blog84.fc2.com/blog-entry-123.html
//

var shell = WScript.CreateObject("WScript.Shell");

var DBPath = "";

//Get MtData.mdb location;
try {
    DBPath = shell.RegRead("HKLM\\SOFTWARE\\Sony Corporation\\Sony MediaPlayerX\\Database\\MetallicData");
} catch (Exception) {
    DBPath = "";
}

if(!DBPath) {
    try {
        DBPath = shell.RegRead("HKLM\\SOFTWARE\\Wow6432Node\\Sony Corporation\\Sony MediaPlayerX\\Database\\MetallicData");
    } catch (Exception)
    {
        WScript.Quit(0);
    }
}

var Adodb = null;
try {
    Adodb = WScript.CreateObject("ADODB.Connection");
} catch (Exception)
{
    WScript.Quit(0);
}

//Load DataBase (Access 2007 ~)
try {
    Adodb.Open("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DBPath + ";");
} catch (Exception) {
    try {
        //Load DataBase (~ Access 2003)
        Adodb.Open("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + DBPath + ";");
    }
    catch (Exception) {
        //Load DataBase (DAO)
        Adodb.Open("Driver={Microsoft Access Driver (*.mdb)};DBQ=" + DBPath + ";");
    }
}

if(!Adodb.State) {
    WScript.Quit(0);
}

// Main
var sql = "select * from t_object where ObjectSpecID=2;";
var set = null;
try {
    set = Adodb.Execute(sql);
} catch (Exception) {
    WScript.Quit(0);
}

if(set) {
    var fso = WScript.CreateObject("Scripting.FileSystemObject");
    var file = fso.CreateTextFile("xapplication.db", true, true);

    //file.WriteLine("#Song Title,Artist Name,Album Name,Music File,Jacket File");

    while(set && !set.EOF) {
        var line = "";

        //Title
        if(set.Fields("ObjectName").Value) {
            line += "\"" + replaceAll(set.Fields("ObjectName").Value, "\"", "%22") + "\",";
        } else {
            line += "\"\",";
        }

        //Artist
        if(set.Fields("201").Value) {
            line += "\"" + replaceAll(set.Fields("201").Value, "\"", "%22") + "\",";
        } else {
            line += "\"\",";
        }

        //Album
        if(set.Fields("206").Value) {
            line += "\"" + replaceAll(set.Fields("206").Value, "\"", "%22") + "\",";
        } else {
            line += "\"\",";
        }

        //Music
        if(set.Fields("500").Value) {
            line += "\"" + set.Fields("500").Value + "\",";
        } else {
            line += "\"\",";
        }

        //Jacket
        if(set.Fields("202").Value) {
            line += "\"" + set.Fields("202").Value + "\"";
        } else {
            line += "\"\"";
        }

        file.WriteLine(line);
        set.MoveNext();
    }

    file.Close();
    file = null;
}

set.Close();
set = null;
Adodb.Close();
Adodb = null;

WScript.Quit(0);


function replaceAll(expression, org, dest) {
    return expression.split(org).join(dest);
}