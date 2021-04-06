package com.mega.mappingJSON;

public class ConstraintsProperties {
	
	private String type;
	private boolean mandatory;
	private String maxLength;
	private boolean readOnly;
//	private boolean filter;	
	private boolean translatable;
	private boolean formattedText;
	private boolean unique;

	
	/*		
	public ConstraintsProperties() {
		
	}

	public ConstraintsProperties(String type, boolean mandatory, String maxLength, boolean readOnly, boolean filter, boolean translatable, boolean formattedText ) {
		this.type = type;
		this.mandatory =mandatory;
		this.maxLength = maxLength;
		this.readOnly = readOnly;
		this.filter = filter;
		this.translatable = translatable;
		this.formattedText = formattedText;
	}
*/

	
	public void setType (String type) {
		this.type =type;
	}
	
	public String getType() {
		return this.type;
	}
	
	public void setMandatory(boolean mandatory) {
		this.mandatory =mandatory;
	}
	
	public boolean getMandatory() {
		return this.mandatory;
	}
	
	public void setMaxLength (String maxLength) {
		this.maxLength = maxLength;
	}
	
	public String getMaxLength() {
		return this.maxLength;
	}

	public void setReadOnly(boolean readOnly) {
		this.readOnly = readOnly;
	}
	

	public boolean getReadOnly() {
		return this.readOnly;
	}	
/*	
	public void setFilter(boolean filter) {
		this.filter = filter;
	}
		
	
	public boolean getFilter() {
		return this.filter;
	}
	*/
	public void setTranslatable (boolean translatable) {
		this.translatable =translatable;
	}
	
	public boolean getTranslatable() {
		return this.translatable;
	}	

	public void setFormattedText (boolean formattedText) {
		this.formattedText =formattedText;
	}
	
	public boolean getFormattedText() {
		return this.formattedText;
	}		
	
	public void setUnique(boolean unique) {
		this.unique =unique;
	}
	
	public boolean getUnique() {
		return this.unique;
	}	
	
}
