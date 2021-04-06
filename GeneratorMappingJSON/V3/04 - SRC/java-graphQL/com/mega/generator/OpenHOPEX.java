package com.mega.generator;

import com.mega.modeling.api.*;
import com.mega.modeling.api.MegaSite.VersionInformation;

public class OpenHOPEX {

	private MegaRoot megaRoot = null; 
	private MegaDatabase data  = null;
	private MegaApplication megaApplication = null;
	private MegaEnvironments megaEnvironments = null;
	private MegaEnvironment megaEnvironment = null; 
	private MegaDatabases megaDatabases  = null;
	private VersionInformation versionInformation = null;
	
	private String version = null;
	private String name = null;
	

	//private MegaDatabase systemDB  = null;
	
	public OpenHOPEX(String sAdministrator, String sPassword, String openMode) throws Exception {

		Generator.logger.info("Open HOPEX ");
		
		
		megaApplication = new MegaApplication();

		Generator.logger.fine("new MegaApplication success ");
	
		megaEnvironments = megaApplication.getEnvironments();
		megaEnvironment = megaEnvironments.get(Arguments.getEnvironment());
		
		
	//	megaEnvironment.is
		

		Generator.logger.fine("Mega Environment = " + megaEnvironment.getName());
	
		megaDatabases = megaEnvironment.databases();

		megaDatabases.get(Arguments.getRepository());
		
		//systemDB = megaDatabases.next();
		//Generator.logger.fine("Getting Repository SystemDb = " + systemDB.getName());
		data = megaDatabases.next();
		Generator.logger.fine("Getting Repository Data = " + data.getName());
		
		
		Generator.logger.fine("Hopex setCurrentAdministrator = " + sAdministrator);
		Generator.logger.fine("Hopex setCurrentPassword = " + sPassword);
		
		megaEnvironment.setCurrentAdministrator(sAdministrator);
		megaEnvironment.setCurrentPassword(sPassword);
		Generator.logger.info("Open Session ");
		Generator.logger.info("sAdministrator: " + sAdministrator + "");
		
		megaRoot = data.openEx(openMode);
	
		versionInformation = megaApplication.getVersionInformation();
		this.name = versionInformation.getName();
		this.version = versionInformation.getReleaseNumber() + "."+ versionInformation.getPatchNumber();

		Generator.logger.fine("Version = " + this.version);

		String languageName = megaRoot.currentEnvironment().getCurrentLanguageName();
		
		Generator.logger.info("Language Name = " + languageName);
		
		String languageIdStr = megaRoot.currentEnvironment().toolkit().getString64FromID(megaRoot.currentEnvironment().getCurrentLanguageID());

		Generator.logger.fine("Language ID = " + languageIdStr);
		
		if (!languageIdStr.equals("00(6wlHmk400")) {
			throw new Exception("Expected language English (00(6wlHmk400) current value :" + languageName + " (ID = " + languageIdStr + ")"); 
		}

		
	}
	
	public String getName() {
		return this.name;
	}	
	public String getVersion() {
		return this.version;
	}
	
	public MegaRoot getMegaRoot() {
		return megaRoot;
	}
	
	
	public void closeHOPEX() {
		data  = null;
		megaApplication = null;
		megaEnvironments = null;
		megaEnvironment = null; 
		megaDatabases  = null;
		versionInformation = null;		
		
		if (!megaRoot.isClosed()) {
			megaRoot.close();
			megaRoot = null;
		}
		Generator.logger.info("HOPEX Closed ");	
	}
	
}
