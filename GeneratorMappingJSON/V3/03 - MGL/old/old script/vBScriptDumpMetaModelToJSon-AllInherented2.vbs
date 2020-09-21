Option Explicit

Dim sPath
Dim oMetaModel 

' sPath= "C:\temp\ITPM.json"
' Set oMetaModel = getObjectFromId("~TeEKeRMmSPYK[00 - Webservice extract ITPM]")

' sPath= "C:\temp\Data.json"
' Set oMetaModel = getObjectFromId("~LITwsiH(Szn4[00 - Webservice extract Data]")

 sPath= "C:\temp\GDPR.json"
 Set oMetaModel = getObjectFromId("~igEKBLUkSjkG[00 - Webservice extract GDPR Data breach]")

 sPath= "C:\temp\GRCIncident.json"
 Set oMetaModel = getObjectFromId("~NfEKpIDmSDpH[00 - Webservice extract GRC Incident]")

Dim objFSO
Set objFSO = CreateObject("Scripting.FileSystemObject")

' --- We create a new JSON file ---
Dim File
Set File = objFSO.OpenTextFile(sPath, 2, True)

File.writeLine("{") ' open json

File.writeLine(Chr(34) & "entities" & Chr(34) &":[") 'open entitites


Dim oRoot
Set oRoot = oMetaModel.getRoot()

Dim sQuery
Dim absoluteIdentifierMetamodel

absoluteIdentifierMetamodel = oMetaModel.getProp("~310000000D00[Absolute Identifier]")
sQuery = "Select [MetaClass] Where [MetaLayer] = " & Chr(34) & "30" & Chr(34) & " And [SystemDiagram].[Described Element]:[MetaModel].[Absolute Identifier] = " & Chr(34) & absoluteIdentifierMetamodel & Chr(34) & ""


Dim oColMetaclass 
Set oColMetaclass = oRoot.getSelection(sQuery)

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
	sDescription = cleanComment(oMetaClass.getProp("~f10000000b20[Comment]","Display"))

  Dim sQueryNode  
	sQueryNode = "false"

Dim oObjTemp
oObjTEmp = oMetaModel.getCollection("~x0000000Cy20[MetaClass]").item(oMetaClass)

  if (oObjTEmp.getId() <> 0)  then
		sQueryNode = "true"
	end if

  File.writeLine(Chr(9) & Chr(9) & "{")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & technincalName  & Chr(34) & ",")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & oIdAbs & Chr(34) & ",")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(34) & "description" & Chr(34) & ":" & Chr(34) & sDescription & Chr(34) & ",")
  File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(34) & "queryNode" & Chr(34) & ":" & sQueryNode  & ",")

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

  Dim pMetaClass
	pMetaClass = oRoot.GetCollection("~Ffs9P58kg1fC[PMetaClass]").item(oMetaClass)

  Dim pColMetaAssociationEnd
  pColMetaAssociationEnd = pMetaClass.getCollection("~1fs9P5egg1fC[Description]").item(1).getCollection("Collections")

  Dim i
  Dim iCount
  icount = pColMetaAssociationEnd.count()

  Dim pMEA
  for i = 1 to icount 
    pMEA = pColMetaAssociationEnd.item(i)

    Dim oMea
		oMea = oRoot.getCollection("~R20000000k10[MetaAssociationEnd]").item(pMEA.megaField)

		' we take into account only the meta association end that are on the diagram
		Dim otest
		otest = oMetaModel.getCollection("~hq6zWgoiC5F4[System Description]").item(1).getCollection("~vhCGT34OxS30[MetaAssociationEnd]").item(oMea)
    
    if (otest.getId() <> 0)  then
  
  		Dim oMetaClassTarget
  		oMetaClassTarget = oMEA.getCollection("~j0000000C420[MetaClass]").item(1)
			
			dim isFirst
      isFirst = true
      dim sUseless
      sUseless = subMetaClassConcrete(oMetaModel, oMEA,oMetaClassTarget,isFirst, i, iCount)  
  
    end if
  next

 
  Set pColMetaAssociationEnd  = Nothing
  Set oRoot = Nothing
	getMetaAssociationEnd = sResultMAE 
