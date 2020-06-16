Option Explicit

Dim sPath
Dim oMetaModel 

'sPath= "C:\temp\ITPM.json"
'Set oMetaModel = getObjectFromId("~TeEKeRMmSPYK[00 - Webservice extract ITPM]")

 sPath= "C:\temp\GDPR.json"
 Set oMetaModel = getObjectFromId("~igEKBLUkSjkG[00 - Webservice extract GDPR Data breach]")

' sPath= "C:\temp\GRCIncident.json"
' Set oMetaModel = getObjectFromId("~NfEKpIDmSDpH[00 - Webservice extract GRC Incident]")

' oRoot.getCollection

'  set oPMetaClass = oroot.getcollection("~Ffs9P58kg1fC[PMetaClass]").item(oMetaClass)

Dim objFSO
Set objFSO = CreateObject("Scripting.FileSystemObject")

' --- We create a new JSON file ---
Dim File
Set File = objFSO.OpenTextFile(sPath, 2, True)

File.writeLine("{") ' open json

File.writeLine(Chr(9) & Chr(34) & "entities" & Chr(34) &":[") 'open entitites




Dim oColMetaclass 
Set oColMetaclass = oMetaModel.getcollection("~x0000000Cy20[MetaClass]")

Dim oCount
oCount = oColMetaclass.Count()
Dim i

Dim oMetaClass
for i = 1 to oCount 
	Set oMetaClass = oColMetaclass.item(i)


  Dim technincalName
  technincalName = GetTechnicalName(oMetaClass.getProp("~Z20000000D60[Short Name]"))

	Dim oIdAbs
	oIdAbs = oMetaClass.getProp("~310000000D00[Absolute Identifier]")

	Dim sDescription
	sDescription =  oMetaClass.getProp("~f10000000b20[Comment]")

  File.writeLine(Chr(9) & Chr(9) & "{")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & technincalName  & Chr(34) & ",")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & oIdAbs & Chr(34) & ",")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(34) & "description" & Chr(34) & ":" & Chr(34) & sDescription & Chr(34) & ",")


	' --- MetaAttribute ---

  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(34) & "properties" & Chr(34) & ":[")

  Dim sResult
  sResult = getMetaAttribute(oMetaClass,File)

  File.writeLine(Chr(9) & Chr(9) & Chr(9) & "],")

  ' --- MetaAssociationEnd ---

  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(34) & "relationships" & Chr(34) & ":[")

	Dim sResultMAE
	sResultMAE = getMetaAssociationEnd(oMetaModel, oMetaClass,File)	

  File.writeLine(Chr(9) & Chr(9) & Chr(9) & "]")


if (i = 1 and oCount > 1) or not (i=oCount) then
  File.writeLine(Chr(9) & Chr(9) & "},")
Else
  File.writeLine(Chr(9) & Chr(9) & "}")
End If

  Set oMetaClass = Nothing
next

Set oColMetaclass = Nothing
Set oMetaModel  = Nothing

File.writeLine(Chr(9) & "]")  ' entities
File.writeLine("}") 'json close

' --- close the file ---
File.Close
Set File = Nothing

' --- Close the log file ---
'log_file.Close
'Set log_file = Nothing

Set objFSO = Nothing

