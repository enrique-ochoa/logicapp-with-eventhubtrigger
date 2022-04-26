# Overview
The purpose of this demo is to show how the Azure platform can address the requirement of obtaining and caching access tokens for a serverless process to access an API

# Requirement
An external processs will be constantly uploading files with business data to an storage account. Another process that obtains, from an Identity Provider (IdP), a valid access token to access an API is needed every time a record in those files is processed. One of the main goals of this demo is to avoid obtaining a new access token per record in those files. Instead, the idea is to cache access tokens for the time they are valid so new records in the files can reuse those tokens when making a call to the API

# Scope of this Demo
For this demo, Azure Active Directory (AAD) was chosen as an Identity Provider and Microsoft Graph as the API. The input files are manually uploaded to an Azure storage account. Then, a Logic Apps App is configured to run with a blob upload as a trigger and with two Azure functions as actions. The main goal of the first function is to get a valid access token and make it available for the second Logic Apps action (second function). The first function checks how close from expiration the cached access token is. If it's not close to expiration, the function will just get the access token from memory. Otherwise, a call to the IdP token generation service will be made. The second function will call the Microsoft Graph API using the access token obtained by the first function.

From an access token caching perspective, three options were evaluated to be implemented in this demo: IdP caching, Private in-memory caching, Distributed caching. Since we assumed that not all the IdPs have caching capabilities, we decided to avoid using this method. From the last two, we opted for Private in-memory caching but Distributed caching could have been a valid option to implement as well 

# Out of Scope
1. Input file data processing
2. Data conversion from file-based to json for API calls 

# Architecture

![image](https://user-images.githubusercontent.com/91332911/134840789-beb3a93c-fe13-4493-9738-1e22f9078b41.png)


# Implementation Pre-requisites
There are some dependencies that need to be in place before implementing the infrastructure and serverless code associated to this demo:
1. Access to an Azure Subscription is needed
2. An Azure Resource Group needs to be created. Ideally, this resource group should only be used to implement this demo 
3. The Client Application, in this case  the Azure Function App that contains the two functions, needs to be registered with the IdP, in this case Azure Active Directory 
4. The Client Application needs to be granted at least read access permissions/roles to the Microsoft Graph Service
5. In order to have the function app code available for deployment to Azure, Visual Studio or Visual Studio Code and related Azure Account, Azure Functions and the Azure CLI Tools extensions are needed
6. ARM templates include obfuscated subscription ids, resource groups, connection strings and identity ids. Those elements need to be updated before running the templates   

# Implementation Steps
In order to implement this demo in an Azure environment, the following are the steps that need to be taken:
1. Run the create-functionapp-and-dependencies.json ARM template using the "az deployment group create" command. An example of this command is:
   az deployment group create --name deployment --resource-group rg_test --template-file create-functionapp-and-dependencies.json
2. Deploy to Azure the function app code included in the functionapp folder
3. Implement the Azure Logic Apps workflow that is triggered when a blob file is uploaded or updated. arm_templates/logicapp_blobtrigger folder has some json files that can be taken as a reference when creating this Logic App in a new environment. An ARM template that creates the whole Logic App may be available soon
