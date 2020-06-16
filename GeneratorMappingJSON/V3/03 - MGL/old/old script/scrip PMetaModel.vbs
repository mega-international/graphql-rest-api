oRoot = GetRoot
oCol = oRoot.GetCollection("~Ffs9P58kg1fC[PMetaClass]")

for each oObj in oCol

print oObj.getProp("Name")

next


'Dim colMetaClass
'colMetaClass=mgobjObject.GetRoot().GetCollection("~Ffs9P58kg1fC[PMetaClass]")
'Dim oMetaClass
'For Each oMetaClass in colMetaClass
' If oMetaClass.IsAvailable And Not oMetaClass.SameId("~Ffs9P58kg1fC[PMetaClass]") And Not oMetaClass.SameId("~hefM6bncz000[Collection]") And Not oMetaClass.SameId("~d20000000U20[_Collection]") Then
'  If oMetaClass.GetProp("~x7HQkCdX5L10[Abstraction]")=0  And oMetaClass.GetProp("~Hgs9P58)e1fC[Technical Level]")<> "K"  then
'      mgcolCollection.insert oMetaClass 
'  End If
'End If
'Next