function getMetaAssociationEnd(oMetaModel, oMetaClass,File)
  Dim sResultMAE 
  sResultMAE =""

	Dim oRoot
	Set oRoot = oMetaModel.getRoot()

	Dim IdAbsMetaClass 
	IdAbsMetaClass = oMetaClass.getProp("~310000000D00[Absolute Identifier]")

  Dim IdAbsMetaModel
	IdAbsMetaModel= oMetaModel.getProp("~310000000D00[Absolute Identifier]")

	Dim sQuery 
	'sQuery = "Select ~R20000000k10{S}[MetaAssociationEnd] Where ~i0000000C020{P}[MetaOppositeClass].~310000000D00{A}[_idAbs] = " & Chr(34) & IdAbsMetaClass & Chr(34) & " And ~uhCGT34OxO30{P}[System Diagram].~iq6zWgoiC9F4{P}[Described Element]:~W20000000220{S}[_MetaModel].~310000000D00{A}[_idAbs] =" & Chr(34) & IdAbsMetaModel & Chr(34) & " "

  sQuery = "" _
  & "Select ~R20000000k10{S}[MetaAssociationEnd] Into @MEA "_ 
  & " Where ~i0000000C020{P}[MetaOppositeClass].~310000000D00{A}[_idAbs]   = " & Chr(34) & IdAbsMetaClass & Chr(34) & " " _
  & " And ~uhCGT34OxO30{P}[System Diagram].~iq6zWgoiC9F4{P}[Described Element]:~W20000000220{S}[_MetaModel].~310000000D00{A}[_idAbs]  = " & Chr(34) & IdAbsMetaModel & Chr(34) & " " _
  & " Select ~P20000000c10{S}[MetaClass] Into @AbsMC Where ~I10000000910{A}[_AbstractionLevel] = '5' " _
  & " And ~ahCGT34Ox820{P}[SystemDiagram].~iq6zWgoiC9F4{P}[Described Element]:~W20000000220{S}[_MetaModel].~310000000D00{A}[_idAbs]  = " & Chr(34) & IdAbsMetaModel & Chr(34) & " " _
  & " And ~B10000008C40{P}[SubMetaClass].~310000000D00{A}[_idAbs] = " & Chr(34) & IdAbsMetaClass & Chr(34) & " " _
  & " Select ~R20000000k10{S}[MetaAssociationEnd] Into @AbsMEA " _
  & " Where  ~uhCGT34OxO30{P}[System Diagram].~iq6zWgoiC9F4{P}[Described Element]:~W20000000220{S}[_MetaModel].~310000000D00{A}[_idAbs]  = " & Chr(34) & IdAbsMetaModel & Chr(34) & "" _
  & " And ~i0000000C020{P}[MetaOppositeClass] In @AbsMC " _
  & " Select ~R20000000k10{S}[MetaAssociationEnd] From @MEA Or @AbsMEA "


	Dim oColMetaAssociationEnd
	oColMetaAssociationEnd = oRoot.getSelection(sQuery)

  Dim i
  Dim iCount
  icount = oColMetaAssociationEnd.count()

  Dim oMEA
  for i = 1 to icount 
    oMEA = oColMetaAssociationEnd.item(i)

  	Dim IdAbsMEA
  	IdAbsMEA = oMEA.getProp("~310000000D00[Absolute Identifier]")

		Dim oMetaClassTarget
		oMetaClassTarget = oMEA.getCollection("~j0000000C420[MetaClass]").item(1)

		' if target MetaClass is Abstract, we look for concrete and we loop if multiple


  	Dim technincalName
  	Dim technincalNameTarget

		dim k
		dim oColConcrete
    oColConcrete = oMetaClassTarget.getCollection("~B10000008C40[SubMetaClass]")

    Dim kCount 
		kCount = oColConcrete.count

		Dim oConcreteMetaclass
		for k = 0 to kCount

			If k=0 and kCount = 0 then 
			' we are in the case that it's a concrete already

      	technincalName = GetTechnicalName(oMEA.getProp("~Z20000000D60[Short Name]"))   
      	technincalNameTarget= oMetaClassTarget.getProp("~310000000D00[Absolute Identifier]")
      
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{")
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & technincalName & Chr(34) & ",")
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & IdAbsMEA & Chr(34) & ",")  
       
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "targetMetaClassID" & Chr(34) & ":" & Chr(34) & technincalNameTarget & Chr(34) & "")  
       
        if not (i=icount ) then
       		File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "},")
        Else
       		File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}")
        End If

			Elseif (k=0 and kCount > 0) then
      	' we do nothing as we need to loop on the concrete based on the collection
			Else
				oConcreteMetaclass  = oColConcrete.item(k)
      	technincalName = GetTechnicalName(oMEA.getProp("~Z20000000D60[Short Name]")) & GetTechnicalName(oConcreteMetaclass.getProp("~Z20000000D60[Short Name]"))     
      	technincalNameTarget= oConcreteMetaclass.getProp("~310000000D00[Absolute Identifier]")

        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{")
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & technincalName & Chr(34) & ",")
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & IdAbsMEA & Chr(34) & ",")  
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "targetMetaClassID" & Chr(34) & ":" & Chr(34) & technincalNameTarget & Chr(34) & "")  
       
        if (k=kcount and i=icount) then
       		File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}")
        Else
       		File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "},")
        End If

			End if

      Set oConcreteMetaclass = Nothing
		next

  next

 
  Set oColMetaAssociationEnd  = Nothing
  Set oRoot = Nothing
	getMetaAssociationEnd = sResultMAE 
end function




Function getMetaAttribute(oMetaClass, File)
  Dim oJsonMetaAttribute