end function



function subMetaClassConcrete(oMetaModel, oMEA, oMetaClassTarget,isFirst, i, iCount)

			' 2 cases : either oMetaClassTarget is concrete or abstract
			' case concrete then we write the info
			' case abstract : we loop on all sub concrete metaclass

  Dim otest
  otest = oMetaModel.getCollection("~hq6zWgoiC5F4[System Description]").item(1).getCollection("~bhCGT34OxC20[MetaClass]").item(oMetaClassTarget)
  ' only if the sub metaclass is in the diagram we navigate	
  if (otest.getId() <> 0)  then

      dim sMetaLayer
			sMetaLayer = oMetaClassTarget.getProp("~I10000000910[MetaLayer]","internal")
      ' we test the layer

      If (strComp(sMetaLayer ,"30") = 0) then
        ' ################# concrete ################# 
       		Dim technincalNameMEA 
      		Dim technincalNameMetaClass
      		Dim targetMetaClassID
        	Dim IdAbsMEA
					Dim pathToTarget

          Dim isFirstPath 
					isFirstPath  =  True

          pathToTarget = findpathToLastMetaClass(isFirst, isFirstPath, oMEA,oMetaClassTarget,IdAbsMEA,technincalNameMetaClass,technincalNameMEA,targetMetaClassID)

          File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{")
    			File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & technincalNameMEA & Chr(34) & ",")
    			'File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "targetMetaAssociationEndId" & Chr(34) & ":" & Chr(34) & IdAbsMEA & Chr(34) & ",")  
    			'File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "targetMetaClassId" & Chr(34) & ":" & Chr(34) & targetMetaClassID & Chr(34) & ",")  
   				File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "pathToTarget" & Chr(34) & ": [") 
			  		File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & pathToTarget) 
   				File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "]")  

          ' TO DO : enelver la virgule si c'est le dernier
          ' mais comment savoir si c'est le dernier ? il faudrait stocker le resultant dans un tableau et ensuite écrire le json

          'if not (i=icount ) then
         		File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "},")
         ' Else
         '		File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}")
         ' End If

      else
        ' ################# abstract ################# 
      		isFirst = false
      		dim oColSubMetaClass 
          oColSubMetaClass = oMetaClassTarget.getCollection("~B10000008C40[SubMetaClass]")
      
          Dim kCount 
      		kCount = oColSubMetaClass.count
      
      		Dim oSubMetaclass
      		dim k
      		for k = 0 to kCount
            oSubMetaclass = oColSubMetaClass.item(k)
  	          dim sUseless
	            sUseless = subMetaClassConcrete(oMetaModel, oMEA,oSubMetaclass,isFirst ,i, iCount)  
            Set oSubMetaclass= Nothing
      		next
    
          Set oColSubMetaClass  = Nothing
     
      end if ' test concrete/abstract
  end if ' test metaclass on diagram   

      subMetaClassConcrete = ""
end function

