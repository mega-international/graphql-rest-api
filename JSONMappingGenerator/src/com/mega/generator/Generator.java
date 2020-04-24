package com.mega.generator;

import java.io.StringWriter;
import java.text.SimpleDateFormat;
import java.util.*;

import java.io.File;
import java.io.FileWriter;
import com.fasterxml.jackson.annotation.JsonInclude.Include;
import com.fasterxml.jackson.databind.*;
import com.mega.mappingJSON.*;
import com.mega.metamodelToJSON.MetaModel;
import com.mega.modeling.api.MegaRoot;

public class Generator {
	

	public static void main(String[] args) throws Exception{
	
//		OpenHOPEX openHOPEX = new OpenHOPEX("mega", "Hopex", "-User:Mega -Role:Enterprise Architect -TranType:Micro -OpenMode:R");
	
	    System.out.println("########### Starting ###########");		

		HashMap<String,String> overRideNameList = null;
		String rootMetaModel = "";
		String fileName = "";
		String fileNameOverride="00_OverrideName_Global.JSON";
		String filePath = "C:\\\\temp\\\\java\\\\";	
		boolean isClosed = false;
		MegaRoot megaRoot;		
		OpenHOPEX openHOPEX = null;	

		

		Calendar cal = Calendar.getInstance();
		
		SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
		String latestGeneration = format.format(cal.getTime());
		
		String year = "" + cal.get(Calendar.YEAR);
		year = year.substring(2, 4);
		
		//String month ="" + (cal.get(Calendar.MONTH)+1);		
		String day = ""+ cal.get(Calendar.DAY_OF_MONTH);
		
		if (day.length()<2) {
			day = "0"+ day;
		}
			
		

		
		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
 //	
			
		System.out.println("Starting ITPM");
		System.out.println("Hopex Open");				
		openHOPEX = new OpenHOPEX("Tibere", "Hopex", "-Role:ITPM Functional Administrator -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();
	
		rootMetaModel = "~TeEKeRMmSPYK[00 - Webservice extract ITPM]";
		fileName = "ITPM.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);
			
		// 
		  		
		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
		
 		System.out.println("Starting BPA");
		System.out.println("Hopex Open");				
		openHOPEX = new OpenHOPEX("Frank", "Hopex", "-Role:Process Functional Administrator -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();
		
		rootMetaModel = "~KdLTVAz5T5BS[00 - Webservice extract BPA]";
		fileName = "BPA.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);
		
		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
 		
		System.out.println("Starting Data");
		System.out.println("Hopex Open");				
//		openHOPEX = new OpenHOPEX("Lina", "Hopex", "-Role:IA Functional Administrator -TranType:Micro -OpenMode:R");
		openHOPEX = new OpenHOPEX("Mega", "Hopex", "-Role:Hopex Customizer -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();

		rootMetaModel = "~7JBWOI9bUPqB[ extract Data]";
		fileName = "Data.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);

		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
 		
		System.out.println("Starting Assessment");
		System.out.println("Hopex Open");				
		openHOPEX = new OpenHOPEX("Mega", "Hopex", "-Role:Hopex Customizer -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();

		rootMetaModel = "~A4mYhmucTbeH[00 - Webservice extract Assessment]";
		fileName = "Assessment.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);		
	
		
		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
 	
		System.out.println("Starting Audit");
		System.out.println("Hopex Open");				
		openHOPEX = new OpenHOPEX("Ernesto", "Hopex", "-Role:Audit Functional Administrator -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();
		
		rootMetaModel = "~bcLTwMt5THDO[00 - Webservice extract Audit]";
		fileName = "Audit.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);
	
		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
 		
		System.out.println("Starting Privacy");
		System.out.println("Hopex Open");				
		openHOPEX = new OpenHOPEX("Olivier", "Hopex", "-Role:Functional Administrator (GDPR) -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();

		rootMetaModel = "~igEKBLUkSjkG[00 - Webservice extract GDPR]";	
		fileName = "DataPrivacy.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);
			
		
		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
 		
		System.out.println("Starting Risk");
		System.out.println("Hopex Open");				
		openHOPEX = new OpenHOPEX("Jude", "Hopex", "-Role:Incidents and Losses Administrator -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();

		rootMetaModel = "~NfEKpIDmSDpH[00 - Webservice extract Incident]";
		fileName = "Risk.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);
		
		
		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
 	
		System.out.println("Starting MetaModel");
		System.out.println("Hopex Open");				
		openHOPEX = new OpenHOPEX("mega", "Hopex", "-Role:Enterprise Architect -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();

		rootMetaModel = "~XSTxVyCQTrMF[xtract MetaModel]";
		fileName = "MetaModel.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);

		
		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
 	
		System.out.println("Starting Workflow");
		System.out.println("Hopex Open");				
		openHOPEX = new OpenHOPEX("mega", "Hopex", "-Role:Enterprise Architect -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();

		rootMetaModel = "~882gHmQVUXbJ[xtracat Workflow]";
		fileName = "Workflow.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);

		// ***********************************************************
		// ***********************************************************
		// ***********************************************************
 	
		System.out.println("Starting Reporting");
		System.out.println("Hopex Open");				
		openHOPEX = new OpenHOPEX("mega", "Hopex", "-Role:Enterprise Architect -TranType:Micro -OpenMode:R");
		megaRoot = openHOPEX.getMegaRoot();

		rootMetaModel = "~J82geYQVU9EF[xtract Reporting]";
		fileName = "Reporting.JSON";
		overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,rootMetaModel,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);

		isClosed = openHOPEX.CloseHOPEX();
		System.out.println("hopex closed:"+isClosed);		
			
		//
	    System.out.println("########### All Done ###########");
    
	}


	private static void readwriteOverRideName(String filePath,  HashMap<String,String> overRideNameList, boolean write) {

		if (write) {
			System.out.println("     write OverRideName JSON");
		} else 
		{
			System.out.println("     read OverRideName JSON");			
		}
		
		try {
			//create ObjectMapper instance
			ObjectMapper objectMapper = new ObjectMapper();	
			objectMapper.configure(SerializationFeature.INDENT_OUTPUT, true);		
			objectMapper.setSerializationInclusion(Include.NON_NULL);		
			
			File file = new File(filePath);
			StringWriter stringEmp = new StringWriter();
			
			if (!file.exists()) {
				// create a template file in case it does not exist
		    	file.createNewFile();
				FileWriter fileWriter=new FileWriter(file);				
				stringEmp.append("{");
				//	stringEmp.append("\"metaClassID\" : \"MetaClass to rename\",");
				//	stringEmp.append("\"metaAssociationID_metaClassID\" : \"Way to build the key for a simple path\",");
				//	stringEmp.append("\"metaAttributeID\" : \"MetaAttribute to rename\",");
				//stringEmp.append("\"metaAssociationID1_metaClassID1_metaAssociationID2_metaClassID2\" : \"Way to build the key for a 2 step path\"");
				stringEmp.append("}");
				fileWriter.write(stringEmp.toString());
				fileWriter.close();	
			}
	

			if (write) {
				objectMapper.writeValue(stringEmp, overRideNameList);				
				FileWriter fileWriter=new FileWriter(file);	
				fileWriter.write(stringEmp.toString());
				fileWriter.close();			
				
			} else {
				@SuppressWarnings("unchecked")
				HashMap<String, String> localHashmap = objectMapper.readValue(file, HashMap.class);
							
				overRideNameList.putAll(localHashmap);
			}
			
			
		} catch (Exception e) {
			e.printStackTrace();
		}
	}
	
	private static void generateJSON(MegaRoot megaRoot,String rootMetaModel,String filePath,HashMap<String,String> overRideNameList, String name, String version,String latestGeneration) {

		System.out.println("     creating JSON... ");			

		MetaModel metaModel = new MetaModel(megaRoot,rootMetaModel,overRideNameList);
		
		RootJSON rootJSON = metaModel.generateJSON(name,version,latestGeneration);
		
		System.out.println("     mapper ready :" + rootMetaModel);			
		
		//create ObjectMapper instance
		ObjectMapper objectMapper = new ObjectMapper();
		
		//configure Object mapper for pretty print
		objectMapper.configure(SerializationFeature.INDENT_OUTPUT, true);		
		
		//writing to console, can write to any output stream such as file
		StringWriter stringEmp = new StringWriter();
		
		objectMapper.setSerializationInclusion(Include.NON_NULL);		
		
		try {
			objectMapper.writeValue(stringEmp, rootJSON);
		} catch (Exception e) {
			e.printStackTrace();
		}
		//System.out.println(stringEmp);	

		System.out.println("     writing file :" + filePath);			
		
	    try {
			File file = new File(filePath);
	    	file.createNewFile();
			FileWriter fileWriter=new FileWriter(file);		
			fileWriter.write(stringEmp.toString());
			fileWriter.close();			
			
	    	//FileInputStream fileInputStream = new FileInputStream(file);
			
		} catch (Exception e) {
			e.printStackTrace();
		}			
		
	}
	

}
