package callGraphQLAPI;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.Reader;
import java.io.UnsupportedEncodingException;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.ProtocolException;
import java.net.URL;
import java.net.URLEncoder;
import org.json.*;


public class AuthenticationUASBearer {

	private String accessToken;
	private String tokenType;
	private int expiresIn;
	private String stringResponse;
	private JSONObject jsonResponse;
	
	private boolean isSuccess = false;
	
	public String getAccessToken(){
		return this.accessToken;
	}
	
	public String getTokenType(){
		return this.tokenType;
	}
	
	public int getExpiresIn(){
		return this.expiresIn;
	}
		
	public boolean getisSuccess() {
		return this.isSuccess;
	}
	
	public String getStringResponse() {
		return this.stringResponse;
	}
	
	public JSONObject getjsonResponse() {
		return this.jsonResponse;
	}


private void parseResponse(String response) {
	try {
		jsonResponse = new JSONObject(response.toString());
	
		if (jsonResponse.has("error")) {
			isSuccess = false;
		} else if (jsonResponse.has("access_token")) {
			this.accessToken = (String) jsonResponse.get("access_token");
			this.tokenType = (String) jsonResponse.get("token_type");
			this.expiresIn = (int) jsonResponse.get("expires_in");			
		}	
	} catch (JSONException e) {
		// TODO Auto-generated catch block
		e.printStackTrace();
	}	
}
		
private String bodyParameters(String charset, String urlUAS, String grant_type,String scope,String username, String password,String client_id,String client_secret, String environmentId) {

	String body = null;
	try {
		body = String.format("grant_type=%s&scope=%s&username=%s&password=%s&client_id=%s&client_secret=%s&environmentId=%s", 
		URLEncoder.encode(grant_type, charset), 
		URLEncoder.encode(scope, charset),
		URLEncoder.encode(username, charset),
		URLEncoder.encode(password, charset),
		URLEncoder.encode(client_id, charset),
		URLEncoder.encode(client_secret, charset),
		URLEncoder.encode(environmentId, charset)
				);
	} catch (UnsupportedEncodingException e) {
		// TODO Auto-generated catch block
		e.printStackTrace();
	}	
	
	return body;
}
	
	
public AuthenticationUASBearer(String charset, String urlUAS, String grant_type,String scope,String username, String password,String client_id,String client_secret, String environmentId) {

	try {
		
		String query = bodyParameters( charset,  urlUAS,  grant_type, scope, username,  password, client_id, client_secret,  environmentId);

		URL url;		
		
		url = new URL(urlUAS);

		HttpURLConnection connection = (HttpURLConnection) url.openConnection();
		connection.setRequestMethod("POST");				
		connection.setDoOutput(true);
		connection.setConnectTimeout(5000);
		connection.setReadTimeout(5000);		
		connection.setRequestProperty("Content-Type", "application/x-www-form-urlencoded;charset="+charset);

		//add the body parameter 
		if (query != null) {
			OutputStream output = connection.getOutputStream();
			output.write(query.getBytes(charset));
		}
		// execute the query and get the response code
		int responseCode  = connection.getResponseCode();
		
		Reader streamReader = null;
		// responseCode == HttpURLConnection.HTTP_OK
		if (responseCode  > 299) {
		    streamReader = new InputStreamReader(connection.getErrorStream());
			isSuccess = false;
		} else {
		    streamReader = new InputStreamReader(connection.getInputStream());
			isSuccess = true;
		}
		
		String readLine = null;
		BufferedReader in = new BufferedReader(streamReader);
		StringBuffer response = new StringBuffer();
		while ((readLine = in.readLine()) != null) {
			response.append(readLine);
		} in .close();

		stringResponse = response.toString();

		parseResponse(response.toString());
		
		/*
		System.out.println("URL=" + url.toString());
		System.out.println("query=" + query);
		System.out.println("status=" + responseCode);			
		System.out.println("jsonResponse="+ jsonResponse.toString());
		System.out.println("accessToken=" + accessToken);
		System.out.println("tokenType=" + tokenType);
		System.out.println("expiresIn="+expiresIn);
		 */		
		
		connection.disconnect();


	} catch (MalformedURLException e) {
		// TODO Auto-generated catch block
		e.printStackTrace();
	} catch (ProtocolException e) {
		// TODO Auto-generated catch block
		e.printStackTrace();
	} catch (IOException e) {
		// TODO Auto-generated catch block
		e.printStackTrace();
	}
	
		
	
	
} // creator
	
	
} // class
