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


public class ExecuteGraphQLQuery {

	private String stringResponse;
	private JSONObject jsonResponse;
	
	private boolean isSuccess = false;
		
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
	
		if (jsonResponse.has("errors")) {
			isSuccess = false;
		} else if (jsonResponse.has("data")) {
			//System.out.println("OK");
			isSuccess = true;
		}	
	} catch (JSONException e) {
		// TODO Auto-generated catch block
		e.printStackTrace();
	}	
}


public ExecuteGraphQLQuery(String graphQLQuery, String urlGraphQL, String bearer, String charset, String environmentId, String repositoryId, String ProfileId) {

	try {
		
		//String query = bodyParameters( environmentId);

		//System.out.println("start in graphQL execute");
		
		URL url;		
		
		url = new URL(urlGraphQL);

		//System.out.println("urlGraphQL=" + urlGraphQL);
		
		String hopexContext = "{\"EnvironmentId\":\""+environmentId+"\",\"RepositoryId\":\""+ repositoryId +"\",\"ProfileId\":\""+ProfileId+"\"}";

		
		HttpURLConnection connection = (HttpURLConnection) url.openConnection();
		connection.setRequestMethod("POST");				
		connection.setDoOutput(true);
		connection.setConnectTimeout(5000);
		connection.setReadTimeout(5000);		
		connection.setRequestProperty("X-HopexContext", hopexContext);
		connection.setRequestProperty("Authorization", "Bearer " + bearer);
		

		connection.setRequestProperty("Content-Type", "application/json");		
		
		//add the body parameter 	
		
		
		if (graphQLQuery != null) {
			OutputStream output = connection.getOutputStream();
			output.write(graphQLQuery.getBytes(charset));
		}
		
		//System.out.println("outputstream=" + connection.getOutputStream());		
		//System.out.println("contentType" + connection.getContentType());
		//System.out.println("getrequest method:" +connection.getRequestMethod());
	
		// execute the query and get the response code
		int responseCode  = connection.getResponseCode();
		
		//System.out.println("status=" + responseCode);
		
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
		//System.out.println(stringResponse);
		
		parseResponse(response.toString());

		
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