' to do : ajouter commentaire

  Dim oColMetaAttribute
  oColMetaAttribute = cleanCollectionMetaAttribute(oMetaClass.getCollection("~k0000000C820[MetaAttribute]"))
 
  Dim oCount
  oCount = oColMetaAttribute.Count()

  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & "HexaIdAbs" & Chr(34) & ",")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & "H20000000550" & Chr(34) & ",")  
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "type" & Chr(34) & ":" & Chr(34) & "String" & Chr(34) & "")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "},")

  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & "ShortName" & Chr(34) & ",")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & "Z20000000D60" & Chr(34) & ",")  
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "type" & Chr(34) & ":" & Chr(34) & "String" & Chr(34) & "")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "},")

  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & "Comment" & Chr(34) & ",")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & "f10000000b20" & Chr(34) & ",")  
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "type" & Chr(34) & ":" & Chr(34) & "String" & Chr(34) & "")

  If oCount > 0 then
    File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "},")
  Else
    File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}")
  End if

  
  Dim i 
	Dim oMetaAttribute
  For i = 1 to oCount
  	oMetaAttribute = oColMetaAttribute.item(i)

    Dim technincalName
    technincalName = GetTechnicalName(oMetaAttribute.getProp("~Z20000000D60[Short Name]"))

  	Dim sIdAbs
	  sIdAbs = oMetaAttribute.getProp("~310000000D00[Absolute Identifier]")

	  Dim sDescription
	  sDescription = oMetaAttribute.getProp("~f10000000b20[Comment]")

    Dim sType
    sType =  GetGraphQLType(oMetaAttribute.getProp("~Q10000000f10[MetaAttribute Type]","internal"))

		Dim isEnum
	  isEnum = hasMetaAttributeValue(oMEtaAttribute)
    File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{")
    File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & technincalName & Chr(34) & ",")
    File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & sIdAbs & Chr(34) & ",")  
    File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "description" & Chr(34) & ":" & Chr(34) & sDescription & Chr(34) & ",")  
		If isEnum = 0 Then
    	File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "type" & Chr(34) & ":" & Chr(34) & sType & Chr(34) & "")
		Else
    	File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "type" & Chr(34) & ":" & Chr(34) & sType & Chr(34) & ",")
		End if  
  	
    Dim useless
    useless = manageMetaAttributeValue(oMetaAttribute,isEnum ,File)
  

    if not (i=oCount) then
      File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "},")
    Else
      File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}")
    End If
  


    Set oMetaAttribute = Nothing
  Next

Set oColMetaAttribute = Nothing            

End Function

Function manageMetaAttributeValue(oMetaAttribute,isEnum , File)

	if not isEnum =0 then
      Dim enumValues
			enumValues = ""
	
      Dim oColMAV
      oColMAV = oMEtaAttribute.getCollection("~(0000000C830[MetaAttributeValue]")

      Dim iCountMAV
			iCountMAV = oColMAV.count()

			Dim j
			Dim oMEtaAttributeValue
      for j= 1 to iCountMAV
				oMEtaAttributeValue = oColMAV.item(j)
				Dim sNameInternalValue
				sNameInternalValue = GetTechnicalNameMetaAttribute(oMEtaAttributeValue.getProp("~H3l5fU1F3n80[Value Name]"))


				if (iCountMAV = 1) or (j=1) then
        	enumValues = Chr(34) & sNameInternalValue & Chr(34)
				Else
        	enumValues = enumValues & "," & Chr(34) & sNameInternalValue & Chr(34)
				end if

      next


    	File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "enumValues" & Chr(34) & ":[" & enumValues & "]")
	end if

  manageMetaAttributeValue = ""

End function

Function hasMetaAttributeValue(oMEtaAttribute) 
  Dim isEnum 

	Dim sFormat
  sFormat=  oMetaAttribute.getProp("~S10000000n10[MetaAttribute Format]","internal")

  Select Case sFormat
    Case "F"
      isEnum = true
    Case "T"
      isEnum = true
    Case Else
      ' we do nothing
			isEnum = false
	End Select
  hasMetaAttributeValue= isEnum 
End function

Function GetGraphQLType(oMetaAttributeType)
  Dim sResult
	sResult = ""

	Select case oMetaAttributeType
		Case "X" ' String
      sResult="String"
		Case "1" ' Boolean
      sResult="Boolean"
		Case "S" ' Short
      sResult="Int"
		Case "L" ' Long
      sResult="Long"
		Case "D" ' DateTime
      sResult="Date"
		Case "A" ' VarChar
      sResult="String"  ' To check
		Case "B" ' VarBinary
      sResult="Binary"  ' To check
		Case "Q" ' Binary
      sResult="Binary"
		Case "H" ' MegaIdentifier
      sResult="String"
		Case "F" ' Float
      sResult="Float"
		Case "C" ' Currency
      sResult="Double" ' To check
		Case "W" ' DateTime64
      sResult="Date"  ' To check
		Case "U" ' AbsoluteDateTime64
      sResult="Date"
		Case Else
      sResult="String"
  End select

	GetGraphQLType = sResult
End function