Function findpathToLastMetaClass(isFirst, isFirstPath, oMEA,oMetaClassTarget,IdAbsMEA,technincalNameMetaClass,technincalNameMEA,targetMetaClassID)
	Dim stringToSend
	Dim oRoot
	Set oRoot = oMetaClassTarget.getRoot()

	Dim pMetaClassTarget
	pMetaClassTarget = oRoot.GetCollection("~Ffs9P58kg1fC[PMetaClass]").item(oMetaClassTarget)
  
  Dim oFound 
  ' we test here is this is an in between. in between object are inherit from "Feature".
  ' TO DO : how to manage case like "Operation" that is considered in the MetaModel as intermediate but not by the customer
  oFound = pMetaClassTarget.getcollection("~(es9P5ufg1fC[UpperClasses]").item("~jHt39NXIPL2H[Feature]") 


  IdAbsMEA = oMEA.getProp("~310000000D00[Absolute Identifier]")
  technincalNameMetaClass = GetTechnicalName(oMetaClassTarget.getProp("~Z20000000D60[Short Name]"))  
	technincalNameMEA = GetTechnicalName(oMEA.getProp("~Z20000000D60[Short Name]"))   
	targetMetaClassID= oMetaClassTarget.getProp("~310000000D00[Absolute Identifier]")
	if not isFirst then
		technincalNameMEA  = technincalNameMEA  & "_" & technincalNameMetaClass              
	end if

	Dim sMetaMultiplicity 
  sMetaMultiplicity = oMEA.getProp("~h8Rdbvvb0900[MetaMultiplicity]")
  
  if (oFound.getId <> "") then
  	' #### the target is an in between object ####

    stringToSend = "{" &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "maeName" &  Chr(34) & ":" &  Chr(34) & technincalNameMEA &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "maeID" &  Chr(34) & ":" &  Chr(34) & IdAbsMEA  &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "metaClassName" &  Chr(34) & ":" &  Chr(34) & technincalNameMetaClass &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "metaClassID" &  Chr(34) & ":" &  Chr(34) & targetMetaClassID  &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "multiplicity" &  Chr(34) & ":" &  Chr(34) & sMetaMultiplicity &  Chr(34) & "" &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}" &_
","

    Dim isFirstPathFollow  
		isFirstPathFollow  =  false

		' TO DO gérer la récursivité 

    IdAbsMEA=  "TO-DO"
    technincalNameMetaClass =  "TO-DO"
    technincalNameMEA =  "TO-DO"
    targetMetaClassID=  "TO-DO"

    stringToSend = stringToSend &_
"" & Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{" &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "maeName" &  Chr(34) & ":" &  Chr(34) & technincalNameMEA &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "maeID" &  Chr(34) & ":" &  Chr(34) & IdAbsMEA  &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "metaClassName" &  Chr(34) & ":" &  Chr(34) & technincalNameMetaClass &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "metaClassID" &  Chr(34) & ":" &  Chr(34) & targetMetaClassID  &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "multiplicity" &  Chr(34) & ":" &  Chr(34) & sMetaMultiplicity &  Chr(34) & "" &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}"

  else
  	' #### the target is a real metaclass ####
    stringToSend = "{" &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "maeName" &  Chr(34) & ":" &  Chr(34) & technincalNameMEA &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "maeID" &  Chr(34) & ":" &  Chr(34) & IdAbsMEA  &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "metaClassName" &  Chr(34) & ":" &  Chr(34) & technincalNameMetaClass &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "metaClassID" &  Chr(34) & ":" &  Chr(34) & targetMetaClassID  &  Chr(34) & "," &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "multiplicity" &  Chr(34) & ":" &  Chr(34) & sMetaMultiplicity &  Chr(34) & "" &_
"" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}"

  end if

  Set oRoot = Nothing
  findpathToLastMetaClass = stringToSend

end function





