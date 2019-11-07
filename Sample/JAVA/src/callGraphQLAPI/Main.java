package callGraphQLAPI;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class Main {

	public static void main(String[] args) throws Exception {
		// TODO Auto-generated method stub
		System.out.println("Start ...");
		
		String charset = "UTF-8";
		
		String urlGraphQL = "https://s000002nug.labsmega.com/hopexgraphQL/api/ITPM";		
		String urlUAS = "https://s000002nug.labsmega.com/UAS/connect/token";		
		//String urlUAS = "http://192.168.131.155/UAS/connect/token";
		String grant_type = "password";
		String scope = "hopex openid read write";
		String username ="WebService";
		//String username ="Mega";
		String password = "Hopex";
		String client_id ="HopexAPI";
		String client_secret ="secret";
		String environmentId = "PLQLiNbUTfUN";
		String repositoryId = "wIhUR3hTTfBT";
		String ProfileId = "I9iCqGJg)u00";
		//String environmentId = "BOGcQcPcTD0G";
		
		AuthenticationUASBearer authenticationUASBearer = new AuthenticationUASBearer(charset, urlUAS, grant_type,scope,username, password,client_id,client_secret, environmentId);
		
		if (authenticationUASBearer.getisSuccess()) {
			String bearer = authenticationUASBearer.getAccessToken();
			//System.out.println("Success=" +bearer);	

			String graphQLQuery = "{\"query\":\"{application {id name }}\"}";
			
			
			ExecuteGraphQLQuery executeGraphQLQuery = new ExecuteGraphQLQuery(graphQLQuery, urlGraphQL, bearer, charset, environmentId, repositoryId, ProfileId);
			
			// if successful we have a JSON that contains the response
			if (executeGraphQLQuery.getisSuccess()) {
				JSONObject jsonResponse = executeGraphQLQuery.getjsonResponse();
				JSONObject data = jsonResponse.getJSONObject("data");
				
				// our GraphQL Query was requesting application
				JSONArray applicationArray = data.getJSONArray("application");				
				int length = applicationArray.length();				
				for(int i =0;i<length;i++) {
					JSONObject oApplication = applicationArray.getJSONObject(i);
					String name = oApplication.getString("name");
					String id = oApplication.getString("id");	
					System.out.println(name);
				}
			}			
		} else {
			System.out.println("Error=" + authenticationUASBearer.getStringResponse());
		}
		
	}

}
