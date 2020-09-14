package com.mega.generator;

public class SchemaToGenerate {

	private boolean included;
	private String schemaName;
	private String metaModelAbsoluteIdentifier;
	private String login;
	private String password;
	private String profile;
	
	public boolean getIncluded() {
		return included;
	}	
	public String getSchemaName() {
		return schemaName;
	}
	public String getMetaModelAbsoluteIdentifier() {
		return metaModelAbsoluteIdentifier;
	}	
	public String getLogin() {
		return login;
	}	
	public String getPassword() {
		return password;
	}
	public String getProfile() {
		return profile;
	}	
	
	public void setIncluded(boolean included) {
		this.included = included;
	}	
	public void setSchemaName(String schemaName) {
		this.schemaName = schemaName;
	}	
	public void setMetaModelAbsoluteIdentifier(String metaModelAbsoluteIdentifier) {
		this.metaModelAbsoluteIdentifier = metaModelAbsoluteIdentifier;
	}		
	public void setLogin(String login) {
		this.login = login;
	}		
	public void setPassword(String password) {
		this.password = password;
	}		
	public void setProfile(String profile) {
		this.profile = profile;
	}	
	
}
