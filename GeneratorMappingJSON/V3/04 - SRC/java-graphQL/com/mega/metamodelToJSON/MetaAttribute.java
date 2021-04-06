package com.mega.metamodelToJSON;


import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

import com.mega.generator.Arguments;
import com.mega.generator.Generator;
import com.mega.mappingJSON.EnumValuesJSON;
import com.mega.mappingJSON.PropertiesJSON;
import com.mega.modeling.api.MegaCollection;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public class MetaAttribute  extends GenericAttributeOrTaggedValue  {
	
	public MetaAttribute(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject pMetaAttribute, HashMap<String,String> overrideNameList) {
		super(megaRoot, oMetamodel, pMetaAttribute, overrideNameList);
	}
	
	public MetaAttribute(String technincalName, String absoluteIdentifier, String sDescription, String maxLength, String type, boolean isMetaAttributeFilter, boolean isMandatory, boolean isReadOnly, boolean translatable, boolean formattedText, boolean isUnique,HashMap<String,String> overrideNameList) {
		super(technincalName, absoluteIdentifier, sDescription, maxLength, type, isMetaAttributeFilter, isMandatory, isReadOnly, translatable, formattedText, isUnique,overrideNameList);	
	}


		
	protected String customNaming(String technincalName, String type ) {
		String localName = technincalName;
				
		if (type.contentEquals("Id") || type.contentEquals("id") ) {
			if (localName.contentEquals("AbsoluteIdentifier")  || localName.contentEquals("absoluteIdentifier") ) {
				localName = "id";
			} else {
			//	localName = localName + "Id";
			}			
		} else {
			if (localName.contentEquals("ExternalIdentifier") || localName.contentEquals("externalIdentifier")) {
				localName = "externalId";	
			}			
		}
		return localName;
	}
	
	
	protected List<EnumValuesJSON> getMetaAttributeValues() {
		
		Generator.logger.finest("Start MetaAttributeValues");
				
		List<EnumValuesJSON> enumValuesJSONList = new ArrayList<EnumValuesJSON>();
	
		MegaCollection megaCollection = oMetaAttribute.getCollection(StaticFields.metaAttributeValue);

		while (megaCollection.hasNext()) {
			MegaObject oMetaAttributeValue = megaCollection.next();		
			MetaAttributeValue metaAttributeValue = new MetaAttributeValue(megaRoot, oMetamodel,oMetaAttribute,oMetaAttributeValue, overrideNameList);
			
			if (metaAttributeValue.getAtLeastOneCustom()) {
				setAtLeastOneCustom(true);
			}			
			
			if (metaAttributeValue.isValideMetaAttributeValue()) {
				enumValuesJSONList.add(metaAttributeValue.getEnumValuesJSON());
			}
		} //while

		MetaModel.reportDuplicateName("MetaAttributeValues", enumValuesJSONList, ensureUniqueName);			
		
		return enumValuesJSONList;
	}
	
	
	protected boolean includedAttribute(String technincalName) {
		// we remove certains attributes that are either too technical or redundant
		boolean include = true;
		
		String technicalLevel = oMetaAttribute.getProp(StaticFields.technicalLevel);

		// to CHECK to impact of this removel
		if(technicalLevel.compareTo("K") == 0) {
			include = false;
		} 
		
		String name = oMetaAttribute.getProp(StaticFields.shortName);
	
		if(name.compareTo("Short Name") == 0) {
			include = false;
		}


		
		if (technincalName.contentEquals("LinkModifierName")  || technincalName.contentEquals("linkModifierName") ) {
			include = true;
		} else if (technincalName.contentEquals("LinkCreatorName") || technincalName.contentEquals("linkCreatorName")) {
			include = true;	
		} else if (technincalName.contentEquals("CreatorName") || technincalName.contentEquals("creatorName")) {
			include = true;	
		} else if (technincalName.contentEquals("ModifierName") || technincalName.contentEquals("modifierName")) {
			include = true;	
		} else if (technincalName.contentEquals("IsWritable") || technincalName.contentEquals("isWritable")) {
			include = true;	
		}				
			
		return include;
	}



	
	protected boolean isReadOnly(String technincalName) {
		boolean readOnly = false;		
        int countMacro =  oMetaAttribute.getCollection(StaticFields.macro).size();
        int updatePermission = Integer.parseInt(oMetaAttribute.getProp(StaticFields.updatePermission));
        
        if (countMacro > 0) {
        	readOnly = true;
        }
        
        
        // if 0 then read only
        // if 1 then can be updated
        if (updatePermission < 1) {
        	readOnly = true;
        } else {
        	readOnly = false;
        }
 
        
        
		
		if (technincalName.contentEquals("ModificationDate")  || technincalName.contentEquals("modificationDate") ) {
			readOnly = true;
		} else if (technincalName.contentEquals("ModifierId") || technincalName.contentEquals("modifierId")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("Modifier") || technincalName.contentEquals("modifier")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("LinkModificationDate") || technincalName.contentEquals("linkModificationDate")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("LinkModifier") || technincalName.contentEquals("linkModifier")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("LinkModifierId") || technincalName.contentEquals("linkModifierId")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("IsWritable") || technincalName.contentEquals("isWritable")) {
			readOnly = true;	
		}						
        
        //System.out.println("isReadOnly " + readOnly + " - " + oMetaAttribute.getProp("Name"));
        
		return readOnly;
	}
	

	
	protected boolean isUnique() {
		boolean unique = false;

		String atIndex = oMetaAttribute.getProp(StaticFields.atIndex,"internal").toString();
		
		switch (atIndex) {  
		case "U":  // Unique 
			unique = true;
		break;   			
		case "S":  // Unique Case sensitive 
			unique = true;
		break; 			
		default:  
			//
		} 

		return unique;
	}
	

	public static List<PropertiesJSON> manualAttribute(boolean hasNameSpace, HashMap<String,String>  overrideNameList) {
			List<PropertiesJSON> propertiesJSONList = new ArrayList<PropertiesJSON>();	
		
			

		// we don't add the name in case of custom		
		if (!Arguments.getExtendOnly()) {
		if (hasNameSpace) {
				String technincalName5 = "Name";
				String absoluteIdentifier5 = "Z20000000D60"; 
				String sDescription5 = "Short Name of the object (posibility to get the long name with an argument)";
				String maxLength5 = "1024";
				String type5 = "String";
				boolean isMetaAttributeFilter5 = true; 
				boolean isMandatory5 = true; 
				boolean isReadOnly5 = false;			
				boolean translatable5 = true; 
				boolean formattedText5 = false;		
				boolean isUnique = false;
				MetaAttribute metaAttribute5 = new MetaAttribute(technincalName5, absoluteIdentifier5, sDescription5, maxLength5, type5, isMetaAttributeFilter5, isMandatory5, isReadOnly5, translatable5,formattedText5,isUnique,overrideNameList);
				PropertiesJSON propertiesJSON5 = metaAttribute5.getPropertiesJSON();
				propertiesJSONList.add(propertiesJSON5);				
				
			}
		
		}
		
	
		return propertiesJSONList;
	}	

	public static MegaCollection cleanCollectionMetaAttributeAssociation(MegaRoot megaRoot, MegaCollection megaCollection) {
		
		MegaCollection oCol = megaRoot.getSelection("");
		oCol.insert(megaCollection);
		List<String> listMetaAttribute = listofMetaAttributeAssociation();
		Iterator<String> it = listMetaAttribute.iterator();	
		while(it.hasNext()) {
			String strMetaAttribute= it.next();
			MegaObject objectToRemove = megaRoot.getObjectFromID(strMetaAttribute);
			boolean objectexist = objectToRemove.exists();
			if (objectexist) {
				oCol.remove(objectToRemove);
			}
		}

		return oCol;
	}


	
	public static List<String> listofMetaAttributeAssociation() {
		List<String> listMetaAttribute = new ArrayList<String>();
		listMetaAttribute.add("~tXERBv6k5r60[AssociativeObject]");
		listMetaAttribute.add("~C3cm9FyluS20[Link Comment]");
		return listMetaAttribute;		
	}	
	
	
	public static MegaCollection cleanCollectionMetaAttribute(MegaRoot megaRoot, MegaCollection megaCollection) {
		
		MegaCollection oCol = megaRoot.getSelection("");
		oCol.insert(megaCollection);
		
		
		List<String> listMetaAttribute = listofMetaAttributeToRemove();
		Iterator<String> it = listMetaAttribute.iterator();	
		while(it.hasNext()) {
			String strMetaAttribute= it.next();
			MegaObject objectToRemove = megaRoot.getObjectFromID(strMetaAttribute);
			boolean objectexist = objectToRemove.exists();
			if (objectexist) {
				oCol.remove(objectToRemove);
			}
		}

		return oCol;
	}
	
	public static List<String> listofMetaAttributeToRemove() {
		List<String> listMetaAttribute = new ArrayList<String>();
		
		
		listMetaAttribute.add("~M30000000P90[Is Writable]");
		listMetaAttribute.add("~Z20000000D60[Short Name]");
		listMetaAttribute.add("~CFmhlMxNT1iE[External Identifier]");
		listMetaAttribute.add("~210000000900[Name]");
		listMetaAttribute.add("~f10000000b20[Comment]");
		listMetaAttribute.add("~H20000000550[_HexaIdAbs]");
		listMetaAttribute.add("~310000000D00[Absolute Identifier]");
		listMetaAttribute.add("~)10000000z30[Creator Name]");
		listMetaAttribute.add("~s20000000P70[Importance]");
		listMetaAttribute.add("~c10000000P20[Modifier Name]");
		listMetaAttribute.add("~510000000L00[Creation Date]");
		listMetaAttribute.add("~610000000P00[Modification Date]");
		listMetaAttribute.add("~(10000000v30[Creator]");
		listMetaAttribute.add("~b10000000L20[Modifier]");
		listMetaAttribute.add("~g20000000f60[Generic Local name]");
		listMetaAttribute.add("~nOU8g8IMCb30[Converted Name Version]");
		listMetaAttribute.add("~f20000000b60[Log]");
		listMetaAttribute.add("~m20000000170[Comment Body]");
		listMetaAttribute.add("~d20000000T60[Object MetaClass IdAbs]");
		listMetaAttribute.add("~930000000b80[Confidential Object]");
		listMetaAttribute.add("~L30000000L90[Is Readable]");
		listMetaAttribute.add("~U30000000v90[Is UI Readable]");
		listMetaAttribute.add("~030000000180[Reading access area]");
		listMetaAttribute.add("~)20000000z70[Reading access area identifier]");
		listMetaAttribute.add("~rr0pynS5yC00[Object Validity]");
		listMetaAttribute.add("~eg6JrB6F1rH0[Object Validity Report]");
		listMetaAttribute.add("~V30000000z90[Status <Diagnostic>]");
		listMetaAttribute.add("~l20000000z60[Comment Header]");
		listMetaAttribute.add("~p20000000D70[Object MetaClass Name]");
		listMetaAttribute.add("~tbFegCw7AXA0[Picture Identifier]");
		listMetaAttribute.add("~rOcwckE07r00[Translation Requirement By Mega Doc]");
		listMetaAttribute.add("~dzKwedUCMXsR[_Report Edition Parameterization]");
		listMetaAttribute.add("~Hf9MIW8gHn5S[_TranslationSettings]");
		listMetaAttribute.add("~d10000000T20[Writing access area]");
		listMetaAttribute.add("~520000000L40[Create Version]");
		listMetaAttribute.add("~620000000P40[Update Version]");
		listMetaAttribute.add("~a20000000H60[Language Update Date]");
		listMetaAttribute.add("~e10000000X20[Writing access level]");
		listMetaAttribute.add("~Z10000000D20[Status]");
		listMetaAttribute.add("~c20000000P60[_DefineIdAbs]");
		listMetaAttribute.add("~z20000000r70[Object Availability]");
		listMetaAttribute.add("~y20000000n70[Object Origin IdAbs]");
		listMetaAttribute.add("~m20000000170[Comment Body]");
		listMetaAttribute.add("~3GYCQDP0BDN0[Owning Library]");
		listMetaAttribute.add("~OGN)aXoGH9LD[_System Current Workflow Status Information <Not Visible>]");
		listMetaAttribute.add("~xkUUefdeFbqE[Immutability]");
		listMetaAttribute.add("~1JubVrPLAfG0[Variable Object Picture Identifier]");
		listMetaAttribute.add("~sB7o8gyyBjI0[Validation State]");
		listMetaAttribute.add("~RGzwku(yBrV1[Previous Validation State  <Deprecated>]");
		listMetaAttribute.add("~lxfuKBkYk400[MMI]");
		listMetaAttribute.add("~7cwMFl57pe00[Purpose]");
		listMetaAttribute.add("~CoK3h1oc8Di0[GenericLabel]");
		listMetaAttribute.add("~IoxIXG5yQ17N[Criticality <DoDAF>]");
		listMetaAttribute.add("~N10000000T10[Permission]");
		listMetaAttribute.add("~Dv(CwL3ZF1f4[CollectionPermission]");
		listMetaAttribute.add("~EMeJuBbAGfQJ[IllustratedPictureIdentifier]");
		listMetaAttribute.add("~eIXaJCfyhuJ0[Parameterization]");
		listMetaAttribute.add("~B6rudNEDqC20[Capability]");
		listMetaAttribute.add("~z10000000r30[TechnicalLevel]");
		listMetaAttribute.add("~aXWJUhq2DHe2[GRCsortkey]");
		listMetaAttribute.add("~gTOjdYxEAXa0[GRCCode]");
		listMetaAttribute.add("~JVOjoZxEAPc0[GRCGUID]");
		listMetaAttribute.add("~O9Wuh9EHAb90[GRCUserName]");
		listMetaAttribute.add("~eAWuo9EHA9E0[GRCUserPassword]");
		listMetaAttribute.add("~g10000000f20[PassWord]");
		listMetaAttribute.add("~faJBOms0EDxE[TimePeriodcolorwithfixeddate]");
		listMetaAttribute.add("~naJBWHr0Ev8E[TimePeriodcolorwithnonfixeddate]");
		listMetaAttribute.add("~gl5Hs6hkPnNL[]");
		listMetaAttribute.add("~2qdSxJfqOnXF[Business Drivers How do you consume data from your application]");
		listMetaAttribute.add("~bl5HFcgkPnwJ[Business Drivers What is the application scability pattern]");
		listMetaAttribute.add("~xsdSWKfqOzgF[Business Drivers What is the current expected level of current SLA]");
		listMetaAttribute.add("~(l5H)jgkP1jK[Business Drivers What is your criterion for application performance]");
		listMetaAttribute.add("~qtdScKfqO9kF[Process Maturity What is the average skill on Cloud technologies and practices within your development team]");
		listMetaAttribute.add("~UtdS3LfqOvwF[Process Maturity What is the level of deployment process automation for provisioning & configuration]");
		listMetaAttribute.add("~NrdSLLfqOH1G[Process Maturity What is your evolution model and feedback loop implementation]");
		listMetaAttribute.add("~6rdSHKfqObaF[Technical Enablers How the application is exposed to external services/apps]");
		listMetaAttribute.add("~XsdSvKfqOjtF[Technical Enablers Is this application multi-tenant]");
		listMetaAttribute.add("~frdSqKfqOXqF[Technical Enablers What are the relationships beyond the application boundaries]");
		listMetaAttribute.add("~lqdSjKfqOLnF[Technical Enablers What is the application database provider]");
		listMetaAttribute.add("~TqdSFLfqO5(F[Technical Enablers What is the current deployment platform]");
		listMetaAttribute.add("~1sdSPKfqOndF[Technical Enablers What is the current user authentication mechanism]");
		listMetaAttribute.add("~i10000000n20[_AuthorizationSet]");
		listMetaAttribute.add("~hpmU8xtmuC20[_CODE Directory History]");
		listMetaAttribute.add("~2fn3P4Hfkm10[_DatabaseCfg]");
		listMetaAttribute.add("~xqmN34sEl400[_DatabaseReport]");
		listMetaAttribute.add("~tBP8f721r400[_DrawingTextFormat]");
		listMetaAttribute.add("~iKdnITSLxW40[_DrawingXmlFormat]");
		listMetaAttribute.add("~hGOgRgwcIn8G[_FavoritesContacts]");
		listMetaAttribute.add("~bPjcN3wNyCE0[_GenNewHelpPage]");
		listMetaAttribute.add("~RIvPdVs3iS10[_Ident]");
		listMetaAttribute.add("~Y10000000920[_idOcc]");
		listMetaAttribute.add("~18P8g721re00[_idRel]");
		listMetaAttribute.add("~hWpWSK8kArC0[_Mega  In Help]");
		listMetaAttribute.add("~m10000000130[_mtType]");
		listMetaAttribute.add("~3zHcgbf0Cf30[_NextStatusInstanceInformation]");
		listMetaAttribute.add("~k10000000v20[_Ordinal]");
		listMetaAttribute.add("~iXZG)R4EIbiM[_ParameterizationOfAlertsScheduling]");
		listMetaAttribute.add("~pCAtrcUGHDO0[_PersonsAssignedForTransition]");
		listMetaAttribute.add("~dzKwedUCMXsR[_Report Edition Parameterization]");
		listMetaAttribute.add("~420000000H40[_ScanInit]");
		listMetaAttribute.add("~OGN)aXoGH9LD[_System Current Workflow Status Information <Not Visible>]");
		listMetaAttribute.add("~l10000000z20[_Temporary]");
		listMetaAttribute.add("~Hf9MIW8gHn5S[_TranslationSettings]");
		listMetaAttribute.add("~GXWWPTuQyy30[_TypeInformation]");
		listMetaAttribute.add("~G20000000150[_UserAccessProfile]");
		listMetaAttribute.add("~BAHujIEBAL83[Action Type]");
		listMetaAttribute.add("~dswHvWaHFL2U[Active Workflow Transition Instance]");
		listMetaAttribute.add("~wjhzOAGeFD0F[Agent Component Picture Identifier]");
		listMetaAttribute.add("~GP5qz7DbAv70[All Users]");
		listMetaAttribute.add("~ahaolqy7Mz1G[Analytics Information Deprecated]");
		listMetaAttribute.add("~rsC4041SHfCF[Answer Comment <Deutsch>]");
		listMetaAttribute.add("~XjAsBO(RHLCQ[Answer Comment <English>]");
		listMetaAttribute.add("~4tC4041SHbDF[Answer Comment <Español>]");
		listMetaAttribute.add("~1lAs1N(RH17Q[Answer Comment <Français>]");
		listMetaAttribute.add("~CqC4141SHzHF[Answer Comment <Italiano>]");
		listMetaAttribute.add("~5EIuEYlVF1kJ[Answer to the security question]");
		listMetaAttribute.add("~wI7SzddDADT3[Application Planning]");
		listMetaAttribute.add("~l7Z9LXI1RD5Q[ArchiMate - Active DiagramTypeViews]");
		listMetaAttribute.add("~Sgi1tSmUQvv6[ArchiMate - Classifiying Attribute]");
		listMetaAttribute.add("~XVsu6FETD1UJ[Architecture Use Picture Identifier]");
		listMetaAttribute.add("~adUnEhSd2vR0[Arity]");
		listMetaAttribute.add("~KrmvAg0ZA5V1[Artifact Component Picture Identifier]");
		listMetaAttribute.add("~el42vfm(DHfD[AsIs Milestone]");
		listMetaAttribute.add("~TOuRw)6pLXZP[Assessment Start Mode]");
		listMetaAttribute.add("~O30000000X90[Assignment End Date]");
		listMetaAttribute.add("~N30000000T90[Assignment Start Date]");
		listMetaAttribute.add("~5OSkJuBE)u30[Association Kind]");
		listMetaAttribute.add("~bx7yd2FEAv50[Attach objects]");
		listMetaAttribute.add("~j0wnbbCb6vB0[Automatic Object]");
		listMetaAttribute.add("~mAH8L6IX0900[Available products]");
		listMetaAttribute.add("~4If5zQniyW10[BackupedObjetIdAbs]");
		listMetaAttribute.add("~KtNn6eiOHvG0[BaselineMaxDate]");
		listMetaAttribute.add("~jlfUkt7AIPQJ[Binary File]");
		listMetaAttribute.add("~GBHmVECX65D0[BPMX Technical Information]");
		listMetaAttribute.add("~HYn(RplEIbl8[ChangeItemProcess Date Index]");
		listMetaAttribute.add("~bX3edYUo51F0[ChangeItemVisibility]");
		listMetaAttribute.add("~bpn)a(Fyt000[Column mapping]");
		listMetaAttribute.add("~CzQ4jCnNFf4L[Comment Displaying]");
		listMetaAttribute.add("~JTkDRd5cziY0[COMNAME_GBMCOMNATURE]");
		listMetaAttribute.add("~)SkDJd5czyX0[COMNAME_GBMNATURE]");
		listMetaAttribute.add("~jTkDfd5czSZ0[COMNAME_IDOP]");
		listMetaAttribute.add("~8i00A30rxCI0[Connector Direction]");
		listMetaAttribute.add("~vCIujtkVFTqH[Connexion status]");
		listMetaAttribute.add("~Bo8z26XBHTgK[Context Order]");
		listMetaAttribute.add("~1xhMyINwBL40[Contribution Triggering Done]");
		listMetaAttribute.add("~nOU8g8IMCb30[Converted Name Version]");
		listMetaAttribute.add("~YIX2JlGzM9L2[Corresponding Exchange]");
		listMetaAttribute.add("~MtdSWGfqOvyE[Could failure of this application lead to disruption? Please define the level of impact.]");
		listMetaAttribute.add("~DrdSmGfqOH3F[Could failure of this application lead to harm of the company's public image? Please define the level of impact.]");
		listMetaAttribute.add("~7sdStGfqOT6F[Could failure of this application lead to loss of customer confidence? Please define the level of impact.]");
		listMetaAttribute.add("~IqdSeGfqO50F[Could failure of this application lead to loss of revenue or business opportunity? Please define the level of impact.]");
		listMetaAttribute.add("~k3f6dp10)4G0[Create Permission <Read-Only Session>]");
		listMetaAttribute.add("~G0f6ek10)G60[Create Permission <User Interface> <Deprecated>]");
		listMetaAttribute.add("~PWLh8NBfS5ZP[Created from ITPM]");
		listMetaAttribute.add("~Hlqo7FD2CrC0[Creation DateTime  Workflow Status Instance]");
		listMetaAttribute.add("~)10000000z30[Creator Name]");
		listMetaAttribute.add("~TUOYtiORGbG1[Current Validation]");
		listMetaAttribute.add("~s10000000P30[Data access]");
		listMetaAttribute.add("~Us4lhDa8NbGP[DataBase Id]");
		listMetaAttribute.add("~Dr4lBJa8NTLP[DataBase Name]");
		listMetaAttribute.add("~AGcxyx0QMTRE[Database Synchronization Trigger]");
		listMetaAttribute.add("~MlsNZ(rsk400[Datatype]");
		listMetaAttribute.add("~H)0t)pjLBf60[Dbms Version Order]");
		listMetaAttribute.add("~RUywbxkVFvnA[Default Desktop Deprecated]");
		listMetaAttribute.add("~)0f61q10)SJ0[Delete Permission <Read-Only Session>]");
		listMetaAttribute.add("~j2f6wl10)yA0[Delete Permission <User Interface> <Deprecated>]");
		listMetaAttribute.add("~AsZNBXsFHzIV[Deploy Sessions Status]");
		listMetaAttribute.add("~8jDowIj4ProV[Deployment Collection Source]");
		listMetaAttribute.add("~9SwtdMU8HTXR[Deployment Status]");
		listMetaAttribute.add("~E5xR4h7BD9N1[Design Task Baseline]");
		listMetaAttribute.add("~DNxivTRj7b30[DiagramTypeLink LineBeginShape]");
		listMetaAttribute.add("~jKxixURj7T50[DiagramTypeLink LineEndShape]");
		listMetaAttribute.add("~SKBw0ckfDPVB[DiagramTypeObject CreationGesture]");
		listMetaAttribute.add("~qgYHmPqYDng2[DiagramTypePath CreationGesture]");
		listMetaAttribute.add("~hGoJiTra6bn0[DiagramTypePath Hidden]");
		listMetaAttribute.add("~gy0N4qL7pu00[DiagramTypeStorage]");
		listMetaAttribute.add("~tbEuJRE8GnKE[DocPicture]");
		listMetaAttribute.add("~UQ8cNafGDjiT[DocumentRTFText]");
		listMetaAttribute.add("~tXEx)qU41L20[DocumentText]");
		listMetaAttribute.add("~w4(hTHyl8980[DoDAF Architecture Modeling Method]");
		listMetaAttribute.add("~WrdSGGfqOXsE[Does the application serve internal or external users?]");
		listMetaAttribute.add("~eiFjCXisGLjP[Enable Snapshots]");
		listMetaAttribute.add("~Jq9j5RRHCPB0[Estimated End Date]");
		listMetaAttribute.add("~xt9j2ORHC560[Estimated Start Date]");
		listMetaAttribute.add("~n7UsIiD(xS10[Evaluation]");
		listMetaAttribute.add("~HfxvjZVeBvT0[Event Category]");
		listMetaAttribute.add("~EtbTalNfEjcF[Exchange Rate]");
		listMetaAttribute.add("~bqbTwlNfEngF[Exchange Rate Date]");
		listMetaAttribute.add("~UQtYIkOGz800[FK mapping]");
		listMetaAttribute.add("~9ryp42jl8D50[Functional Quality]");
		listMetaAttribute.add("~rSUv1sXqPH87[GDPR - Is Archived]");
		listMetaAttribute.add("~nkzG1B6yPfUF[GDPR - Order]");
		listMetaAttribute.add("~BMzDzkp8HXxM[Generate Questionnaires Status]");
		listMetaAttribute.add("~6F4C49pn)OT0[GhostMetaClassIdAbs]");
		listMetaAttribute.add("~sBFTom7jMrsQ[Has Touchpoints]");
		listMetaAttribute.add("~CjvjWdhJOXuC[Hide Valorized Parameters]");
		listMetaAttribute.add("~XTT)C(FTDHVJ[Human Asset Picture Identifier]");
		listMetaAttribute.add("~0d(4NGHv2970[Identifier-Dependent Entity]");
		listMetaAttribute.add("~xkUUefdeFbqE[Immutability]");
		listMetaAttribute.add("~uXVIwfK6Bz00[Implementing Macro]");
		listMetaAttribute.add("~pgV4Q1IP2Hu0[INCREMENT BY  Oracle <deprecated>]");
		listMetaAttribute.add("~MGO046Wp(000[Indicator direct value]");
		listMetaAttribute.add("~0J7SDfdDATe3[Infrastructure Planning]");
		listMetaAttribute.add("~U8q(EklZ3T80[Inherited Comment]");
		listMetaAttribute.add("~NVKR4x6b3f10[Inherited Comment <Deutsch>]");
		listMetaAttribute.add("~8VKR3x6b3n00[Inherited Comment <English>]");
		listMetaAttribute.add("~cVKR5x6b3X20[Inherited Comment <Français>]");
		listMetaAttribute.add("~ISKR7x6b3950[Inherited Comment <Italiano>]");
		listMetaAttribute.add("~jVKR5x6b3z20[Inherited Comment <Spanish>]");
		listMetaAttribute.add("~dcuUxkt0Cv10[Initial Workflow Status Instance]");
		listMetaAttribute.add("~hgyx()(iva10[Interaction Local Name]");
		listMetaAttribute.add("~tavGnmb7Lj0K[is Report Editable <System>]");
		listMetaAttribute.add("~ltdSTEfqOncE[Is this application a custom or a COTS?]");
		listMetaAttribute.add("~CqdSyFfqOboE[Is this application in line with the company's future technology direction?]");
		listMetaAttribute.add("~U30000000v90[Is UI Readable]");
		listMetaAttribute.add("~N0cxvyUSJTfH[isAnyWhere]");
		listMetaAttribute.add("~GtbTC)OfEvLK[Issuer]");
		listMetaAttribute.add("~oBohM(A2PPL9[IsWebAccountActivated]");
		listMetaAttribute.add("~SGi7p(XsKP31[Kind of proposition]");
		listMetaAttribute.add("~a20000000H60[Language Update Date]");
		listMetaAttribute.add("~AmrRTz6BzaG0[LanguageUpdateDate <German>]");
		listMetaAttribute.add("~lruWiwPENzc5[Last Assessment]");
		listMetaAttribute.add("~xFIuXclVF5qJ[Last password change]");
		listMetaAttribute.add("~opePJE7U9L31[LDAP User Identifier]");
		listMetaAttribute.add("~YG1oeBzXGXQ3[Library Type]");
		listMetaAttribute.add("~820000000X40[Link Creator Name]");
		listMetaAttribute.add("~A20000000f40[Link Modifier Name]");
		listMetaAttribute.add("~b20000000L60[LinkLanguageUpdateDate]");
		listMetaAttribute.add("~0nrRTz6BzyJ0[LinkLanguageUpdateDate <German>]");
		listMetaAttribute.add("~THoMojrh6n10[Linktype]");
		listMetaAttribute.add("~f20000000b60[Log]");
		listMetaAttribute.add("~6kSdFxvCFDFF[Main Workflow Status]");
		listMetaAttribute.add("~V4tmgHyVy800[Mapping Selection]");
		listMetaAttribute.add("~qkXzfQ)X9H30[Master Plan Start Date Saved]");
		listMetaAttribute.add("~uUIJZJLXBXJ1[Message Flow CreationKind]");
		listMetaAttribute.add("~TLVnH2zCv4T1[Message local name]");
		listMetaAttribute.add("~ijtl(AiGJL12[Meta Scope]");
		listMetaAttribute.add("~4000)))sMHQC[MetaDiagramType Library]");
		listMetaAttribute.add("~WOLaD6vuu000[MetaDiagramType Library32]");
		listMetaAttribute.add("~O300)))UAIQC[MetaDiagramTypeBitmap]");
		listMetaAttribute.add("~W000)))mAIQC[MetaDiagramTypeCursor]");
		listMetaAttribute.add("~jUERqz6y9v30[MetaTreeBranchMetaClassFilter]");
		listMetaAttribute.add("~oYhsQ0mq(q3k[Method overview]");
		listMetaAttribute.add("~Gs9jUURHCXL0[Milestone Estimated Date]");
		listMetaAttribute.add("~XvFt6MUDGHpO[MinMaxValueLayout]");
		listMetaAttribute.add("~dTbxk3VxK1wO[Mode Questionnaires Creation <deprecated>]");
		listMetaAttribute.add("~UW5hzwS03vJ3[Navigability Display]");
		listMetaAttribute.add("~oCIuIjlVFLyJ[Number of failed login attempts]");
		listMetaAttribute.add("~5MBfaJgSn400[Number of Times]");
		listMetaAttribute.add("~z20000000r70[Object Availability]");
		listMetaAttribute.add("~d20000000T60[Object MetaClass IdAbs]");
		listMetaAttribute.add("~p20000000D70[Object MetaClass Name]");
		listMetaAttribute.add("~T3vfqZw0CjB0[OrgUnit Component MetaPicture Identifier]");
		listMetaAttribute.add("~8mkrnST0uK00[Origin]");
		listMetaAttribute.add("~vasuYLkI1j10[Owner]");
		listMetaAttribute.add("~63g32ymV9z30[Parent Idabs]");
		listMetaAttribute.add("~yYwUektD)000[Parent/child]");
		listMetaAttribute.add("~X4sWyGeE9fF0[Participant Type]");
		listMetaAttribute.add("~UcF8H62ZFHE5[Password management mode]");
		listMetaAttribute.add("~HWB)5G0rTPQV[PasswordsLRU]");
		listMetaAttribute.add("~kWkp2IF5HvpN[Path]");
		listMetaAttribute.add("~NmXS0qAIAP35[Physical Asset Picture Identifier]");
		listMetaAttribute.add("~ZaAlhAztIX02[Picture Id]");
		listMetaAttribute.add("~tbFegCw7AXA0[Picture Identifier]");
		listMetaAttribute.add("~7lmqbEjCz800[PK mapping]");
		listMetaAttribute.add("~RGzwku(yBrV1[Previous Validation State  <Deprecated>]");
		listMetaAttribute.add("~G8RVMuN5u000[Primary Index]");
		listMetaAttribute.add("~w10000000f30[Profile manager]");
		listMetaAttribute.add("~RbFbTcPfEL5K[Progression Rate]");
		listMetaAttribute.add("~5dx1OqWoKrP9[Project path]");
		listMetaAttribute.add("~uG7ShfdDA5k3[Project Planning]");
		listMetaAttribute.add("~KzXYUKiKObRP[Project Status]");
		listMetaAttribute.add("~BfUK8RboKn7M[Project tree title]");
		listMetaAttribute.add("~IlhZBz8UoK00[Quality]");
		listMetaAttribute.add("~nZWnrQSiH5MH[Questionnaire Validation Workflow]");
		listMetaAttribute.add("~qY(XlfQ7E9qV[RACI BPMN Default Value]");
		listMetaAttribute.add("~U0f6zp10)iH0[Read Permission <Read-Only Session>]");
		listMetaAttribute.add("~T1f6Ol10)480[Read Permission <User Interface> <Deprecated>]");
		listMetaAttribute.add("~CM7LDI5fy800[Recycle bin]");
		listMetaAttribute.add("~6LJ6RnYuKLiT[Report DataSet Result]");
		listMetaAttribute.add("~Vt0MLXLBLPGH[Report DataSet Result <Deutsch>]");
		listMetaAttribute.add("~xq0MKXLBLH6H[Report DataSet Result <English>]");
		listMetaAttribute.add("~dq0MOXLBLfaH[Report DataSet Result <Español>]");
		listMetaAttribute.add("~Lt0MOXLBLbVH[Report DataSet Result <Français>]");
		listMetaAttribute.add("~ut0MbXLBLbmI[Report DataSet Result <Italiano>]");
		listMetaAttribute.add("~DKckRsBBLHPW[Report DataSet Result Date]");
		listMetaAttribute.add("~kuKj2)y(RDTS[Report Graph Arc Group Above]");
		listMetaAttribute.add("~Q30000000f90[Repository Access Definition Mode]");
		listMetaAttribute.add("~t10000000T30[Repository Access Mode]");
		listMetaAttribute.add("~xt)IXn4JBz60[Requester]");
		listMetaAttribute.add("~tOu6HTIrHzQI[Reserved Objects]");
		listMetaAttribute.add("~U0jKF5cCAvi0[RFC State <Deprecated>]");
		listMetaAttribute.add("~NHqtc0Ek)S30[RTF Content]");
		listMetaAttribute.add("~lm8QfisQODRC[Running timestamp  System Job Execution]");
		listMetaAttribute.add("~ijXzGQ)X9L00[Schedule Date Begin Saved]");
		listMetaAttribute.add("~JkXzVQ)X9r10[Schedule Date End Saved]");
		listMetaAttribute.add("~WFIu0zkVFX9I[Security question]");
		listMetaAttribute.add("~XOLV9zNs)u60[Sequence add-ons]");
		listMetaAttribute.add("~68DCn9JXB1P0[Sequence Flow CreationKind]");
		listMetaAttribute.add("~R7jIFl6azWR0[Service Application]");
		listMetaAttribute.add("~GUsFV1aIPPe8[Sketching Item Picture Identifier]");
		listMetaAttribute.add("~r4pjivRV9r50[SolMan Description]");
		listMetaAttribute.add("~17pjHxRV9H80[SolMan Io Class]");
		listMetaAttribute.add("~1BWTfxtP95J0[SolMan Storage Category]");
		listMetaAttribute.add("~f6pjAqSV9121[SolMan Storage Category Url]");
		listMetaAttribute.add("~DH7SqedDArY3[Solution Planning]");
		listMetaAttribute.add("~LLjDueJ)A590[Standard Planning]");
		listMetaAttribute.add("~DnCfRNQhNnVN[Start Session Execution Mode]");
		listMetaAttribute.add("~7eV4C2IP2Pw0[START WITH  Oracle <deprecated>]");
		listMetaAttribute.add("~2PxgmmwTIX0D[Step Order]");
		listMetaAttribute.add("~HbXzpRlx4T20[Stereotype]");
		listMetaAttribute.add("~gdgzBHaVCz(0[Stereotype Extension]");
		listMetaAttribute.add("~GYXdc8YLALSB[Strategic Planning]");
		listMetaAttribute.add("~tcl5kdbMPnbK[System Component Picture]");
		listMetaAttribute.add("~Y0BJ2T5VFjw5[System ID]");
		listMetaAttribute.add("~8TD21mpFHrnV[System Trigger]");
		listMetaAttribute.add("~Omkr(ST0uW00[Table mapping]");
		listMetaAttribute.add("~LOp4vBIcMXdI[Tabular Update Date]");
		listMetaAttribute.add("~Yt9ZXpesiO40[Target-DBMS]");
		listMetaAttribute.add("~18MUjwdUBLJ0[Task State <Deprecated>]");
		listMetaAttribute.add("~usSVOytIL9ST[Technical json View Component Path]");
		listMetaAttribute.add("~ZuSsDlDXxK10[Technical password]");
		listMetaAttribute.add("~Oqypj1jl8b30[Technical Quality]");
		listMetaAttribute.add("~Xy1tKqDlQbkD[Test Case Code]");
		listMetaAttribute.add("~BtdSvNfqODUG[The average number of people <FTE> that worked on the code over the last 12 months?]");
		listMetaAttribute.add("~EtdSoFfqOPlE[The number of major releases delivered over the last 12 months?]");
		listMetaAttribute.add("~QtylMBjCIfFA[Tree Title]");
		listMetaAttribute.add("~yPCvFVU4Pzw0[Type Characteristics]");
		listMetaAttribute.add("~Zon)q8Gyte20[UML ActorName]");
		listMetaAttribute.add("~0XytF2kose20[UML ClassName]");
		listMetaAttribute.add("~utDLIK5Au000[UML ComponentName]");
		listMetaAttribute.add("~LXDonfCBsW10[UML EventName]");
		listMetaAttribute.add("~vXyt(2kosG30[UML PackageName]");
		listMetaAttribute.add("~Lon)f8GytS20[UML UseCaseName]");
		listMetaAttribute.add("~k0f6)p10)aI0[Update Permission <Read-Only Session>]");
		listMetaAttribute.add("~72f6hl10)W90[Update Permission <User Interface> <Deprecated>]");
		listMetaAttribute.add("~NcNHyaar7T30[UpgradeStatus]");
		listMetaAttribute.add("~aen5xX19OveL[URI]");
		listMetaAttribute.add("~len5xX19ObfL[URI Alias]");
		listMetaAttribute.add("~GnJ2QsGE9zz0[Used System Type]");
		listMetaAttribute.add("~u20000000X70[User Activation]");
		listMetaAttribute.add("~8XigFCyNE9VP[Value Value Label]");
		listMetaAttribute.add("~1JubVrPLAfG0[Variable Object Picture Identifier]");
		listMetaAttribute.add("~WVhmuCi15520[Variation Inheritance]");
		listMetaAttribute.add("~RifvvT()rO00[Version]");
		listMetaAttribute.add("~UhQ6VcZBLfaT[View Component Scope]");
		listMetaAttribute.add("~5qdSsIfqOLMF[What is the annual staff turnover within the development team?]");
		listMetaAttribute.add("~8rdS9FfqObfE[What is the application type]");
		listMetaAttribute.add("~SsdSPGfqOjvE[What is the approximate number of end users?]");
		listMetaAttribute.add("~BtdSlIfqO9JF[What is the average skill level of the development team on this type of application?]");
		listMetaAttribute.add("~KrdSPIfqO9DF[What is the CMMI level of the organization?]");
		listMetaAttribute.add("~2rdSzIfqOjPF[What is the percentage of change to the base code in the last 12 months?]");
		listMetaAttribute.add("~EsdScIfqOzFF[What percentage of the development effort has been spent on maintenance in the last 12 months?]");
		listMetaAttribute.add("~RejdHHw0Cz50[Workflow Engine Version]");
		listMetaAttribute.add("~qLaREvdOHLI7[Workflow Previous Status]");
		listMetaAttribute.add("~Dl)PstMpBvk0[Workflow Status Instance State]");
		listMetaAttribute.add("~udfd7NwTHjYO[Workspace Connection]");
		listMetaAttribute.add("~5eiWVIeqH9IK[Workspace History]");
		listMetaAttribute.add("~03rwGjU3z000[XPath]");
		listMetaAttribute.add("~I20000000950[MetaProtection]");
		listMetaAttribute.add("~UdHRDRN)4HD0[REPLACEMySQL]");
		listMetaAttribute.add("~F76((mmW9zL0[Computed Start Date]");
		listMetaAttribute.add("~H46(bnmW9fN0[Computed End Date]");
		return listMetaAttribute;

	}
	
	
	
	
}
