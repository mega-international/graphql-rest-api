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
	
	@SuppressWarnings("unused")
	private MegaDatabase systemDB  = null;
	
	public OpenHOPEX(String sAdministrator, String sPassword, String openMode) {
		
		megaApplication = new MegaApplication();
		megaEnvironments = megaApplication.getEnvironments();
		megaEnvironment = megaEnvironments.next();

		

		
		
		megaDatabases = megaEnvironment.databases();
		systemDB = megaDatabases.next();
		data = megaDatabases.next();

		megaEnvironment.setCurrentAdministrator(sAdministrator);
		megaEnvironment.setCurrentPassword(sPassword);
		
		megaRoot = data.openEx(openMode);
	
		versionInformation = megaApplication.getVersionInformation();
		this.name = versionInformation.getName();
		this.version = versionInformation.getReleaseNumber() + "."+ versionInformation.getPatchNumber();

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
	
	
	public boolean CloseHOPEX() {
		//boolean isClosed = false;
	
		megaRoot.close();
		/*
		data.release();
		systemDB.release();
		megaDatabases.release();
		megaEnvironment.release();
		megaEnvironments.release();
		megaApplication.release();
		*/
		return megaRoot.isClosed();	
	}
	
}
