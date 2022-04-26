# Overview
The purpose of this repo is to show how the Azure platform can address the requirement of processing events immediately (based on a push model) after receipt. Processing in this context involves events ingestion, access token generation and API request creation/submission. All of these capabilities are executed automatically (without manual intervention) in a serverless platform  

# Requirement
External processes will be constantly publishing events to an Event Hub instance. When a new event arrives, a Logic App workflow will be triggered. This workflow instance will execute a couple of steps in a sequential way. The first step (an http function) will obtain an access token from memory or from an Identity Provider. With that access token, the second step in the workflow process (another http function) will be triggered. This second step will invoke an API using the previously collected token

# Scope of this Demo
For this demo, Azure Active Directory (AAD) was chosen as an Identity Provider and Microsoft Graph as the API. The input events can be published to the Event Hub manually or using an Event Hub client. Then, a Logic Apps workflow is configured to run using an Event Hub connector as a trigger. Once an event is received, the Logic App workflow will run two Azure functions in a sequence. The main goal of the first function is to get a valid access token and make it available for the second Logic App workflow  action (second function). The first function checks how close from expiration the cached access token is. If it's not close to expiration, the function will just get the access token from memory. Otherwise, a call to the IdP token generation service will be made. The second function will call the Microsoft Graph API using the access token obtained by the first function.

From an access token caching perspective, three options were evaluated to be implemented in this demo: IdP caching, Private in-memory caching, Distributed caching. Since we assumed that not all the IdPs have caching capabilities, we decided to avoid using this method. From the last two, we opted for Private in-memory caching but Distributed caching could have been a valid option to implement as well 

# Out of Scope
1. Infrastructure creation like an Event Hub namespace or App Service Plan for Functions and Logic App. A simple/basic Event Hub instance needs to be manually created and the App Service Plan for Functions and Logic App can be created during deployment from VS Code
2. Event Hub Client. I assume this capability will be tested with a Kafka Client
3. Data conversion: from Event Hub to Graph API (event to json format) 

# Architecture

![image](https://user-images.githubusercontent.com/91332911/165359116-1c1db1d7-cc62-4bb4-ab5b-c96ede70c0de.png)


# Implementation Pre-requisites
There are some dependencies that need to be in place before implementing the infrastructure and serverless code associated to this demo:
1. Access to an Azure Subscription is needed
2. An Azure Resource Group needs to be created. Ideally, this resource group should only be used to implement this demo 
3. The Azure Function App that contains the two functions, needs to be registered as an application with the IdP, in this case Azure Active Directory 
4. The AAD application representing the Azure Function App, needs to be granted at least read access permissions/roles to the Microsoft Graph Service
5. In order to have the function app code available for deployment to Azure, Visual Studio or Visual Studio Code and related Azure Account, Azure Functions and the Azure CLI Tools extensions are needed
6. In order to have the Logic app code available for deployment to Azure, Visual Studio or Visual Studio Code and related Azure Account, Azure Logic App (Standard) and the Azure CLI Tools extensions are needed. Other pre-requisites to run the Logic App locally and deploy to Azure can be found here: https://docs.microsoft.com/en-us/azure/logic-apps/create-single-tenant-workflows-visual-studio-code 

# Implementation Steps
In order to implement this demo in an Azure environment, the following are the steps that need to be taken:
1. Create an Event Hub instance in a new resource group
2. Create Client Credentials for the Function App in AAD
3. Open and initialize the Function App in Visual Studio Code. Add AAD Client Credentials info to local/settings.json. Test it locally
4. Manually create a Function App (Basic or Standard) in Azure based on an App Service Plan. Set AAD Client Credentials as App Settings
5. Deploy VS Code Function App to Azure 
6. Open and initialize the Logic App (Standard) in Visual Studio Code. Test it locally. Deploy to Azure
