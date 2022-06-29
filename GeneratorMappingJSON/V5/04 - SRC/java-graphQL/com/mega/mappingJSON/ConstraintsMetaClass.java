package com.mega.mappingJSON;

public class ConstraintsMetaClass {

	private boolean queryNode = true;
	private boolean nameSpace = false;
	private boolean readOnly = false;
	
	
	public void setQueryNode(boolean queryNode) {
		this.queryNode = queryNode;
	}
	
	public boolean getQueryNode() {
		return this.queryNode;
	}	

	public void setReadOnly(boolean readOnly) {
		this.readOnly = readOnly;
	}
	

	public boolean getReadOnly() {
		return this.readOnly;
	}		
	
	public void setNameSpace(boolean nameSpace) {
		this.nameSpace = nameSpace;
	}
	
	public boolean getNameSpace() {
		return this.nameSpace;
	}		
	
}