' this function is used to remove from the metaclass the attribute that we don't want
' this is hard coded
Function cleanCollectionMetaAttribute(oCol)
  Dim oColResult
	Dim oColIntermediate
	Dim oRoot

	Set oRoot = oCol.getRoot()
  Set oColResult = oRoot.getSelection("")

  oColResult.insert(oCol)

   oColResult.remove(oRoot.getObjectFromId("~g20000000f60[Generic Local name]"))
   oColResult.remove(oRoot.getObjectFromId("~nOU8g8IMCb30[Converted Name Version]"))


   oColResult.remove(oRoot.getObjectFromId("~f20000000b60[Log]"))
   oColResult.remove(oRoot.getObjectFromId("~m20000000170[Comment Body]"))
   oColResult.remove(oRoot.getObjectFromId("~f10000000b20[Comment]"))
'   oColResult.remove(oRoot.getObjectFromId(""))
'   oColResult.remove(oRoot.getObjectFromId(""))
'   oColResult.remove(oRoot.getObjectFromId(""))
'   oColResult.remove(oRoot.getObjectFromId(""))
'   oColResult.remove(oRoot.getObjectFromId(""))

  ' --- add here ---
  ' aditionnal attribute you want to avoid

  cleanCollectionMetaAttribute = oColResult

End function


Function GetTechnicalNameMetaAttribute(strVal)
	strVal = Trim(strVal)
    Do While InStr(1, strVal, "\")
        strVal = Replace(strVal, "\", "")
    Loop
    Do While InStr(1, strVal, "/")
        strVal = Replace(strVal, "/", "")
    Loop
    Do While InStr(1, strVal, "'")
        strVal = Replace(strVal, "'", "")
    Loop
    Do While InStr(1, strVal, "" & Chr(34) & "")
        strVal = Replace(strVal, "" & Chr(34) & "", "")
    Loop
    GetTechnicalNameMetaAttribute = strVal 
End Function

Function GetTechnicalName(strVal)
		strVal = Trim(strVal)
    Do While InStr(1, strVal, " ")
        strVal = Replace(strVal, " ", "")
    Loop
    Do While InStr(1, strVal, "-")
        strVal = Replace(strVal, "-", "")
    Loop
    Do While InStr(1, strVal, "\")
        strVal = Replace(strVal, "\", "")
    Loop
    Do While InStr(1, strVal, "/")
        strVal = Replace(strVal, "/", "")
    Loop
    Do While InStr(1, strVal, "(")
        strVal = Replace(strVal, "(", "")
    Loop
    Do While InStr(1, strVal, ")")
        strVal = Replace(strVal, ")", "")
    Loop
    Do While InStr(1, strVal, "'")
        strVal = Replace(strVal, "'", "")
    Loop
    Do While InStr(1, strVal, "?")
        strVal = Replace(strVal, "?", "")
    Loop
    Do While InStr(1, strVal, "&")
        strVal = Replace(strVal, "&", "")
    Loop
    Do While InStr(1, strVal, "[")
        strVal = Replace(strVal, "[", "")
    Loop
    Do While InStr(1, strVal, "]")
        strVal = Replace(strVal, "]", "")
    Loop
    Do While InStr(1, strVal, "{")
        strVal = Replace(strVal, "{", "")
    Loop
    Do While InStr(1, strVal, "}")
        strVal = Replace(strVal, "}", "")
    Loop
    Do While InStr(1, strVal, "<")
        strVal = Replace(strVal, "<", "")
    Loop
    Do While InStr(1, strVal, ">")
        strVal = Replace(strVal, ">", "")
    Loop
    Do While InStr(1, strVal, "_")
        strVal = Replace(strVal, "_", "")
    Loop
    Do While InStr(1, strVal, "=")
        strVal = Replace(strVal, "=", "")
    Loop
	Do While InStr(1, strVal, "+")
        strVal = Replace(strVal, "+", "")
    Loop
    Do While InStr(1, strVal, "" & Chr(34) & "")
        strVal = Replace(strVal, "" & Chr(34) & "", "")
    Loop
    Loop
    Do While InStr(1, strVal, "GDPR")
        strVal = Replace(strVal, "GDPR", "")
    Loop

    GetTechnicalName = strVal 
End Function


Function GetFile(ByVal FileName)
	Dim FS: Set FS = CreateObject("Scripting.FileSystemObject")
	'Go To windows folder If full path Not specified.
	If InStr(FileName, ":\") = 0 And Left (FileName,2)<>"\\" Then 
		FileName = FS.GetSpecialFolder(0) & "\" & FileName
	End If
	On Error Resume Next

	GetFile = FS.OpenTextFile(FileName).ReadAll
End Function

Function WriteFile(ByVal FileName, ByVal Contents)  
	Dim FS: Set FS = CreateObject("Scripting.FileSystemObject")
	'On Error Resume Next

	'Go To windows folder If full path Not specified.
	If InStr(FileName, ":\") = 0 And Left (FileName,2)<>"\\" Then 
		FileName = FS.GetSpecialFolder(0) & "\" & FileName
	End If

	Dim OutStream: Set OutStream = FS.OpenTextFile(FileName, 2, True)
	OutStream.Write Contents
End Function