Function getMetaAttribute(oMetaClass, File)
  Dim oJsonMetaAttribute

  Dim oRoot
  Set oRoot = oMetaModel.getRoot()

  Dim pMetaClass
	pMetaClass = oRoot.GetCollection("~Ffs9P58kg1fC[PMetaClass]").item(oMetaClass)

  Dim pColMetaAttribute
  pColMetaAttribute = pMetaClass.getCollection("~1fs9P5egg1fC[Description]").item(1).getCollection("MainProperties")

  pColMetaAttribute = cleanCollectionMetaAttribute(pColMetaAttribute)

 
  Dim oCount
  oCount = pColMetaAttribute.Count()

  Dim i 
	Dim pMetaAttribute
  For i = 1 to oCount
  	pMetaAttribute = pColMetaAttribute.item(i)

		Dim oMetaAttribute
		oMetaAttribute = oRoot.getCollection("~O20000000Y10[MetaAttribute]").item(pMetaAttribute.megaField)

	' if it's an metaattribute we proceed else it's a meteaAssociation End
		if (oMetaAttribute.getId() <> 0) then
        ' we remove the meta attribute of system level

				Dim tecLevel
				tecLevel = StrComp(pMetaAttribute.getPRop("Technical Level"),"K")

        if (tecLevel <> 0 ) or (StrComp("Short Name",pMetaAttribute.name) = 0) then


        Dim technincalName
        technincalName = GetTechnicalName(oMetaAttribute.getProp("~Z20000000D60[Short Name]"))
    
      	Dim sIdAbs
    	  sIdAbs = oMetaAttribute.getProp("~310000000D00[Absolute Identifier]")
    
    	  Dim sDescription
    	  sDescription = cleanComment(oMetaAttribute.getProp("~f10000000b20[Comment]","Display"))
    
        Dim sType
        sType =  GetGraphQLType(oMetaAttribute.getProp("~Q10000000f10[MetaAttribute Type]","internal"))
    
    		Dim isEnum
    	  isEnum = hasMetaAttributeValue(oMEtaAttribute)

        Dim sFilter 
        sFilter = addFilterForMetaAttribute(oMEtaAttribute,isEnum)

				Dim sMaxLength
				sMaxLength = oMetaAttribute.getProp("~R10000000j10[MetaAttribute Length]")

        'dim atNature
        'atNature =  oMetaAttribute.getProp("~P10000000b10[_AtNature]","internal")

        dim sMacroCount
        sMacroCount =  oMetaAttribute.getCollection("~QwTGr7K2sO00[Macro]").count()

        dim sReadOnly
        sReadOnly = "false"

				if (sMacroCount > 0 ) then
        	sReadOnly = "true"
				end if


				Dim sMandatory
				sMandatory = getMandatoryFields(oMEtaAttribute)

        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{")
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & technincalName & Chr(34) & ",")
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & sIdAbs & Chr(34) & ",")  
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "description" & Chr(34) & ":" & Chr(34) & sDescription & Chr(34) & ",")  
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "constraints" & Chr(34) & ": {" )  
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9)  & Chr(34) & "type" & Chr(34) & ":" & Chr(34) & sType & Chr(34)  & ",") 
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9)  & Chr(34) & "mandatory" & Chr(34) & ":" & sMandatory & ",")  
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9)  & Chr(34) & "maxLength" & Chr(34) & ":" & Chr(34) & sMaxLength  & Chr(34)  & ",") 
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9)  & Chr(34) & "readOnly" & Chr(34) & ":" & sReadOnly &  ",") 
        File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9)  & Chr(34) & "filter" & Chr(34) & ":" & sFilter &  "")  		


				If isEnum = 0 Then
          File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}" )
    		Else
          File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}," )
    		End if  
      	
        Dim useless
        useless = manageMetaAttributeValue(oMetaAttribute,isEnum ,File)
      
    
        if not (i=oCount) then
          File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "},")
        Else
          File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}")
        End If
      
      End IF
    else 
    ' TO-DO manage the case of the metaAssociationEnd of "1" ?
    end if

    Set oMetaAttribute = Nothing
  Next

Set pColMetaAttribute = Nothing            

End Function

Function getMandatoryFields(oMEtaAttribute)
  Dim sMandatory 
  sMandatory = "false"

	if oMEtaAttribute.sameID("~Z20000000D60[Short Name]") then sMandatory = "true"

	getMandatoryFields= sMandatory 
End function

Function addFilterForMetaAttribute(oMEtaAttribute,isEnum)
  Dim sFilter 
  sFilter = "false"

	If isEnum = 0 Then
		sFilter = "false"
	Else
		sFilter = "true"
	End if

	if oMEtaAttribute.sameID("~Z20000000D60[Short Name]") then sFilter = "true"


	addFilterForMetaAttribute = sFilter 
End function



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
				Dim sValueName
				sValueName= GetTechnicalNameMetaAttribute(oMEtaAttributeValue.getProp("~H3l5fU1F3n80[Value Name]"))

				Dim sNameInternalValue
				sNameInternalValue = oMEtaAttributeValue.getProp("~L20000000L50[Internal Value]")

      	Dim sIdAbs
    	  sIdAbs = oMEtaAttributeValue.getProp("~310000000D00[Absolute Identifier]")

    	  Dim sDescription
    	  sDescription = cleanComment(oMEtaAttributeValue.getProp("~f10000000b20[Comment]","Display"))

				if (iCountMAV = 1) then
        	enumValues = "" & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{" &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & sValueName & Chr(34) & "," &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & sIdAbs& Chr(34) & "," &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "description" & Chr(34) & ":" & Chr(34) & sDescription & Chr(34) & "," &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "internalValue" & Chr(34) & ":" & Chr(34) & sNameInternalValue  & Chr(34) & "" &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}"
            
				Else
					if (j = 1) then
        	enumValues = enumValues &_
            "" & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{" 
          else
        	enumValues = enumValues &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "{"

					
					end if
        	enumValues = enumValues &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "name" & Chr(34) & ":" & Chr(34) & sValueName & Chr(34) & "," &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & sIdAbs& Chr(34) & "," &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "description" & Chr(34) & ":" & Chr(34) & sDescription & Chr(34) & "," &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "internalValue" & Chr(34) & ":" & Chr(34) & sNameInternalValue  & Chr(34) & "" &_
            "" &  Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "}"
            
          if Not (j=iCountMAV) then
          	enumValues = enumValues & ","
          end if
  


				end if

      next


    	File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(34) & "enumValues" & Chr(34) & ":[" )
    	File.writeLine( enumValues )
    	File.writeLine(Chr(9) & Chr(9) & Chr(9) & Chr(9) & Chr(9) & "]")
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
   oColResult.remove(oRoot.getObjectFromId("~d20000000T60[Object MetaClass IdAbs]"))
   oColResult.remove(oRoot.getObjectFromId("~930000000b80[Confidential Object]"))
   oColResult.remove(oRoot.getObjectFromId("~L30000000L90[Is Readable]"))
   oColResult.remove(oRoot.getObjectFromId("~U30000000v90[Is UI Readable]"))
   oColResult.remove(oRoot.getObjectFromId("~M30000000P90[Is Writable]"))
   oColResult.remove(oRoot.getObjectFromId("~030000000180[Reading access area]"))
   oColResult.remove(oRoot.getObjectFromId("~)20000000z70[Reading access area identifier]"))
   oColResult.remove(oRoot.getObjectFromId("~g20000000f60[Generic Local name]"))
   oColResult.remove(oRoot.getObjectFromId("~rr0pynS5yC00[Object Validity]"))
   oColResult.remove(oRoot.getObjectFromId("~eg6JrB6F1rH0[Object Validity Report]"))
   oColResult.remove(oRoot.getObjectFromId("~V30000000z90[Status <Diagnostic>]"))
   oColResult.remove(oRoot.getObjectFromId("~l20000000z60[Comment Header]"))
   oColResult.remove(oRoot.getObjectFromId("~p20000000D70[Object MetaClass Name]"))
   oColResult.remove(oRoot.getObjectFromId("~tbFegCw7AXA0[Picture Identifier]"))
   oColResult.remove(oRoot.getObjectFromId("~rOcwckE07r00[Translation Requirement By Mega Doc]"))
   oColResult.remove(oRoot.getObjectFromId("~dzKwedUCMXsR[_Report Edition Parameterization]"))
   oColResult.remove(oRoot.getObjectFromId("~Hf9MIW8gHn5S[_TranslationSettings]"))
   oColResult.remove(oRoot.getObjectFromId("~510000000L00[Creation Date]")) ' TO dO Keep ?
   oColResult.remove(oRoot.getObjectFromId("~610000000P00[Modification Date]")) ' TO DO keep ?
   oColResult.remove(oRoot.getObjectFromId("~(10000000v30[Creator]"))
   oColResult.remove(oRoot.getObjectFromId("~b10000000L20[Modifier]"))
   oColResult.remove(oRoot.getObjectFromId("~d10000000T20[Writing access area]"))
   oColResult.remove(oRoot.getObjectFromId("~520000000L40[Create Version]"))
   oColResult.remove(oRoot.getObjectFromId("~620000000P40[Update Version]"))
   oColResult.remove(oRoot.getObjectFromId("~a20000000H60[Language Update Date]"))
   oColResult.remove(oRoot.getObjectFromId("~)10000000z30[Creator Name]"))
   oColResult.remove(oRoot.getObjectFromId("~s20000000P70[Importance]"))
   oColResult.remove(oRoot.getObjectFromId("~c10000000P20[Modifier Name]"))
   oColResult.remove(oRoot.getObjectFromId("~e10000000X20[Writing access level]"))
   oColResult.remove(oRoot.getObjectFromId("~Z10000000D20[Status]"))
   oColResult.remove(oRoot.getObjectFromId("~c20000000P60[_DefineIdAbs]"))
   oColResult.remove(oRoot.getObjectFromId("~z20000000r70[Object Availability]"))
   oColResult.remove(oRoot.getObjectFromId("~y20000000n70[Object Origin IdAbs]"))
   oColResult.remove(oRoot.getObjectFromId("~m20000000170[Comment Body]"))
   oColResult.remove(oRoot.getObjectFromId("~310000000D00[Absolute Identifier]"))
   oColResult.remove(oRoot.getObjectFromId("~3GYCQDP0BDN0[Owning Library]"))
   oColResult.remove(oRoot.getObjectFromId("~OGN)aXoGH9LD[_System Current Workflow Status Information <Not Visible>]"))
   oColResult.remove(oRoot.getObjectFromId("~xkUUefdeFbqE[Immutability]"))
   oColResult.remove(oRoot.getObjectFromId("~1JubVrPLAfG0[Variable Object Picture Identifier]"))
   oColResult.remove(oRoot.getObjectFromId("~sB7o8gyyBjI0[Validation State]"))
   oColResult.remove(oRoot.getObjectFromId("~TUOYtiORGbG1[Current Validation]"))
   oColResult.remove(oRoot.getObjectFromId("~w9KvON(3FHDP[Current Workflow Status]"))
   oColResult.remove(oRoot.getObjectFromId("~RGzwku(yBrV1[Previous Validation State  <Deprecated>]"))
   oColResult.remove(oRoot.getObjectFromId("~(YByTkohHrGG[Current State]"))
   oColResult.remove(oRoot.getObjectFromId("~lxfuKBkYk400[MMI]"))
   oColResult.remove(oRoot.getObjectFromId("~7cwMFl57pe00[Purpose]"))
   oColResult.remove(oRoot.getObjectFromId("~CoK3h1oc8Di0[GenericLabel]"))
   oColResult.remove(oRoot.getObjectFromId("~IoxIXG5yQ17N[Criticality <DoDAF>]"))
'   oColResult.remove(oRoot.getObjectFromId(""))


  ' --- add here ---
  ' aditionnal attribute you want to avoid

  cleanCollectionMetaAttribute = oColResult

End function




Function cleanComment(strVal)
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
    Do While InStr(1, strVal, "" & Chr(10) & "")
        strVal = Replace(strVal, "" & Chr(10) & "", "")
    Loop
    Do While InStr(1, strVal, "" & Chr(13) & "")
        strVal = Replace(strVal, "" & Chr(13) & "", "")
    Loop
    cleanComment= strVal 
End Function


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
		Do While InStr(1, strVal, ".")
        strVal = Replace(strVal, ".", "")
    Loop
		Do While InStr(1, strVal, ",")
        strVal = Replace(strVal, ",", "")
    Loop
		Do While InStr(1, strVal, ";")
        strVal = Replace(strVal, ";", "")
    Loop
		Do While InStr(1, strVal, ":")
        strVal = Replace(strVal, ":", "")
    Loop
    Do While InStr(1, strVal, "" & Chr(34) & "")
        strVal = Replace(strVal, "" & Chr(34) & "", "")
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